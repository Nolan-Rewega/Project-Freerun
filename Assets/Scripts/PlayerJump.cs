using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    private bool IsStandingJumping, IsWallJumping;
    private Vector3 wallNormal;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        this.IsStandingJumping = false;
        this.IsWallJumping = false;
        this.rb = this.gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !this.IsStandingJumping)
        {
            this.rb.AddForce(Vector3.up * Random.Range(250.0f, 270.0f), ForceMode.Force);
            this.IsStandingJumping = true;
        }

        if (Input.GetKeyDown(KeyCode.Space) && !this.IsWallJumping)
        {
            this.rb.AddForce(this.wallNormal * Random.Range(250.0f, 270.0f), ForceMode.Force);
            this.IsWallJumping = true;
        }
    }

    // Clipping into multiple triggers can cause IsJumping to be stuck true.
    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "Standable")
        {
            // Reset Jump when touching the ground
            this.IsStandingJumping = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "WallJumpable")
        {
            Vector3 normal = collision.GetContact(0).normal;
            // Hack to give the normal a Y component. Might bug on normals that already have a y component.
            normal.y = 1.0f;

            this.wallNormal = normal;
            this.IsWallJumping = false;
        }
    }
}
