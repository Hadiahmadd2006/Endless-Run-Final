using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float forwardSpeed = 15f;      // Constant +X speed
    public float laneOffset = 10f;        // Distance between lanes on Z
    public float laneChangeSpeed = 10f;   // How fast to slide between lanes
    public float jumpForce = 10f;         // Upwards impulse

    [Header("Grounding")]
    public float groundCheckDistance = 1.2f;

    [Header("Animation Names")]
    public string runStateName = "Run";
    public string jumpTriggerName = "Jump";
    public string dieTriggerName = "Die";
    public string rollTriggerName = "Roll";

    private Rigidbody rb;
    private Animator anim;
    private int lane = 0;         // -1 left, 0 middle, +1 right (on Z)
    private float baseLaneZ;      // Z position of the middle lane at start
    private float baseY;          // Y position to stick to when grounded
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        baseLaneZ = transform.position.z;
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

        // A/Left = left lane (negative Z), D/Right = right lane (positive Z)
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.LeftArrow))
            lane = Mathf.Clamp(lane - 1, -1, 1);

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.RightArrow))
            lane = Mathf.Clamp(lane + 1, -1, 1);

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            TriggerAnim(jumpTriggerName);
        }

        // Roll (S or DownArrow)
        if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && IsGrounded())
        {
            TriggerAnim(rollTriggerName);
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;

        bool grounded = IsGrounded();

        // Lane target and side movement
        float targetZ = baseLaneZ + lane * laneOffset;
        float zDelta = targetZ - rb.position.z;
        float sideSpeed = zDelta * laneChangeSpeed;

        Vector3 velocity = rb.linearVelocity;
        velocity.x = forwardSpeed;       // constant forward
        velocity.z = sideSpeed;          // slide toward lane

        // Keep Y steady when grounded (no drift until jump)
        if (grounded && velocity.y <= 0f)
        {
            velocity.y = 0f;
            rb.position = new Vector3(rb.position.x, baseY, rb.position.z);
        }

        rb.linearVelocity = velocity;

        // Keep run looping
        if (grounded)
        {
            PlayRunIfNeeded();
        }
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance);
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Train"))
        {
            Die();
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
