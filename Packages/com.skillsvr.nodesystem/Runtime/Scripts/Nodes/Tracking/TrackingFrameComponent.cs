using Oculus.Movement.Tracking;
using System;
using UnityEngine;



public class TrackingFrameComponent : MonoBehaviour
{
    public static Action<EyeTrackingFrame> CollectDataEye = _ => { };
    public static Action<FaceTrackingFrame> CollectDataFace = _ => { };

    private static OVRFaceExpressions face;

    private void OnEnable()
    {
        if (face == null)
            face = GetComponentInChildren<OVRFaceExpressions>();
    }

    private void FixedUpdate()
    {
        var data = TrackingFrameManager.RecordEyeFrame();
        CollectDataEye?.Invoke(data);

        if (HeadsetDetection.usingFace)
        {
#if !PICO_XR
            if (face == null)
                face = GetComponentInChildren<OVRFaceExpressions>();

            if (face == null)
                return;
#endif
                    
            var facedata = TrackingFrameManager.RecordFaceFrame(face);
            CollectDataFace?.Invoke(facedata);
        }
    }
}