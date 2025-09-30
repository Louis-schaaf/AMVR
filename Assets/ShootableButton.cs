using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class ShootableButton : MonoBehaviour
{
    [Header("Event to trigger when the button is hit")]
    public UnityEvent onHit;

    [Tooltip("Optional tag that the hitting object must have (e.g., 'Bullet')")]
    public string requiredTag = "Bullet";

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collider belongs to the correct object
        if (string.IsNullOrEmpty(requiredTag) || collision.collider.CompareTag(requiredTag))
        {
            Debug.Log($"{name} was hit by {collision.collider.name}");

            // Trigger the assigned event(s)
            onHit.Invoke();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Optional: also allow trigger-based bullets
        if (string.IsNullOrEmpty(requiredTag) || other.CompareTag(requiredTag))
        {
            Debug.Log($"{name} was triggered by {other.name}");

            onHit.Invoke();
        }
    }
}
