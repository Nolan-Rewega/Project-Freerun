using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    // Settings
    // Movement parameters
    [Header("Movement Settings")]
    [SerializeField] float walkSpeed = 10;           // Walking speed of the player.
    [SerializeField] float sprintSpeed = 15;         // Sprinting speed of the player.
    [SerializeField] float crouchSpeed = 4;          // Crouch movement speed of the player.
    [SerializeField] float acceleration = 10;        // Speed at which the player accelerates from standing to moving.

    // Stamina parameters
    [SerializeField] float staminaDrainSpeed = 1;    // Speed at which stamina is drained while sprinting.
    [SerializeField] float staminaRecoverySpeed = 1; // Speed at which stamina is recovered after stopping sprinting.
    [SerializeField] float staminaDeadRecoverySpeed = .5f; // Speed at which stamina is recovered when not sprinting.

    // Physics parameters
    [SerializeField] float groundDrag = 8;          // Drag applied to the player when on the ground (simulate friction).
    [SerializeField] float jumpForce = 10;          // Force applied to the player when jumping.
    [SerializeField] float jumpCooldown = 0.25f;    // Cooldown between consecutive jumps.
    [SerializeField] float airMultiplier = 0.5f;    // Multiplier applied to movement speed when in the air.
    [SerializeField] float playerHeight = 2;        // Height of the player's collider.
    [SerializeField] float sprintFov;               // Field of view when sprinting.
    [SerializeField] float zoomFov;                 // Field of view when zooming.

    // World kill height if player falls off map
    [SerializeField] float killHeight = -10f;

    // Assignables
    // Input keys and references to components and objects
    [Header("Assignables")]
    [SerializeField] KeyCode jumpKey = KeyCode.Space; // Key used for jumping.
    [SerializeField] KeyCode crouchKey = KeyCode.LeftControl; // Key used for crouching.
    [SerializeField] KeyCode sprintKey = KeyCode.LeftShift; // Key used for sprinting.
    [SerializeField] LayerMask whatIsGround;          // Layer mask to detect ground for grounding checks.
    [SerializeField] Collider standingCollider;       // Collider used when the player is standing.
    [SerializeField] Collider crouchingCollider;     // Collider used when the player is crouching.
    [SerializeField] Transform orientation;          // Transform representing the player's view direction (e.g., camera).
    [SerializeField] Camera mainCamera;              // Main camera used for the player's view.
    [SerializeField] TMP_Text velocityText;          // Text component used to display the player's velocity.
    [SerializeField] RectTransform staminaBar;       // UI element representing the player's stamina bar.
    [SerializeField] CanvasGroup staminaBarParent;   // Parent canvas group for the stamina bar (used for fading in/out).
    [SerializeField] AudioSource cameraAudioSource;  // Audio source used for camera-related sounds.
    [SerializeField] AudioSource footstepsAudioSource; // Audio source used for footsteps sounds.

    // Footsteps parameters
    [Header("Footsteps")]
    [SerializeField] float walkStepRate = 0.4f;      // Time interval between walking footsteps.
    [SerializeField] float stepSprintRate = 0.28f;   // Time interval between sprinting footsteps.
    [SerializeField] float crouchStepRate = 0.8f;    // Time interval between crouching footsteps.
    [SerializeField] float stepCoolDown;            // Time interval between consecutive footsteps.

    // Audio clips
    [Header("Sounds")]
    [SerializeField] AudioClip[] walkingFootSteps;    // Footstep sounds played when walking.
    [SerializeField] AudioClip[] sprintingFootSteps;  // Footstep sounds played when sprinting.
    [SerializeField] AudioClip landSound;             // Sound played when the player lands from a fall.
    [SerializeField] AudioClip jumpSound;             // Sound played when the player jumps.
    [SerializeField] AudioClip zoomIn;                // Sound played when zooming in.
    [SerializeField] AudioClip zoomOut;               // Sound played when zooming out.

    // HUD colors
    [Header("HUD")]
    [SerializeField] Color32 staminaColor;           // Color of the stamina bar when stamina is not depleted.
    [SerializeField] Color32 staminaDepletedColor;   // Color of the stamina bar when stamina is depleted.
    [SerializeField] Color32 velocityColor;          // Color of the velocity text.

    // Private variables
    private bool grounded;                          // Indicates if the player is currently grounded.
    private bool readyToJump = true;                // Flag to control consecutive jumping.
    private bool landed;                            // Indicates if the player has just landed.
    private bool sprinting;                         // Indicates if the player is currently sprinting.
    private bool crouching;                         // Indicates if the player is currently crouching.
    private bool zooming;                           // Indicates if the player is currently zooming.
    private bool staminaDepleted;                   // Indicates if the player's stamina is depleted.
    private float horizontalInput;                  // Horizontal input from the player.
    private float verticalInput;                    // Vertical input from the player.
    private float currentMoveSpeed;                 // Current movement speed of the player.
    private float newMoveSpeed;                     // The target movement speed of the player.
    private float defaultFov;                       // Default field of view of the main camera.
    private float currentStamina;                   // Current stamina value of the player.
    private float staminaInUseTimer;                // Timer to control stamina bar visibility during use.
    private bool showVelocity;                      // Flag to control the visibility of the velocity text.
    private bool staminaInUse;                      // Flag indicating if the player's stamina is being used.
    private Color32 currentStaminaColor;            // Current color of the stamina bar.
    private Rigidbody rb;                           // Reference to the player's Rigidbody.
    private Vector3 moveDirection;                  // Direction of the player's movement.

    [SerializeField] Animator cameraAnimator;        // Camera animator for landing effect

    private void Start()
    {
        // Initialization
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        newMoveSpeed = walkSpeed;
        defaultFov = mainCamera.fieldOfView;
        currentStaminaColor = staminaColor;
        currentStamina = 1;
    }

    private void Update()
    {
        // Handle player input and update UI elements
        MyInput();
        SpeedControl();
        CheckForLanding();
        CheckForGrounded();
        CameraFX();

        if (transform.position.y < killHeight) print("dead"); // TODO -- Respawn player

        velocityText.text = rb.velocity.magnitude.ToString("00");

        if (showVelocity)
            velocityText.color = Color.Lerp(velocityText.color, velocityColor, Time.deltaTime * 7);
        else
            velocityText.color = Color.Lerp(velocityText.color, new Color32(255, 255, 255, 0), Time.deltaTime * 7);

        staminaBar.localScale = (new Vector3(currentStamina, 1, 1));

        staminaBar.GetComponent<Image>().color = Color.Lerp(staminaBar.GetComponent<Image>().color, currentStaminaColor, Time.deltaTime * 5);

        if (!staminaInUse)
            staminaInUseTimer -= Time.deltaTime * 3;
        else
        {
            staminaInUseTimer = 1;
            staminaBarParent.alpha = Mathf.Lerp(staminaBarParent.alpha, 1, Time.deltaTime * 5);
        }

        if (staminaInUseTimer < 0)
            staminaBarParent.alpha = Mathf.Lerp(staminaBarParent.alpha, 0, Time.deltaTime * 3);

        FootSteps();

        currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, newMoveSpeed, acceleration * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        // Move the player using physics
        MovePlayer();
    }

    private void CheckForGrounded()
    {
        // Check if the player is grounded using a raycast
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    private void CheckForLanding()
    {
        // Check if the player has landed after falling
        if (!grounded && landed) landed = false;
        if (grounded && !landed)
        {
            // Play landing sound and landing camera animation
            footstepsAudioSource.pitch = 1f - 0.1f + Random.Range(-.1f, .1f);
            footstepsAudioSource.PlayOneShot(landSound);
            if (rb.velocity.y < -2)
                cameraAnimator.SetTrigger("Land");
            landed = true;
        }
    }

    private void MyInput()
    {
        // Handle player input for movement, jumping, and other actions
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        Sprinting();

        if (Input.GetKeyDown(crouchKey) && !crouching)
            StartCrouch();
        else if (Input.GetKeyUp(crouchKey) && crouching)
            StopCrouch();

        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            // Perform a jump and set a jump cooldown
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Input.GetMouseButtonDown(1))
        {
            // Zoom in
            cameraAudioSource.PlayOneShot(zoomIn);
            zooming = true;
        }
        if (Input.GetMouseButtonUp(1))
        {
            // Zoom out
            cameraAudioSource.PlayOneShot(zoomOut);
            zooming = false;
        }
    }

    private void Sprinting()
    {
        // Handle sprinting mechanics and stamina management
        if (verticalInput > 0 && currentStamina > 0 && !staminaDepleted)
        {
            if (Input.GetKeyDown(sprintKey) && grounded && !staminaDepleted)
                StartSprint();
            if (Input.GetKeyUp(sprintKey) && sprinting)
                StopSprint();
        }

        if (sprinting)
        {
            // Drain stamina while sprinting
            staminaInUse = true;
            if (currentStamina > 0)
                currentStamina -= Time.deltaTime * staminaDrainSpeed;
            if (currentStamina <= 0)
                StaminaDepleted();
        }
        else if (staminaDepleted)
        {
            // Recover stamina after sprinting and depleted
            if (currentStamina < 0.75f)
                staminaInUse = true;
            else
                staminaInUse = false;
            if (currentStamina < 1)
                currentStamina += Time.deltaTime * staminaRecoverySpeed;
            if (currentStamina >= 1)
            {
                staminaDepleted = false;
                currentStaminaColor = staminaColor;
            }
        }
        else if (crouching)
        {
            // Recover stamina while crouching
            if (currentStamina < 1)
                currentStamina += Time.deltaTime * staminaDeadRecoverySpeed;
            if (currentStamina < 0.75f)
                staminaInUse = true;
            else
                staminaInUse = false;
        }
        else // Normal walking
        {
            // Recover stamina while walking
            if (currentStamina < 1)
                currentStamina += Time.deltaTime * staminaDeadRecoverySpeed;
            if (currentStamina < 0.75f)
                staminaInUse = true;
            else
                staminaInUse = false;
        }

        if (staminaDepleted && sprinting)
            StopSprint();
    }

    private void CameraFX()
    {
        // Handle camera field of view effects (zooming, sprinting)
        if (zooming)
        {
            showVelocity = false;
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, zoomFov, 10 * Time.deltaTime);
        }
        else if (sprinting)
        {
            showVelocity = true;
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, sprintFov, 10 * Time.deltaTime);
        }
        else
        {
            showVelocity = true;
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, defaultFov, 10 * Time.deltaTime); // Normal walking
        }
    }

    private void StartSprint()
    {
        // Start sprinting and adjust movement speed
        StopCrouch();
        newMoveSpeed = sprintSpeed;
        sprinting = true;
    }

    private void StopSprint()
    {
        // Stop sprinting and reset movement speed
        newMoveSpeed = walkSpeed;
        sprinting = false;
    }

    private void StartCrouch()
    {
        // Start crouching and adjust movement speed and collider
        StopSprint();
        newMoveSpeed = crouchSpeed;
        crouching = true;
        standingCollider.enabled = false;
        crouchingCollider.enabled = true;
        cameraAnimator.SetBool("Crouching", true);
    }

    private void StopCrouch()
    {
        // Stop crouching and reset movement speed and collider
        newMoveSpeed = walkSpeed;
        crouching = false;
        standingCollider.enabled = true;
        crouchingCollider.enabled = false;
        cameraAnimator.SetBool("Crouching", false);
    }

    private void StaminaDepleted()
    {
        // Handle stamina depletion state
        if (staminaDepleted == true)
            return;
        currentStaminaColor = staminaDepletedColor;
        staminaDepleted = true;
    }

    private void MovePlayer()
    {
        // Move the player using Rigidbody physics
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        if (grounded)
            rb.AddForce(10 * currentMoveSpeed * moveDirection.normalized, ForceMode.Force);
        else
            rb.AddForce(10 * airMultiplier * currentMoveSpeed * moveDirection.normalized, ForceMode.Force);
    }

    private void SpeedControl()
    {
        // Limit the player's maximum movement speed
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (flatVel.magnitude > currentMoveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * currentMoveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        // Perform a jump
        footstepsAudioSource.pitch = 1f - 0.1f + Random.Range(-.1f, .1f);
        footstepsAudioSource.PlayOneShot(jumpSound);
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        // Reset the jump state for consecutive jumps
        readyToJump = true;
    }

    void FootSteps()
    {
        // Handle playing footstep sounds
        stepCoolDown -= Time.deltaTime;
        if (!grounded)
            return;
        if ((Input.GetAxis("Horizontal") != 0f || Input.GetAxis("Vertical") != 0f) && stepCoolDown < 0f)
        {
            footstepsAudioSource.pitch = 1f - 0.1f + Random.Range(-.1f, .1f);
            if (sprinting)
            {
                int randomStep = Random.Range(0, sprintingFootSteps.Length);
                footstepsAudioSource.PlayOneShot(sprintingFootSteps[randomStep]);
                stepCoolDown = stepSprintRate;
            }
            else if (crouching)
            {
                int randomStep = Random.Range(0, walkingFootSteps.Length);
                footstepsAudioSource.PlayOneShot(walkingFootSteps[randomStep]);
                stepCoolDown = crouchStepRate;
            }
            else // Normal walking
            {
                int randomStep = Random.Range(0, walkingFootSteps.Length);
                footstepsAudioSource.PlayOneShot(walkingFootSteps[randomStep]);
                stepCoolDown = walkStepRate;
            }
        }
    }
}
