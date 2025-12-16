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
    public string dieStateName = "Die";

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
    [Header("Tag Fallback")]
    public int taggedCollectiblePoints = 1; // used if no Collectible component is found

    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioClip jumpSfx;
    public AudioClip dieSfx;

    public void ResetState()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (anim == null) anim = GetComponent<Animator>();

        isDead = false;
        inJump = false;
        canJump = true;
        leftGround = false;
        groundedTimer = 0f;
        lane = 0;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Snap back to run state
        if (anim != null && !string.IsNullOrEmpty(runStateName))
        {
            anim.Play(runStateName, 0, 0f);
        }
    }

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
        if (anim != null && !string.IsNullOrEmpty(dieStateName))
        {
            anim.CrossFadeInFixedTime(dieStateName, 0.05f);
        }
        GameManager.Instance?.OnPlayerDied();
    }

    public void PlaySfx(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }

    void OnTriggerEnter(Collider other)
    {
        HandleTagged(other);
    }

    void OnCollisionEnter(Collision collision)
    {
        HandleTagged(collision.collider);
    }

    void HandleTagged(Collider col)
    {
        if (col == null) return;
        if (col.CompareTag("Collectible"))
        {
            GameManager.Instance?.AddScore(taggedCollectiblePoints);
            Destroy(col.attachedRigidbody != null ? col.attachedRigidbody.gameObject : col.gameObject);
        }
        else if (col.CompareTag("Obstacle"))
        {
            PlaySfx(dieSfx);
            Die();
        }
    }

    void CheckInteractions()
    {
        Collider[] hits;
        if (col != null)
        {
            float radius = Mathf.Max(col.radius * 0.95f, interactionRadius);
            float halfHeight = Mathf.Max(col.height * 0.5f - col.radius, 0f);
            Vector3 center = transform.TransformPoint(col.center);
            Vector3 bottom = center + Vector3.down * halfHeight;
            Vector3 top = center + Vector3.up * halfHeight;
            hits = Physics.OverlapCapsule(bottom, top, radius, interactionMask, QueryTriggerInteraction.Collide);
        }
        else
        {
            Vector3 origin = transform.position + Vector3.up * 0.5f;
            float radius = interactionRadius;
            hits = Physics.OverlapSphere(origin, radius, interactionMask, QueryTriggerInteraction.Collide);
        }

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
            else if (h.CompareTag("Collectible"))
            {
                GameManager.Instance?.AddScore(taggedCollectiblePoints);
                Destroy(h.attachedRigidbody != null ? h.attachedRigidbody.gameObject : h.gameObject);
                continue;
            }
            var obstacle = h.GetComponentInParent<Obstacle>();
            if (obstacle != null)
            {
                obstacle.HandleHit(this);
                continue;
            }
            else if (h.CompareTag("Obstacle"))
            {
                PlaySfx(dieSfx);
                Die();
                continue;
            }
            var simple = h.GetComponentInParent<SimpleObstacle>();
            if (simple != null)
            {
                PlaySfx(simple.hitSfx);
                Die();
            }
        }
    }
}
