using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static OVRFaceExpressions;


#if PICO_XR
using Unity.XR.PXR;
#endif

namespace SkillsVRNodes.ScriptableObjects
{


    public class EmotionAsset : ScriptableObject 
     {
        public List<FaceTrackingDataOc> ocData = new List<FaceTrackingDataOc>();
        public List<FaceTrackingDataPico> picoData = new List<FaceTrackingDataPico>();
    }

[Serializable]
    public class FaceTrackingDataOc 
    {
        public FaceExpression faceExpression;
        public float min;
        public float max = 100;

        public float weight = 0;
    }
    [Serializable]
    public class FaceTrackingDataPico 
    {
        public BlendShapeIndex pxr_FaceExpression;
        public float min;
        public float max = 100;
        public float weight = 0;
    }
}
        public enum BlendShapeIndex
    {
        EyeLookDown_L = 0,
        NoseSneer_L = 1,
        EyeLookIn_L = 2,
        BrowInnerUp = 3,
        BrowDown_R = 4,
        MouthClose = 5,
        MouthLowerDown_R = 6,
        JawOpen = 7,
        MouthUpperUp_R = 8,
        MouthShrugUpper = 9,
        MouthFunnel = 10,
        EyeLookIn_R = 11,
        EyeLookDown_R = 12,
        NoseSneer_R = 13,
        MouthRollUpper = 14,
        JawRight = 15,
        BrowDown_L = 16,
        MouthShrugLower = 17,
        MouthRollLower = 18,
        MouthSmile_L = 19,
        MouthPress_L = 20,
        MouthSmile_R = 21,
        MouthPress_R = 22,
        MouthDimple_R = 23,
        MouthLeft = 24,
        JawForward = 25,
        EyeSquint_L = 26,
        MouthFrown_L = 27,
        EyeBlink_L = 28,
        CheekSquint_L = 29,
        BrowOuterUp_L = 30,
        EyeLookUp_L = 31,
        JawLeft = 32,
        MouthStretch_L = 33,
        MouthPucker = 34,
        EyeLookUp_R = 35,
        BrowOuterUp_R = 36,
        CheekSquint_R = 37,
        EyeBlink_R = 38,
        MouthUpperUp_L = 39,
        MouthFrown_R = 40,
        EyeSquint_R = 41,
        MouthStretch_R = 42,
        CheekPuff = 43,
        EyeLookOut_L = 44,
        EyeLookOut_R = 45,
        EyeWide_R = 46,
        EyeWide_L = 47,
        MouthRight = 48,
        MouthDimple_L = 49,
        MouthLowerDown_L = 50,
        TongueOut = 51,
        PP = 52,
        CH = 53,
        o = 54,
        O = 55,
        I = 56,
        u = 57,
        RR = 58,
        XX = 59,
        aa = 60,
        i = 61,
        FF = 62,
        U = 63,
        TH = 64,
        kk = 65,
        SS = 66,
        e = 67,
        DD = 68,
        E = 69,
        nn = 70,
        sil = 71

    };