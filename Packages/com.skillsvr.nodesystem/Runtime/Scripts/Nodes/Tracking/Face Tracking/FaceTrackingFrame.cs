using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static OVRFaceExpressions;

public struct FaceTrackingFrame
{
    public FaceTrackingFrame(float[] _blendShapeWeight)
    {
        blendShapeWeight = _blendShapeWeight;
    }

    [SerializeField] public float[] blendShapeWeight;
}
