using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f; // The speed of the bullet
    public float lifeTime = 3f; // The time before the bullet is destroyed

    private Rigidbody rb;

    void Start()
    {
        // Get the Rigidbody component
        rb = GetComponent<Rigidbody>();

        // Destroy the bullet after its lifeTime has passed
        Destroy(gameObject, lifeTime);
    }

    void FixedUpdate()
    {
        // Move the bullet forward using the Rigidbody
        rb.velocity = transform.forward * speed;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if the bullet hits a target
        // if (collision.gameObject.CompareTag("Target"))
        // {
        //     // Handle target hit logic here (e.g., scoring, dealing damage)
        //     Debug.Log("Hit " + collision.gameObject.name);
        // }

        // Destroy the bullet on collision
        Destroy(gameObject);
    }
}