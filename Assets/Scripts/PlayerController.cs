using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float forwardSpeed = 14f;

    // Your map is sideways, so lanes move on Z not X
    public float laneDistance = 10f;
    public float laneSwitchSpeed = 12f;

    // Middle lane world position (sideways map)
    public float laneOffsetZ = 0f;

    private int currentLane = 0;
    private CharacterController cc;
    private Animator anim;

    [Header("Jump")]
    public float jumpHeight = 5f;
    public float gravity = -30f;
    private float verticalVelocity;

    [Header("Roll")]
    public float rollDuration = 0.7f;
    private bool isRolling = false;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();

        anim.Play("Run");

        // Start player at the middle lane
        Vector3 startPos = transform.position;
        startPos.z = laneOffsetZ;
        transform.position = startPos;
    }

    void Update()
    {
        Vector3 move = Vector3.zero;

        // Your world is rotated → forward = LEFT direction
        move += Vector3.left * forwardSpeed;

        HandleLaneInput();
        ApplyLaneMovement(ref move);
        ApplyGravity(ref move);

        cc.Move(move * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space) && cc.isGrounded)
            Jump();

        if (Input.GetKeyDown(KeyCode.LeftControl) && cc.isGrounded && !isRolling)
            StartCoroutine(Roll());
    }

    void HandleLaneInput()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow) && currentLane < 1)
            currentLane++;

        if (Input.GetKeyDown(KeyCode.LeftArrow) && currentLane > -1)
            currentLane--;
    }

    void ApplyLaneMovement(ref Vector3 move)
    {
        // Lanes move on Z axis in your sideways map
        float desiredZ = laneOffsetZ + currentLane * laneDistance;
        float diff = desiredZ - transform.position.z;

        move.z = diff * laneSwitchSpeed;
    }

    void ApplyGravity(ref Vector3 move)
    {
        if (cc.isGrounded)
        {
            if (verticalVelocity < 0)
                verticalVelocity = -1f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        move.y = verticalVelocity;
    }

    void Jump()
    {
        verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        anim.SetTrigger("Jump");
    }

    IEnumerator Roll()
    {
        isRolling = true;
        anim.SetTrigger("Roll");
        yield return new WaitForSeconds(rollDuration);
        isRolling = false;
    }

    public void Die()
    {
        anim.SetTrigger("Die");
        forwardSpeed = 0f;
    }
}