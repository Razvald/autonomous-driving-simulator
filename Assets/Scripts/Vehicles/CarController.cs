using UnityEngine;

public class CarController : MonoBehaviour
{
    public float acceleration = 8f;
    public float steering = 120f;
    public float maxSpeed = 10f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        float move = Input.GetAxis("Vertical");
        float turn = Input.GetAxis("Horizontal");

        if (rb.linearVelocity.magnitude < maxSpeed)
        {
            rb.AddForce(transform.up * move * acceleration);
        }

        rb.MoveRotation(rb.rotation - turn * steering * Time.fixedDeltaTime);
    }
}
