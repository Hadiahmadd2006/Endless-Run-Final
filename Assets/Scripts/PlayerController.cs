using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float laneOffset = 2f;
    public float laneChangeSpeed = 10f;
    public float jumpForce = 10f;
    public float groundCheckDistance = 1.2f;
    public float minYClamp = -5f;

    [Header("Animation Names")]
    public string runStateName = "Run";
    public string jumpTriggerName = "Jump";
    public string jumpStateName = "Jump";
    public string dieTriggerName = "Die";

    private Rigidbody rb;
    private Animator anim;
    private CapsuleCollider col;

    private int lane = 0;         // -1 left, 0 middle, +1 right
    private float baseLaneX;
    private float baseY;
    private bool isDead = false;
    private bool canJump = true;
    private bool inJump = false;
    private bool leftGround = false;
    private float groundedTimer = 0f;
    [Header("Interaction")]
    public float interactionRadius = 0.6f;
    public LayerMask interactionMask = ~0;

    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioClip jumpSfx;
    public AudioClip dieSfx;

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
            PlayRun();
        }
    }

    void Update()
    {
        if (isDead) return;

        // Swap directions: A/Left moves right lane, D/Right moves left lane
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            lane = Mathf.Clamp(lane + 1, -1, 1);

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            lane = Mathf.Clamp(lane - 1, -1, 1);

        if (Input.GetKeyDown(KeyCode.Space) && canJump && IsGrounded())
        {
            Jump();
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;

        bool grounded = IsGrounded();
        if (!grounded) leftGround = true;
        if (grounded)
        {
            groundedTimer += Time.fixedDeltaTime;
        }
        else
        {
            groundedTimer = 0f;
        }

        // Lane movement
        float targetX = baseLaneX + lane * laneOffset;
        float xDelta = targetX - rb.position.x;
        float sideSpeed = xDelta * laneChangeSpeed;

        Vector3 velocity = rb.linearVelocity;
        velocity.x = sideSpeed;
        velocity.z = 0f;          // world moves, player stays
        if (grounded && velocity.y < 0f) velocity.y = 0f;
        rb.linearVelocity = velocity;

        // Clamp Y
        Vector3 pos = rb.position;
        if (grounded && !inJump)
        {
            // Hard lock to baseY and disable gravity while on the ground to prevent vertical hunting.
            pos.y = baseY;
            rb.position = pos;
            Vector3 v = rb.linearVelocity;
            v.y = 0f;
            rb.linearVelocity = v;
        }
        else
        {
            if (pos.y < minYClamp)
            {
                pos.y = baseY;
                rb.position = pos;
            }
        }

        // Landing
        // End jump after grounded for 0.5s
        if (inJump && grounded && leftGround && groundedTimer >= 0.5f)
        {
            inJump = false;
            canJump = true;
            PlayRun();
            leftGround = false;
        }
        else if (grounded)
        {
            canJump = true;
        }

        // Keep run playing when not jumping
        if (!inJump && !isDead)
        {
            PlayRun();
        }

        CheckInteractions();
    }

    void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        inJump = true;
        canJump = false;
        TriggerAnim(jumpTriggerName);
        PlaySfx(jumpSfx);
        // Force jump state immediately so the clip plays
        if (anim != null && !string.IsNullOrEmpty(jumpStateName))
        {
            anim.CrossFadeInFixedTime(jumpStateName, 0.01f);
        }
    }

    void CheckInteractions()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        float radius = interactionRadius;
        // If we have a capsule, align the overlap with its height/radius.
        if (col != null)
        {
            origin = transform.position + Vector3.up * (col.height * 0.5f);
            radius = Mathf.Max(col.radius * 1.1f, interactionRadius);
        }

        var hits = Physics.OverlapSphere(origin, radius, interactionMask, QueryTriggerInteraction.Collide);
        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];
            if (h == null) continue;
            var collectible = h.GetComponentInParent<Collectible>();
            if (collectible != null)
            {
                collectible.HandleCollect(this);
                continue;
            }
            var obstacle = h.GetComponentInParent<Obstacle>();
            if (obstacle != null)
            {
                obstacle.HandleHit(this);
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

    void PlayRun()
    {
        if (anim == null || string.IsNullOrEmpty(runStateName)) return;
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName(runStateName))
        {
            anim.CrossFade(runStateName, 0.05f);
        }
    }

    void TriggerAnim(string triggerName)
    {
        if (anim == null || string.IsNullOrEmpty(triggerName)) return;
        anim.ResetTrigger(triggerName);
        anim.SetTrigger(triggerName);
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        rb.linearVelocity = Vector3.zero;
        TriggerAnim(dieTriggerName);
        PlaySfx(dieSfx);
        GameManager.Instance?.OnPlayerDied();
    }

    public void PlaySfx(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }
}
