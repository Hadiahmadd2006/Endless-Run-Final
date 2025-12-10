using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float forwardSpeed = 14f;
    public float laneDistance = 10f; 
    public float laneSwitchSpeed = 12f;
    private int currentLane = 0; // -1 = left, 0 = mid, +1 = right
    private Vector3 targetPosition;

    [Header("Jump")]
    public float jumpForce = 7f;
    private bool isGrounded = true;

    [Header("Roll")]
    public float rollDuration = 0.7f;
    private bool isRolling = false;

    private Rigidbody rb;
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        // Start immediately with Run animation
        anim.Play("Run");
    }

    void Update()
    {
        // Auto-forward running
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, forwardSpeed);

        HandleLaneInput();
        SmoothLaneMovement();

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            Jump();

        if (Input.GetKeyDown(KeyCode.LeftControl) && isGrounded && !isRolling)
            StartCoroutine(Roll());
    }

    // --------------------------------------------------
    // LANE MOVEMENT SYSTEM
    // --------------------------------------------------
    void HandleLaneInput()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow) && currentLane < 1)
            currentLane++;

        if (Input.GetKeyDown(KeyCode.LeftArrow) && currentLane > -1)
            currentLane--;

        targetPosition = new Vector3(currentLane * laneDistance, transform.position.y, transform.position.z);
    }

    void SmoothLaneMovement()
    {
        Vector3 newPos = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * laneSwitchSpeed);
        transform.position = newPos;
    }

    // --------------------------------------------------
    // JUMP
    // --------------------------------------------------
    void Jump()
    {
        isGrounded = false;

        anim.SetTrigger("Jump");
        anim.SetBool("isJumping", true);

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            anim.SetBool("isJumping", false);
        }
    }

    // --------------------------------------------------
    // ROLL
    // --------------------------------------------------
    IEnumerator Roll()
    {
        isRolling = true;
        anim.SetTrigger("Roll");
        anim.SetBool("isRolling", true);

        // CapsuleCollider col = GetComponent<CapsuleCollider>();
        // col.height = 1f;

        yield return new WaitForSeconds(rollDuration);

        anim.SetBool("isRolling", false);
        isRolling = false;

        // col.height = 2f;
    }

    // --------------------------------------------------
    // DEATH
    // --------------------------------------------------
    public void Die()
    {
        anim.SetTrigger("Die");
        forwardSpeed = 0;
    }
}
