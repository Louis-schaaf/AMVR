using UnityEngine;

public class Shooter : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform muzzle;
    public float fireRate = 0.5f;

    // References for audio and particles
    public AudioSource audioSource;
    public ParticleSystem muzzleFlashEffect;

    private float nextFireTime;
    private OVRGrabber currentGrabber;

    public void SetGrabber(OVRGrabber grabber)
    {
        currentGrabber = grabber;
    }

    void Update()
    {
        if (currentGrabber != null && Time.time > nextFireTime)
        {
            OVRInput.Controller controller = currentGrabber.gameObject.name.Contains("Right") ? OVRInput.Controller.RTouch : OVRInput.Controller.LTouch;

            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, controller))
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }
    }

    void Shoot()
    {
        // Instantiate the projectile
        Instantiate(projectilePrefab, muzzle.position, muzzle.rotation);

        // Play the sound effect if an AudioSource is assigned
        if (audioSource != null)
        {
            audioSource.Play();
        }

        // Play the particle effect if a ParticleSystem is assigned
        if (muzzleFlashEffect != null)
        {
            muzzleFlashEffect.Play();
        }
    }
}