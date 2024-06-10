using Oculus.Movement.Tracking;
using System.Collections.Generic;
using System.Linq;
#if PICO_XR
using Unity.XR.PXR;
#endif
using UnityEngine;

public static class TrackingFrameManager
{

    public static EyeTrackingFrame RecordEyeFrame()
    {
        if (TrackingDirector.EyeTrackingForward != null)
        {
            Transform lookAtTransform = TrackingDirector.EyeTrackingForward;
            var data = new EyeTrackingFrame(lookAtTransform.position, lookAtTransform.rotation);
            return data;

        }
        else if (Camera.main != null)
        {
            Transform lookAtTransform = Camera.main.transform;
            var data = new EyeTrackingFrame(lookAtTransform.position, lookAtTransform.rotation);
            return data;
        }

        return new EyeTrackingFrame();
    }


#if PICO_XR

    private static PxrFaceTrackingInfo faceTrackingInfo;
#endif


    public static float[] blendShapeWeight = new float[72];

    public static FaceTrackingFrame RecordFaceFrame(OVRFaceExpressions ocExpresion)
    {

        var  ftf  =new FaceTrackingFrame();
#if PICO_XR
       
            PXR_System.GetFaceTrackingData(0, GetDataType.PXR_GET_FACELIP_DATA, ref faceTrackingInfo);
            blendShapeWeight = faceTrackingInfo.blendShapeWeight;
        ftf = new FaceTrackingFrame(blendShapeWeight);
#else
         ocExpresion.CopyTo(blendShapeWeight);
        ftf = new FaceTrackingFrame(blendShapeWeight);

#endif
        return ftf;
    }

}