using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    private Transform childCameraTransform;
    private bool IsCrouching;
    private Vector3 CrouchHeight;

    // Start is called before the first frame update
    void Start()
    {
        this.childCameraTransform = gameObject.transform.Find("PlayerCamera");

        if (this.childCameraTransform == null)
        {
            // Trying out Debug Logging
            Debug.LogError($"ERROR: Could not find Child 'PlayerCamera' on Object '{this.gameObject.name}'");
            Application.Quit();
            UnityEditor.EditorApplication.isPlaying = false;
        }

        IsCrouching = false;
        CrouchHeight = new Vector3(0, 0.5f, 0);
    }

    // Update is called once per frame
    void Update()
    {

        // Moving the mouse will move the camera. 
        HandleMouse();

        // Crouching will cause the player camera to change. 
        HandleCrouch();

    }

    private void HandleMouse()
    {

        float deltaX = Input.GetAxis("Mouse X");
        float deltaY = Input.GetAxis("Mouse Y");

        // This is also Scuffed, will change later...
        this.childCameraTransform.eulerAngles += new Vector3(-deltaY, 0.0f, 0.0f);
        this.gameObject.transform.eulerAngles += new Vector3(0.0f, deltaX, 0.0f);
    }

    private void HandleCrouch()
    {

        if (!IsCrouching && Input.GetKey(KeyCode.LeftControl))
        {
            //!TODO: Need code to update the player model to a crouched state
            this.childCameraTransform.position -= CrouchHeight;
            IsCrouching = true;
            return; 
        }

        // Check if crouch has been released this frame 
        if(IsCrouching && !Input.GetKey(KeyCode.LeftControl))
        {
            this.childCameraTransform.position += CrouchHeight;
            IsCrouching = false;
            return; 
        }

    }


}
