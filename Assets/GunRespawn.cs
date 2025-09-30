using UnityEngine;

public class GunRespawn : MonoBehaviour
{
    [Header("Where the gun will respawn")]
    public Transform respawnPoint;

    [Header("Tag or Layer that counts as 'floor'")]
    public string floorTag = "Floor";

    [Header("Delay before respawn (seconds)")]
    public float respawnDelay = 0.5f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (respawnPoint == null)
        {
            Debug.LogWarning("GunRespawn: No respawn point assigned! Please assign one in the inspector.");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Detect if the gun has hit the floor
        if (collision.collider.CompareTag(floorTag))
        {
            Debug.Log("Gun touched the floor, respawning...");

            // Start a short delay before respawning
            Invoke(nameof(RespawnGun), respawnDelay);
        }
    }

    private void RespawnGun()
    {
        if (respawnPoint == null) return;

        // Reset position & rotation
        transform.position = respawnPoint.position;
        transform.rotation = respawnPoint.rotation;

        // Stop any movement
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Debug.Log("Gun respawned!");
    }
}
