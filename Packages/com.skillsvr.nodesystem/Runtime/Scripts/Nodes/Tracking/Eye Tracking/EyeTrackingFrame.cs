using UnityEngine;

public struct EyeTrackingFrame
{
    public EyeTrackingFrame(Vector3 lookAtPosition, Quaternion lookAtRotation, string[] activeNodes = null)
    {
        timeStamp = Time.realtimeSinceStartup;
        this.lookAtRotation = lookAtRotation;
        this.lookAtPosition = lookAtPosition;
    }

    [SerializeField] float timeStamp;
    [SerializeField] public Vector3 lookAtPosition;
    [SerializeField] public Quaternion lookAtRotation;
}
