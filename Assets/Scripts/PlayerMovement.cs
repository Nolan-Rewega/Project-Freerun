using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class movement : MonoBehaviour
{
    //Settings
    [Header("Movement Settings")]
    [SerializeField] float moveSpeed = 10;
    [SerializeField] float sprintSpeed = 15;
    [SerializeField] float acceleration = 10;

    [SerializeField] float staminaDrainSpeed = 1;
    [SerializeField] float staminaRecoverySpeed = 1;
    [SerializeField] float staminaDeadRecoverySpeed = .5f;
    [SerializeField] float groundDrag = 8;
    [SerializeField] float jumpForce = 10;
    [SerializeField] float jumpCooldown = 0.25f;
    [SerializeField] float airMultiplier = 0.5f;
    [SerializeField] float playerHeight = 2;
    [SerializeField] float sprintFov;
    [SerializeField] float zoomFov;

    [SerializeField] float killHeight = -10f; //World kill height if player falls off map
    [Header("Assignables")]
    [SerializeField] KeyCode jumpKey = KeyCode.Space;
    [SerializeField] KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] LayerMask whatIsGround;
    [SerializeField] Transform orientation;
    [SerializeField] Camera mainCamera;
    [SerializeField] TMP_Text velocityText;
    [SerializeField] RectTransform staminaBar;
    [SerializeField] CanvasGroup staminaBarParent;
    [SerializeField] AudioSource cameraAudioSource;
    [SerializeField] AudioSource footstepsAudioSource;

    [Header("Footsteps")]
    [SerializeField] float stepRate = 0.4f;
    [SerializeField] float stepSprintRate = .28f;
    [SerializeField] float stepCoolDown;

    [Header("Sounds")]
    [SerializeField] AudioClip[] walkingFootSteps;
    [SerializeField] AudioClip[] sprintingFootSteps;
    [SerializeField] AudioClip landSound;
    [SerializeField] AudioClip jumpSound;
    [SerializeField] AudioClip zoomIn;
    [SerializeField] AudioClip zoomOut;

    [Header("HUD")]
    [SerializeField] Color32 staminaColor;
    [SerializeField] Color32 staminaDepletedColor;
    [SerializeField] Color32 velocityColor;

    //Private variables
    private bool grounded;
    private bool readyToJump = true;
    private bool landed;
    private bool sprinting;
    private bool zooming;
    private bool staminaDepleted;
    private float horizontalInput;
    private float verticalInput;
    private float currentMoveSpeed;
    private float defaultFov;
    private float currentStamina;
    private float staminaInUseTimer;
    private bool showVelocity;
    private bool staminaInUse;
    private Color32 currentStaminaColor;
    private Rigidbody rb;
    private Vector3 moveDirection;

    [SerializeField] Animator cameraAnimator; //Camera animator for landing effect


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        defaultFov = mainCamera.fieldOfView;
        currentStaminaColor = staminaColor;
        currentStamina = 1;
    }


    private void Update()
    {
        MyInput();
        SpeedControl();
        CheckForLanding();
        CheckForGrounded();
        
        if (transform.position.y < killHeight) print("dead"); //TODO -- Respawn player

        velocityText.text = rb.velocity.magnitude.ToString("00");

        if (zooming) {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, zoomFov, 10 * Time.deltaTime);
            showVelocity = false;
        }
        else showVelocity = true;

        if (showVelocity) velocityText.color = Color.Lerp(velocityText.color, velocityColor, Time.deltaTime * 7);
        else velocityText.color = Color.Lerp(velocityText.color, new Color32(255, 255, 255, 0), Time.deltaTime * 7);

        staminaBar.localScale = (new Vector3(currentStamina, 1, 1));

        staminaBar.GetComponent<Image>().color = Color.Lerp(staminaBar.GetComponent<Image>().color, currentStaminaColor, Time.deltaTime * 5);
        if (!staminaInUse) staminaInUseTimer -= Time.deltaTime * 3;
        else {
            staminaInUseTimer = 1;
            staminaBarParent.alpha = Mathf.Lerp(staminaBarParent.alpha, 1, Time.deltaTime * 5);
        } 
        if (staminaInUseTimer < 0) staminaBarParent.alpha = Mathf.Lerp(staminaBarParent.alpha, 0, Time.deltaTime * 3);

        FootSteps();
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
            footstepsAudioSource.pitch = 1f - 0.1f + Random.Range(-.1f, .1f);
            footstepsAudioSource.PlayOneShot(landSound);
            if (rb.velocity.y < -2) cameraAnimator.SetTrigger("Land");
            landed = true;
        }
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (verticalInput > 0 && currentStamina > 0 && !staminaDepleted)
        {
            if (Input.GetKey(sprintKey)) sprinting = true;
            else sprinting = false;
        }
        else sprinting = false;

        if (sprinting){
            if (currentStamina > 0) currentStamina -= Time.deltaTime * staminaDrainSpeed;
            staminaInUse = true;
            if (!zooming) mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, sprintFov, 10 * Time.deltaTime);
            currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, sprintSpeed, acceleration * Time.deltaTime);
            if (currentStamina <= 0) StaminaDepleted();
        }
        else if(staminaDepleted){
            if (currentStamina < 1) currentStamina += Time.deltaTime * staminaRecoverySpeed;
            if (currentStamina < 0.75f) {
                staminaInUse = true;
            } 
            else staminaInUse = false;
            if (!zooming) mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, defaultFov, 10 * Time.deltaTime);
            currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, moveSpeed, acceleration * Time.deltaTime);
            if (currentStamina >= 1)
            {
                staminaDepleted = false;
                currentStaminaColor = staminaColor;
            }
        }
        else //Normal walking
        {
            if (currentStamina < 1) currentStamina += Time.deltaTime * staminaDeadRecoverySpeed;
            if (currentStamina < 0.75f) staminaInUse = true;
            else staminaInUse = false;
            if(!zooming) mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, defaultFov, 10 * Time.deltaTime);
            currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, moveSpeed, acceleration * Time.deltaTime);
        }

        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Input.GetMouseButtonDown(1))
        {
            cameraAudioSource.PlayOneShot(zoomIn);
            zooming = true;
        }
        if (Input.GetMouseButtonUp(1))
        {
            cameraAudioSource.PlayOneShot(zoomOut);
            zooming = false;
        }
    }



    private void StaminaDepleted()
    {
        if (staminaDepleted == true) return;
        currentStaminaColor = staminaDepletedColor;
        staminaDepleted = true;
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
        footstepsAudioSource.pitch = 1f - 0.1f + Random.Range(-.1f, .1f);
        footstepsAudioSource.PlayOneShot(jumpSound);
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;
    }

    void FootSteps()
    {
        stepCoolDown -= Time.deltaTime;
        if (grounded)
        {
            if ((Input.GetAxis("Horizontal") != 0f || Input.GetAxis("Vertical") != 0f) && stepCoolDown < 0f)
            {
                if (sprinting)
                {
                    footstepsAudioSource.pitch = 1f - 0.1f + Random.Range(-.1f, .1f);
                    int randomStep = Random.Range(0, sprintingFootSteps.Length);
                    footstepsAudioSource.PlayOneShot(sprintingFootSteps[randomStep]);
                    stepCoolDown = stepSprintRate;
                }
                else //Normal walking
                {
                    footstepsAudioSource.pitch = 1f - 0.1f + Random.Range(-.1f, .1f);
                    int randomStep = Random.Range(0, walkingFootSteps.Length);
                    footstepsAudioSource.PlayOneShot(walkingFootSteps[randomStep]);
                    stepCoolDown = stepRate;
                }
            }
        }
    }
}
