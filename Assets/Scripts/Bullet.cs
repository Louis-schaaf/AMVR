using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 20f;      // Speed of the bullet
    public float lifeTime = 3f;    // Time before the bullet is destroyed

    [Header("Bullet Hole")]
    public GameObject bulletHolePrefab; // Assign your BulletHole prefab in the Inspector

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Destroy(gameObject, lifeTime);
    }

    void FixedUpdate()
    {
        rb.velocity = transform.forward * speed;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Get first contact point
        ContactPoint contact = collision.contacts[0];

        // Spawn bullet hole
        if (bulletHolePrefab != null)
        {
            // Align the bullet hole to the surface normal
            Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, contact.normal);

            // Slightly offset from the surface to prevent z-fighting
            Vector3 spawnPosition = contact.point + contact.normal * 0.001f;

            GameObject hole = Instantiate(bulletHolePrefab, spawnPosition, rotation);

            // Make the hole stick to the object that was hit
            hole.transform.SetParent(collision.transform);

            // Randomize rotation for visual variety
            hole.transform.Rotate(Vector3.forward, Random.Range(0f, 360f));

            // Optionally destroy after a delay to clean up old decals
            Destroy(hole, 10f);
        }

        // Destroy bullet immediately after impact
        Destroy(gameObject);
    }
}
