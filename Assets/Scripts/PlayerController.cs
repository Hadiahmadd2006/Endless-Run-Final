using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float forwardSpeed = 15f;      // Constant -Z speed
    public float laneOffset = 2f;         // Distance between lanes on X
    public float laneChangeSpeed = 10f;   // How fast to slide between lanes
    public float jumpForce = 10f;         // Upwards impulse
    public float groundCheckDistance = 1.5f;
    public float minYClamp = -5f;         // Safety floor

    [Header("Animation Names")]
    public string runStateName = "Run";
    public string jumpTriggerName = "Jump";
    public string dieTriggerName = "Die";

    private Rigidbody rb;
    private Animator anim;
    private int lane = 0;         // -1 left, 0 middle, +1 right (on X)
    private float baseLaneX;      // X position of the middle lane at start
    private float baseY;          // starting ground height
    private CapsuleCollider col;
    private bool isDead = false;
    private bool canJump = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        col = GetComponent<CapsuleCollider>();
        baseLaneX = transform.position.x;
        baseY = transform.position.y;

        rb.freezeRotation = true;
        rb.useGravity = true;

        if (anim != null)
        {
            anim.applyRootMotion = false;
            if (!string.IsNullOrEmpty(runStateName))
            {
                anim.CrossFade(runStateName, 0.05f);
            }
        }
    }

    void Update()
    {
        if (isDead) return;

        // A/Left = left lane (negative X), D/Right = right lane (positive X)
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.LeftArrow))
            lane = Mathf.Clamp(lane - 1, -1, 1);

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.RightArrow))
            lane = Mathf.Clamp(lane + 1, -1, 1);

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && canJump && IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            TriggerAnim(jumpTriggerName);
            canJump = false;
        }

    }

    void FixedUpdate()
    {
        if (isDead) return;

        bool grounded = IsGrounded();

        // Lane target and side movement
        float targetX = baseLaneX + lane * laneOffset;
        float xDelta = targetX - rb.position.x;
        float sideSpeed = xDelta * laneChangeSpeed;

        Vector3 velocity = rb.linearVelocity;
        velocity.z = -forwardSpeed;      // constant forward on -Z
        velocity.x = sideSpeed;          // slide toward lane on X

        // Clamp to ground when grounded and falling
        if (grounded && velocity.y < 0f)
        {
            velocity.y = 0f;
        }

        rb.linearVelocity = velocity;

        // Safety clamp to keep player above floor
        Vector3 pos = rb.position;
        if (grounded && pos.y < baseY - 0.05f)
        {
            pos.y = baseY;
            rb.position = pos;
        }
        else if (pos.y < minYClamp)
        {
            pos.y = baseY;
            rb.position = pos;
        }

        // Keep run looping
        if (grounded)
        {
            PlayRunIfNeeded();
            // reset jump when touching ground
            if (rb.linearVelocity.y <= 0f)
            {
                canJump = true;
            }
        }
    }

    bool IsGrounded()
    {
        float radius = 0.2f;
        float castDistance = groundCheckDistance;

        if (col != null)
        {
            radius = Mathf.Max(0.05f, col.radius * 0.9f);
            castDistance = Mathf.Max(groundCheckDistance, (col.height * 0.5f) - col.radius + 0.1f);
        }

        Vector3 origin = transform.position + Vector3.up * 0.05f;
        return Physics.SphereCast(origin, radius, Vector3.down, out _, castDistance);
    }

    void PlayRunIfNeeded()
    {
        if (anim == null || string.IsNullOrEmpty(runStateName)) return;
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName(runStateName))
        {
            anim.CrossFade(runStateName, 0.05f);
        }
    }

    void TriggerAnim(string triggerName)
    {
        if (anim != null && !string.IsNullOrEmpty(triggerName))
        {
            anim.SetTrigger(triggerName);
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        rb.linearVelocity = Vector3.zero;
        TriggerAnim(dieTriggerName);
    }
}
