using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movement : MonoBehaviour
{
    private bool IsJumping;
    private float time;
    private float startY;
    private float distToGround;

    private Rigidbody body;

    // Start is called before the first frame update
    void Start()
    {
        this.IsJumping = false;
        this.time = 0.0f;
        this.startY = this.gameObject.transform.position.y;
        this.distToGround = GetComponent<Collider>().bounds.extents.y;
     
        this.body = GetComponent<Rigidbody>();
        if (this.body == null)
        {
            this.body = gameObject.AddComponent<Rigidbody>();   
        }
    }

    // Update is called once per frame
    void Update()
    {
        Jump();
        Move();
        Strafe();
    }

 
    private void Move()
    {
        // Move this GameObject relative to its forward vector.
        float angle = this.gameObject.transform.eulerAngles.y * Mathf.Deg2Rad;
        float sprint = Input.GetKey(KeyCode.LeftShift) ? 1.5f : 1.0f;
        float speed = 5.0f;

        float up = Input.GetKey(KeyCode.W) ? 1.0f : 0.0f;
        float left = Input.GetKey(KeyCode.A) ? 1.0f : 0.0f;
        float down = Input.GetKey(KeyCode.S) ? 1.0f : 0.0f;
        float right = Input.GetKey(KeyCode.D) ? 1.0f : 0.0f;

        Vector3 direction = sprint * speed * Time.deltaTime * (new Vector3(right - left, 0.0f, up - down)).normalized;

        // This is scuffed, will change later
        this.gameObject.transform.position += new Vector3(Mathf.Cos(-angle) * direction.x - Mathf.Sin(-angle) * direction.z,
                                                          0.0f,
                                                          Mathf.Sin(-angle) * direction.x + Mathf.Cos(-angle) * direction.z);
    }

    private void Jump()
    {
        this.gameObject.transform.position = new Vector3(this.gameObject.transform.position.x,
                                                 this.JumpInterpolation(),
                                                 this.gameObject.transform.position.z);
    }

    // This is Omega Cursed and will be replaced...
    private float JumpInterpolation()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !this.IsJumping)
        {
            this.time = 0;
            this.startY = this.gameObject.transform.position.y;
            this.IsJumping = true;
        }
        
        if (this.IsJumping)
        {
            // This is definitely not how its done...
            this.time += Time.deltaTime;

            float a = this.startY;
            float b = 9.0f;
            float c = -45.0f;
            float d = 86.0f;
            float e = -65.0f;

            float xInterscept = 0.582f;

            if (this.time > xInterscept)
            {
                this.time = 0.0f;
                this.IsJumping = false;
                return this.gameObject.transform.position.y;
            }

            float x = this.time * xInterscept;

            return a + (b * x + c * Mathf.Pow(x, 2) + d * Mathf.Pow(x, 3) + e * Mathf.Pow(x, 4)) * 2;
        }

        return this.gameObject.transform.position.y;
    }

    private void Strafe()
    {
        if(!isGrounded())
        {
            float left = Input.GetKey(KeyCode.S) ? -1.25f : 1.0f;
            float right = Input.GetKey(KeyCode.D) ? 1.25f : 1.0f;

            Vector3 acceleration = new Vector3( (right - left) * (1/2), 0.0f, 0.0f);

            this.body.velocity += acceleration * 1;
            Debug.Log("here");
        }
    }
    private bool isGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, this.distToGround + 0.1f);
    }
};
