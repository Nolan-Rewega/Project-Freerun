using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    //Controls
    [Header("Settings")]
    [SerializeField] float mouseSensX = 800; //Mouse sensitivity on the X axis (both should be the same)
    [SerializeField] float mouseSensY = 800; //Mouse sensitivity on the Y axis

    [Space(10)]
    //Camera tilting
    public bool allowMovementTilt; //Kept public as it may be an option in a menu
    [SerializeField] float movementTiltAmount; //Amount of tilt camera recieves when input is detected
    [SerializeField] float smoothTiltAmount; //How fast the camera tilts to it's desired rotation
    [Space(10)]
    public bool allowRotationTilt;
    [SerializeField] float rotTiltAmount;
    [SerializeField] float smoothRotAmount;

    [Header("Assignables")]
    //Assignables
    [SerializeField] Transform orientation; //Orientation of the player (this will actually rotate on the Y axis depending on where the player is looking)
    [SerializeField] Transform cameraRot; //Camera parent that is being rotated by mouse movement
    [SerializeField] Transform cameraObject; //Used for camera tilting

    //Private variables
    private float xRotation;
    private float yRotation;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; //Keep cursor locked in center
        Cursor.visible = false; //Hide cursor

        //Set rotation to 0 at start
        xRotation = 0;
        yRotation = 0;
    }

    private void Update()
    {
        CameraLook();
        if (allowMovementTilt) MovementTilt();
        if (allowRotationTilt) RotationTilt();
    }

    private void CameraLook()
    {
        //TODO -- if paused set mouseX & mouseY to 0

        //Recieving input from mouse
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.fixedDeltaTime * mouseSensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.fixedDeltaTime * mouseSensY;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90, 90f); //Clamp X rotation so player can not look too far down or up

        cameraRot.rotation = Quaternion.Euler(xRotation, yRotation, 0); //Actually setting the transform for the camera's rotation
        orientation.rotation = Quaternion.Euler(0, yRotation, 0); //Setting the transform Y rotation of the player's orientation (X & Z are locked to 0)
    }

    private void MovementTilt()
    {
        float rotZ = -Input.GetAxis("Horizontal") * movementTiltAmount; //This is getting the horizontal axis which is just A or D (will also work with controller)
        Quaternion finalRot = Quaternion.Euler(0, 0, rotZ);
        cameraObject.transform.localRotation = Quaternion.Lerp(cameraObject.transform.localRotation, finalRot, smoothTiltAmount * Time.deltaTime); //Lerping the local rotation of the camera's Z axis for the tilting
        //I'm using lerping because when you press A or D the axis goes straight to -1 or 1, so we smooth the rotation so it's not so jarring
    }


    private void RotationTilt()
    {
        float rotZ = Input.GetAxisRaw("Mouse X") * rotTiltAmount;
        Quaternion finalRot = Quaternion.Euler(0, 0, rotZ);
        cameraObject.transform.localRotation = Quaternion.Lerp(cameraObject.transform.localRotation, finalRot, smoothRotAmount * Time.deltaTime);
    }
}
