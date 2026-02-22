using System;
using System.Collections;
using UnityEngine;

public class Movement : MonoBehaviour
{
    //=====PLAYER STATES==========
    public bool isDead = false;

    // ===============BASIC MOVEMENT==========================
    private float horizontal; // Current horizontal input (-1..1)
    private float speed = 8f; // Movement speed 
    private float jumpingPower = 16f; // Initial upward velocity when jumping
    private bool isFacingRight = true; // Which direction the sprite is facing

    // ===== GRAVITY & FALLING =====
    public float originalGravity = 5; // Default gravity scale used when not dashing
    private float stopDash = 0.02f; // Short delay used when cancelling a dash before resetting flags
    private float maxFallSpeed = 50f; // Hard cap on falling speed to avoid extreme velocities

    //=============COYOTE TIME===================
    private float originalCoyoteTime = 0.1f; // How long after leaving ground player can still jump
    private float coyoteTime; // Current coyote time remaining
    private bool isJumping = false; // True during a jump input window
    //=====================MOVEMENT SETTINGS======================
    private float MaxDashes = 1; // Maximum dashes allowed before touching ground
    private float NumDashes = 1; // Current remaining dashes
    public bool canDash = true; // Whether dashing is available (cooldown gating)
    private bool isDashing; // True while a dash coroutine is active
    private float dashingCooldown = 0.5f; // Additional cooldown after dash finishes

    private float dashingPower = 30f; // Horizontal velocity applied during dashes

    // dashingTime equals time dashing in seconds
    private const float dashingTime = 0.1f;
    // dashGravity = gravity scale applied during dash (affects vertical component)
    private const float dashGravity = 2;
    // dashHeight used to set vertical velocity during directional dashes
    private const float dashHeight = 0;
    private const float fallGravityMultiplier = 2.5f;
    private const float lowJumpGravityMultiplier = 2f;


    private bool isWallSliding; // True when the player is attached to a wall and sliding down
    private float wallSlidingSpeed = 2f; // Maximum downward speed while sliding on a wall

    private bool isWallJumping; // True while performing a wall jump and prevented from normal control
    private float originalWallJumpTime = 0.5f; // Reset value for wall jump time tracking
    public float wallJumpTime; // Current wall jump timer
    private float wallJumpingDirection; // Direction to push player during wall jump (sign)
    // CT = Coyote Time for wall jump: short buffer to still wall-jump after leaving wall
    private float wallJumpingCT = 0.2f;
    private float wallJumpingCounter; // Timer for the wall-jump coyote window
    private float wallJumpingDuration = 0.1f; // Short duration used to limit wall-jump effect
    private Vector2 wallJumpingPower = new Vector2(32f, 16f); // (horizontal, vertical) impulse for wall jump

    // Serialized references set in inspector
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private TrailRenderer tr;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;

    void Start()
    {
        tr.emitting = false; // Ensure trail isn't emitting on start
    }

    private void Update()
    {
        // If player is dead, prevent dashing
        if (isDead)
        {
            isDashing = false;
        }

        // Manage wall jump timing: count down while wall jumping
        if (isWallJumping)
        {
            wallJumpTime -= Time.deltaTime;
            if (wallJumpTime <= 0)
            {
                isWallJumping = false; // end wall-jump state when timer expires
            }
        }
        else
        {
            wallJumpTime = originalWallJumpTime; // reset value when not wall-jumping
        }



        // If currently wall-jumping, skip the rest of Update (locks out normal input)
        if (isWallJumping)
        {
            return;
        }

        // Jump input: allows jumping while inside coyote time
        if (Input.GetButton("Jump") && coyoteTime > 0)
        {
            isJumping = true;
            coyoteTime = 0f; // consume coyote time

            if (isDashing)
            {
                // if dashing, cancel dash to allow upward motion
                StartCoroutine(CancelDash());
            }
            else
            {
                // Normal jump applies upward velocity directly
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
            }
        }

        // Short hop behavior: releasing jump early reduces vertical velocity
        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0f && !isDashing)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.25f);
        }

        // If grounded, reset coyote time, dash count, and some flags
        if (IsGrounded())
        {
            coyoteTime = originalCoyoteTime;
            NumDashes = MaxDashes;
            isJumping = false;
        }
        else
        {
            coyoteTime -= Time.deltaTime;
        }

        // If dashing, prevent other movement processing and early-return after clearing jumping flag
        if (isDashing)
        {
            isJumping = false;
            return;
        }

        // If wall sliding, keep default gravity
        if (isWallSliding)
        {
            // Default gravity if wall sliding
            rb.gravityScale = originalGravity;
        }

        // Disable trail by default; individual dash coroutines enable it when active
        tr.emitting = false;

        // Falling: cap max fall speed and increase gravity a bit while falling

        // Caps maximum fall speed to avoid excessive acceleration
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -maxFallSpeed));

        // Read horizontal input each frame
        horizontal = Input.GetAxisRaw("Horizontal");

        // Dashing input: Fire1 button triggers a dash; vertical input selects dash type
        if (Input.GetButton("Fire1") && canDash && NumDashes > 0)
        {
            if(IsGrounded())    StartCoroutine(Dash(0));
            else                StartCoroutine(Dash(1));

            return; // dash started — skip the rest of Update this frame
        }

        // Wall interactions
        WallSlide();
        WallJump();

        // Flip sprite according to movement unless currently wall-jumping
        if (!isWallJumping)
        {
            Flip();
        }
        //gravity modifing 
        if (isDashing)
        {
            //dont do anything
        }
        else if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = originalGravity * fallGravityMultiplier;
        }
        else if (rb.linearVelocityY > 0 && !Input.GetButton("Jump"))
        {
            rb.gravityScale = originalGravity * lowJumpGravityMultiplier;
        }
        else
        {
            rb.gravityScale = originalGravity;
        }
    }

    private void FixedUpdate()
    {
        // When dashing, physics behavior is handled by dash coroutine; skip regular movement
        if (isDashing) return;
        // The next block computes horizontal velocity handling differently based on current direction
        if (Math.Abs(horizontal) > 0.01f)
        {
            float targetSpeed = horizontal * speed;
            float acceleration;
            if (IsGrounded() && rb.linearVelocityX > speed) { acceleration = 200; }
            else if (rb.linearVelocityX > speed) { acceleration = 100; }
            else if (IsGrounded() && rb.linearVelocityX > speed) { acceleration = 60; }
            else acceleration = 40;
            float newXVelosity = Mathf.MoveTowards(rb.linearVelocityX, targetSpeed, acceleration * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(newXVelosity, rb.linearVelocityY);
        }
        else
        {
            float targetSpeed = 0;
            float acceleration = 70f;
            float newXVelosity = Mathf.MoveTowards(rb.linearVelocityX, targetSpeed, acceleration * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(newXVelosity, rb.linearVelocityY);
        }
    }

    // Ground-check helper using a small circle overlap
    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    // Wall-check helper using a small circle overlap
    private bool IsWalled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }

    // Manage wall sliding: activates when touching wall, not grounded, and holding horizontal input
    private void WallSlide()
    {
        if (IsWalled() && !IsGrounded() && horizontal != 0f)
        {
            isWallSliding = true;
            // Clamp the downward velocity so wall slide is slower and controllable
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }

    // Handle wall-jump logic including the short coyote window
    private void WallJump()
    {
        if (isWallSliding)
        {
            // Set direction to opposite of current localScale.x so we jump away from the wall
            wallJumpingDirection = -transform.localScale.x;
            wallJumpingCounter = wallJumpingCT; // give a brief window to trigger wall-jump
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime; // count down the window when not sliding
        }

        if (Input.GetButtonDown("Jump") && wallJumpingCounter > 0f)
        {
            isWallJumping = true;
            // Apply directional impulse for wall jump (horizontal * and vertical)
            rb.linearVelocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
            wallJumpingCounter = 0f;

            // If the player sprite isn't facing the direction we jumped, flip it
            if (transform.localScale.x != wallJumpingDirection)
            {
                isFacingRight = !isFacingRight;
                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }
        }
    }

    // Flip sprite horizontally based on movement input
    private void Flip()
    {
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            Vector3 localScale = transform.localScale;
            isFacingRight = !isFacingRight;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    // Up/forward dash coroutine: sets velocities, gravity, and trail, then resets after time
    private IEnumerator Dash(float verticalDirection)
    {
        NumDashes -= 1;
        canDash = false;
        coyoteTime = 0;
        isDashing = true;
        rb.gravityScale = -verticalDirection*dashGravity;
        tr.emitting = true;
        float direction = -Math.Abs(transform.localScale.y);
        if (isFacingRight)
        {
            direction = 1;
        }
        rb.linearVelocity = new Vector2(direction * dashingPower, transform.localScale.y * dashHeight);
        yield return new WaitForSeconds(dashingTime);
        if (isDashing == true)
        {
            isDashing = false;
            //After dash, zero horizontal velocity and reduce vertical to smooth transition
            rb.gravityScale = originalGravity;
        }
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }



    // CancelDash is used when the player presses jump during a dash to interrupt it
    private IEnumerator CancelDash()
    {
        // Apply upward velocity scaled down from jumpingPower for a consistent jump feel
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower / 1.25f);
        yield return new WaitForSeconds(stopDash); // short delay to let jump take effect
        isDashing = false;
        rb.gravityScale = originalGravity;
    }
}