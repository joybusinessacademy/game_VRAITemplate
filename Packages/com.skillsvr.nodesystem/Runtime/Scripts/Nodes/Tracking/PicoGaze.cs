using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PicoGaze : MonoBehaviour
{
    public Transform eyeTracking;

    void FixedUpdate()
    {
#if PICO_XR
        if (Unity.XR.PXR.PXR_EyeTracking.GetCombineEyeGazeVector(out var direction))
        {
            eyeTracking.localRotation = Quaternion.LookRotation(direction, Vector3.up);
        }
#endif
    }


}
