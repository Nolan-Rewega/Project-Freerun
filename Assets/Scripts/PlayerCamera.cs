using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    private Transform childCameraTransform;

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
    }

    // Update is called once per frame
    void Update()
    {
        float deltaX = Input.GetAxis("Mouse X");
        float deltaY = Input.GetAxis("Mouse Y");

        // This is also Scuffed, will change later...
        this.childCameraTransform.eulerAngles += new Vector3(-deltaY, 0.0f, 0.0f);
        this.gameObject.transform.eulerAngles += new Vector3(0.0f, deltaX, 0.0f);
    }
}
