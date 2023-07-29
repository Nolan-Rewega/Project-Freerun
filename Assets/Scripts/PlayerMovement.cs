using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movement : MonoBehaviour
{
    //Settings
    [Header("Movement Settings")]
    [SerializeField] float moveSpeed = 10;
    [SerializeField] float sprintSpeed = 15;
    [SerializeField] float acceleration = 10;
    [SerializeField] float groundDrag = 8;
    [SerializeField] float jumpForce = 10;
    [SerializeField] float jumpCooldown = 0.25f;
    [SerializeField] float airMultiplier = 0.5f;
    [SerializeField] float playerHeight = 2;
    [SerializeField] float sprintFov;

    [SerializeField] float killHeight = -10f; //World kill height if player falls off map
    [Header("Assignables")]
    [SerializeField] KeyCode jumpKey = KeyCode.Space;
    [SerializeField] KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] LayerMask whatIsGround;
    [SerializeField] Transform orientation;
    [SerializeField] Camera mainCamera;

    //Private variables
    private bool grounded;
    private bool readyToJump = true;
    private bool landed;
    private bool sprinting;
    private float horizontalInput;
    private float verticalInput;
    private float currentMoveSpeed;
    private float defaultFov;
    private Rigidbody rb;
    private Vector3 moveDirection;

    [SerializeField] Animator cameraAnimator; //Camera animator for landing effect


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        defaultFov = mainCamera.fieldOfView;
    }


    private void Update()
    {
        MyInput();
        SpeedControl();
        CheckForLanding();
        CheckForGrounded();
        
        if (transform.position.y < killHeight) //TODO -- Respawn player
        {
            print("dead");
        }

    }
    private void FixedUpdate()
    {
        MovePlayer(); //In physics always use fixed update
    }

    private void CheckForGrounded()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        if (grounded) rb.drag = groundDrag;
        else rb.drag = 0;
    }

    private void CheckForLanding()
    {
        if (!grounded && landed) landed = false;
        if (grounded && !landed)
        {
            print("landed");
            if(rb.velocity.y < -2) cameraAnimator.SetTrigger("Land");
            landed = true;
        }
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(sprintKey)) sprinting = true;
        else sprinting = false;

        if (sprinting)
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, sprintFov, 10 * Time.deltaTime);
            currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, sprintSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, defaultFov, 10 * Time.deltaTime);
            currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, moveSpeed, acceleration * Time.deltaTime);
        }

        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        if (grounded) rb.AddForce(10 * currentMoveSpeed * moveDirection.normalized, ForceMode.Force);
        else rb.AddForce(10 * airMultiplier * currentMoveSpeed * moveDirection.normalized, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new(rb.velocity.x, 0f, rb.velocity.z);
        if (flatVel.magnitude > currentMoveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * currentMoveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;
    }
}
