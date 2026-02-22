using System;
using System.Collections;
using UnityEngine;

public class Movement : MonoBehaviour
{
    //=====PLAYER STATES==========
    public bool isDead = false; 

    // ===============BASIC MOVEMENT==========================
    private float horizontal; // Current horizontal input [-1,1]
    private float speed = 8f; // Movement speed 
    private float jumpingPower = 16f; // Initial upward velocity when jumping
    private bool isFacingRight = true; // Which direction the sprite is facing

    // ===== GRAVITY & FALLING =====
    public float originalGravity = 5; // Default gravity scale used when not dashing
    private float stopDash = 0.02f; // Short delay used when cancelling a dash before resetting flags
    private float maxFallSpeed = 50f; // Hard cap on falling speed to avoid extreme velocities

    //=============COYOTE TIME===================
    private float originalCoyoteTime = 0.2f; // How long after leaving ground player can still jump
    private float coyoteTime; // Current coyote time remaining
    private bool isJumping = false; // True during a jump input window
    //=====================MOVEMENT SETTINGS======================
    private float MaxDashes = 1; // Maximum dashes allowed before touching ground
    private float NumDashes = 1; // Current remaining dashes
    public bool canDash = true; // Whether dashing is available (cooldown gating)
    private bool isDashing; // True while a dash coroutine is active
    private float dashingCooldown = 0.5f; // Additional cooldown after dash finishes

    private float dashingPower = 20f; // Horizontal velocity applied during dashes

    // dashingTime equals time dashing in seconds
    private const float dashingTime = 0.3f;
    // dashGravity = gravity scale applied during dash (affects vertical component)
    private const float dashGravity = 2f;
    // dashHeight used to set vertical velocity during directional dashes
    private const float dashHeight = 0;//dashGravity / (2 * dashingTime);


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
        // If player is dead, reset state to avoid dashing inbetween lives
        if (isDead)
        {
            isDashing = false;
        }

        // While in the air, decrement coyote time
        if (!IsGrounded())
        {
            coyoteTime -= Time.deltaTime;
        }

        if (isWallJumping) {
            // Counts down and stops walljump when timer ends
            wallJumpTime -= Time.deltaTime; 
            if (wallJumpTime <= 0) {
                isWallJumping = false;
            }
            return; // skip the rest of Update while wall-jumping to prevent input interference
        }
        wallJumpTime = originalWallJumpTime; // reset value when not wall-jumping

        // Jump input: allows jumping while inside coyote time
        if (Input.GetButton("Jump") && coyoteTime > 0)
        {
            isJumping = true;
            coyoteTime = 0f; // Prevent extra jumps in same coyote time
            
            if (isDashing)
            {
                StartCoroutine(CancelDash()); //Starts "wave dash" process by cancelling dash prematurely
                return;
            }
            else
            {
                // Normal jump applies upward velocity directly
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
            }
        }

        // Short hop behavior: releasing jump early reduces vertical velocity, better control
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
            if (!isDashing)
            {
                // Default gravity if standing on a platform
                rb.gravityScale = originalGravity;
            }
        }

        // If dashing, prevent other movement processing and early-return after clearing jumping flag
        if (isDashing)
        {
            isJumping = false;
            return;
        }

        

        // Disable trail by default; individual dash coroutines enable it when active
        tr.emitting = false;

        // Falling: cap max fall speed and increase gravity a bit while falling
        if (rb.linearVelocity.y < 0)
        {
            // Caps maximum fall speed to avoid excessive acceleration
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -maxFallSpeed));
            if (rb.gravityScale == originalGravity) {
                // add some extra gravity while falling to make the fall feel snappier
                rb.gravityScale += rb.gravityScale / 2;
            }
        }

        // Read horizontal input each frame
        horizontal = Input.GetAxisRaw("Horizontal");

        // Dashing input: Fire1 button triggers a dash; vertical input selects dash type
        if (Input.GetButton("Fire1") && canDash && NumDashes > 0)
        {
            StartCoroutine(Dash(Input.GetAxisRaw("Vertical")));
            
            return; // dash started — skip the rest of Update this frame
        }

        // Wall interactions
        WallSlide();
        WallJump();
        if (isWallSliding)
        {
            rb.gravityScale = originalGravity; // Default gravity if wall sliding
        }
        // Flip sprite according to movement
        Flip();
    }

    private void FixedUpdate()
    {
        // When dashing, physics behavior is handled by dash coroutine; skip regular movement
        if (isDashing)
        {
            return;
        }

        // Using a double here to preserve some math behavior, then apply to rigidbody
        double velocity = rb.linearVelocity.x;
        float airDrag = speed;

        // The next block computes horizontal velocity handling differently based on current direction
        bool velocityRight = true;
        if (velocity < 0)
        {
            // Work with absolute value for comparisons
            velocity = -1 * velocity;
        }
        if (horizontal == 0) {
            airDrag = speed / 15; // small drag when no input in air
        } else
        {
            airDrag = 0; // removing drag when input opposes current velocity (quick turn)
        }
        if (velocity > (speed))
        {
            if (IsGrounded())
            {
                // If moving faster than speed on ground, reduce speed (friction-ish)
                rb.linearVelocity = new Vector2((rb.linearVelocity.x / 1.5f), rb.linearVelocity.y);
            }
            else
            {
                // In air while fast, add input and air drag to gradually change velocity
                if (velocityRight)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x + (horizontal * speed) - airDrag, rb.linearVelocity.y);
                }
                else
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x + (horizontal * speed) + airDrag, rb.linearVelocity.y);
                }
            }
        }
        else
        {
            // Normal movement: set horizontal velocity according to input
            rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);
        }
        airDrag = speed; // reset local var (not strictly necessary, keeps behavior consistent)
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
            return;
        }
        isWallSliding = false;
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
        isDashing = true;
        //IDK why i need a negative here however its fliped otherwise if it ever breaks than thats probaply why
        rb.gravityScale = -verticalDirection*dashGravity; // reduce gravity while dashing upward/forward
        tr.emitting = true;
        float direction = -Math.Abs(transform.localScale.y);
        if(isFacingRight)
        {
            direction = 1;
        }
        if (rb.linearVelocity.x > speed)
        {
            rb.linearVelocity = new Vector2((rb.linearVelocity.x - speed) + direction * dashingPower, transform.localScale.y * dashHeight);
        } else if (rb.linearVelocity.x < (-1 * speed))
        {
            rb.linearVelocity = new Vector2((rb.linearVelocity.x + speed) + direction * dashingPower, transform.localScale.y * dashHeight);
        } else
        {
            rb.linearVelocity = new Vector2(direction * dashingPower, transform.localScale.y * dashHeight);
        }
        yield return new WaitForSeconds(dashingTime);
        if (isDashing == true)
        {
            isDashing = false;
            // After dash, zero horizontal velocity and reduce vertical to smooth transition
            rb.linearVelocity = new Vector2(0f, (rb.linearVelocity.y / 1.5f));
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