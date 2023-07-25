using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movement : MonoBehaviour
{
    private float time;
    private float startY;


    // Start is called before the first frame update
    void Start()
    {
        this.time = 0.0f;
        this.startY = this.gameObject.transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
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
}
