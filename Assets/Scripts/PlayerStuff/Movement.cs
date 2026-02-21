using System.Collections;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public bool isDead = false;


    private float horizontal;
    private float speed = 8f;
    private float jumpingPower = 16f;
    private bool isFacingRight = true;
    public float originalGravity = 5;
    private float stopDash = 0.02f;
    private float maxFallSpeed = 50f;
    private float originalCoyoteTime = 0.2f;
    private float coyoteTime;
    private bool isJumping = false;

    private float MaxDashes = 1;
    private float NumDashes = 1;
    public bool canDash = true;
    private bool isDashing;
    private float dashingCooldown = 0f;

    private float dashingPower = 40f;

    //dashingTime equals time dashing in seconds
    private const float dashingTime = 0.3f;
    //dashGravity = gravity, affecting how high the dash goes
    private const float dashGravity = 7f;
    private const float dashHeight = dashGravity / (2 * dashingTime);


    private bool isWallSliding;
    private float wallSlidingSpeed = 2f;

    private bool isWallJumping;
    private float originalWallJumpTime = 0.5f;
    public float wallJumpTime;
    private float wallJumpingDirection;
    //CT = Coyote Time
    private float wallJumpingCT = 0.2f;
    private float wallJumpingCounter;
    private float wallJumpingDuration = 0.1f;
    private Vector2 wallJumpingPower = new Vector2(32f, 16f);

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private TrailRenderer tr;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;

    void Start()
    {
        tr.emitting = false;

    }
    private void Update()
    {
        if (isDead)
        {
        isDashing = false;
        }
        if (isWallJumping)
        {
            wallJumpTime -= Time.deltaTime;
            if (wallJumpTime <= 0) {
                isWallJumping = false;
            }
        } else
        {
            wallJumpTime = originalWallJumpTime;
        }
        if (!IsGrounded())
        {
            coyoteTime -= Time.deltaTime;
        }
        if (isWallJumping) {
            return;
        }
        if (Input.GetButton("Jump") && coyoteTime > 0)
        {
            isJumping = true;
            coyoteTime = 0f;
            
            if (isDashing)
            {
                StartCoroutine(CancelDash());
            }
            else
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
            }
        }
        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0f && !isDashing)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.25f);
        }

        if (IsGrounded())
        {
            coyoteTime = originalCoyoteTime;
            NumDashes = MaxDashes;
            isJumping = false;
            if (!isDashing)
            {
                //Default gravity if standing on a platform
                rb.gravityScale = originalGravity;
            }
        }
        if (isDashing)
        {
            isJumping = false;
            return;
        }
        if (isWallSliding)
        {
                //Default gravity if wall sliding
                rb.gravityScale = originalGravity;
        }
        
        tr.emitting = false;
        if (rb.linearVelocity.y < 0)
        {
            //Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -maxFallSpeed));
            if (rb.gravityScale == originalGravity) {
                rb.gravityScale += rb.gravityScale / 2;
            }
        }
        horizontal = Input.GetAxisRaw("Horizontal");
        if (Input.GetButton("Fire1") && canDash && NumDashes > 0)
        {
            coyoteTime = 0;
            if (Input.GetButton("Vertical"))
            {
                if (IsGrounded())
                {
                    StartCoroutine(GroundDash());
                }
                else
                {
                    StartCoroutine(DownDash());
                }
            }
            else
            {
                StartCoroutine(UpDash());
            }
            return;

        }
       
        WallSlide();
        WallJump();

        if (!isWallJumping)
        {
            Flip();
        }
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            return;
        }
        double velocity = rb.linearVelocity.x;
        float airDrag = speed;
        
        if (velocity < 0)
        {
            velocity = -1 * velocity;
            if (!(horizontal != 0)) {
                airDrag = speed / 15;
            }
            if (horizontal > 0)
            {
                airDrag = 0;
            }
            if (velocity > (speed))
            {
                if (IsGrounded())
                {
                    rb.linearVelocity = new Vector2((rb.linearVelocity.x / 1.5f), rb.linearVelocity.y);
                }
                else
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x + (horizontal * speed) + airDrag, rb.linearVelocity.y);
                }
            }
            else
            {
                rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);
            }
        } else
        {
            if (!(horizontal != 0)) {
                airDrag = speed / 15;
            }
            if (horizontal < 0)
            {
                airDrag = 0;
            }
            if (velocity > (speed))
            {
                if (IsGrounded())
                {
                    rb.linearVelocity = new Vector2((rb.linearVelocity.x / 1.5f), rb.linearVelocity.y);
                }
                else
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x + (horizontal * speed) - airDrag, rb.linearVelocity.y);
                }
            }
            else
            {
                rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);
            }
        }
        airDrag = speed;
    }

    private bool IsGrounded()
    { 
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private bool IsWalled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }

    private void WallSlide()
    {
        if (IsWalled() && !IsGrounded() && horizontal != 0f)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void WallJump()
    {
        if (isWallSliding)
        {
            wallJumpingDirection = -transform.localScale.x;
            wallJumpingCounter = wallJumpingCT;
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump") && wallJumpingCounter > 0f)
        {
            isWallJumping = true;
            rb.linearVelocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
            wallJumpingCounter = 0f;

            if (transform.localScale.x != wallJumpingDirection)
            {
                isFacingRight = !isFacingRight;
                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }
        }
    }

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

    private IEnumerator UpDash()
    {
        NumDashes -= 1;
        canDash = false;
        isDashing = true;
        rb.gravityScale = dashGravity;
        tr.emitting = true;
        if (rb.linearVelocity.x > speed)
        {
            rb.linearVelocity = new Vector2((rb.linearVelocity.x - speed) + transform.localScale.x * dashingPower, transform.localScale.y * dashHeight);
        } else if (rb.linearVelocity.x < (-1 * speed))
        {
            rb.linearVelocity = new Vector2((rb.linearVelocity.x + speed) + transform.localScale.x * dashingPower, transform.localScale.y * dashHeight);
        } else
        {
            rb.linearVelocity = new Vector2(transform.localScale.x * dashingPower, transform.localScale.y * dashHeight);
        }
        yield return new WaitForSeconds(dashingTime);
        if (isDashing == true)
        {
            isDashing = false;
            rb.linearVelocity = new Vector2(0f, (rb.linearVelocity.y / 1.5f));
            rb.gravityScale = originalGravity;
        }
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }
    public IEnumerator DownDash()
    {
        NumDashes -= 1;
        canDash = false;
        isDashing = true;
        rb.gravityScale = -dashGravity;
        tr.emitting = true;
        if (rb.linearVelocity.x > speed)
        {
            rb.linearVelocity = new Vector2((rb.linearVelocity.x - speed) + transform.localScale.x * dashingPower, transform.localScale.y * -dashHeight);
        }
        else if (rb.linearVelocity.x < (-1 * speed))
        {
            rb.linearVelocity = new Vector2((rb.linearVelocity.x + speed) + transform.localScale.x * dashingPower, transform.localScale.y * -dashHeight);
        }
        else
        {
            rb.linearVelocity = new Vector2(transform.localScale.x * dashingPower, transform.localScale.y * -dashHeight);
        }
        yield return new WaitForSeconds(dashingTime);
        if (isDashing == true)
        {
            isDashing = false;
            rb.linearVelocity = new Vector2(0f, (rb.linearVelocity.y / 2));
            rb.gravityScale = originalGravity;
        }
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }
    private IEnumerator GroundDash()
    {
        NumDashes -= 1;
        canDash = false;
        isDashing = true;
        rb.gravityScale = 0f;
        tr.emitting = true;
        if (rb.linearVelocity.x > speed)
        {
            rb.linearVelocity = new Vector2((rb.linearVelocity.x - speed) + transform.localScale.x * dashingPower, transform.localScale.y * -dashHeight / 2);
        }
        else if (rb.linearVelocity.x < (-1 * speed))
        {
            rb.linearVelocity = new Vector2((rb.linearVelocity.x + speed) + transform.localScale.x * dashingPower, transform.localScale.y * -dashHeight / 2);
        }
        else
        {
            rb.linearVelocity = new Vector2(transform.localScale.x * dashingPower, transform.localScale.y * -dashHeight / 2);
        }
        yield return new WaitForSeconds(dashingTime);
        if (isDashing == true)
        {
            isDashing = false;
            rb.linearVelocity = new Vector2(0f, 0f);
            rb.gravityScale = originalGravity;
        }
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }
    private IEnumerator CancelDash()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower / 1.25f);
        yield return new WaitForSeconds(stopDash);
        isDashing = false;
        rb.gravityScale = originalGravity;
    }
}
