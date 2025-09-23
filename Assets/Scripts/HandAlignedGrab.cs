using UnityEngine;

public class HandAlignedGrab : OVRGrabbable
{
    public Transform rightHandPose;
    public Transform leftHandPose;
    private Shooter gunShooter;

    protected override void Start()
    {
        base.Start();
        gunShooter = GetComponent<Shooter>();
    }

    public override void GrabBegin(OVRGrabber hand, Collider grabPoint)
    {
        base.GrabBegin(hand, grabPoint);

        // Tell the gun shooter script which hand is holding the gun
        if (gunShooter != null)
        {
            gunShooter.SetGrabber(hand);
        }

        if (hand.gameObject.name.Contains("Right"))
        {
            transform.position = rightHandPose.position;
            transform.rotation = rightHandPose.rotation;
        }
        else if (hand.gameObject.name.Contains("Left"))
        {
            transform.position = leftHandPose.position;
            transform.rotation = leftHandPose.rotation;
        }
    }
}