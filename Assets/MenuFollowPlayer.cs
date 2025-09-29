using UnityEngine;

public class MenuFollowPlayer : MonoBehaviour
{
    [Header("Target to follow (usually the VR Camera)")]
    public Transform playerCamera;

    [Header("Offset from player")]
    public Vector3 offset = new Vector3(0, 0, 2.0f); // 2 meters in front

    [Header("Follow settings")]
    public float followSpeed = 5f;
    public bool rotateToFacePlayer = true;

    void Start()
    {
        if (playerCamera == null)
        {
            // Automatically find the main VR camera
            playerCamera = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        if (playerCamera == null) return;

        // Desired position: in front of the camera
        Vector3 targetPosition = playerCamera.position + playerCamera.forward * offset.z
                               + playerCamera.up * offset.y
                               + playerCamera.right * offset.x;

        // Smoothly move towards the target
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);

        // Optionally rotate to face the camera
        if (rotateToFacePlayer)
        {
            Vector3 lookDirection = transform.position - playerCamera.position;
            lookDirection.y = 0; // keep upright
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * followSpeed);
        }
    }
}
