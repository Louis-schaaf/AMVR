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

        if (gunShooter != null)
        {
            gunShooter.SetGrabber(hand);
        }

        Transform pose = hand.gameObject.name.Contains("Right") ? rightHandPose : leftHandPose;

        // gun relative to hand
        transform.position = hand.transform.TransformPoint(pose.localPosition);
        transform.rotation = hand.transform.rotation * pose.localRotation;
    }

}