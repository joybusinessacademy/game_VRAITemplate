using System.Collections;
using System.Collections.Generic;
#if PICO_XR
using Unity.XR.PXR;
#endif
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.SceneManagement;


public enum HEADSET_TYPE
{
    QUEST,
    QUEST_EYE,
    QUEST_EYE_FACE,
    PICO,
    PICO_EYE,
    PICO_EYE_FACE
}

public static class HeadsetDetection 
{
    public static HEADSET_TYPE headsetType;

    public static bool usingFace, usingEye;
    public static HEADSET_TYPE editorOverride;
    public static void Initialize()
    {

#if PICO_XR
        //PICO
        headsetType = HEADSET_TYPE.PICO;
        if (SystemInfo.deviceModel.Contains("Pico A8E50"))
        {
            PermissionsGranted(ref headsetType);
        }


#else
        //OCULUS
        headsetType = HEADSET_TYPE.QUEST;
        ////var ocType = Unity.XR.Oculus.Utils.GetSystemHeadsetType();
        //var ocType = OVRPlugin.GetSystemHeadsetType();
        //switch ((int)ocType)
        //{
        //    case 8:
        //        headsetType = HEADSET_TYPE.QUEST;
        //        break;
        //    case 9:
        //        headsetType = HEADSET_TYPE.QUEST;
        //        break;
        //    case 10:
        //        PermissionsGranted(ref headsetType);
        //        break;
        //}

        string device = SystemInfo.deviceModel;


        switch (device)
        {
            //case 8:
            //    headsetType = HEADSET_TYPE.QUEST;
            //    break;
            //case 9:
            //    headsetType = HEADSET_TYPE.QUEST;
            //    break;
            case "Oculus Quest":
                PermissionsGranted(ref headsetType);
                break;
        }


#endif

#if UNITY_EDITOR
        headsetType = editorOverride;
#endif
    }

    public static void PermissionsGranted(ref HEADSET_TYPE type)
    {
        if(type == HEADSET_TYPE.PICO)
        {

            if (Permission.HasUserAuthorizedPermission("com.picovr.permission.EYE_TRACKING"))
            {
                type = HEADSET_TYPE.PICO_EYE;
                if (Permission.HasUserAuthorizedPermission("com.picovr.permission.FACE_TRACKING"))
                {
                    type = HEADSET_TYPE.PICO_EYE_FACE;
                    usingFace = true;
                }
            }          
        }
        else if (type == HEADSET_TYPE.QUEST)
        {
            if (Permission.HasUserAuthorizedPermission("com.oculus.permission.EYE_TRACKING"))
            {
                type = HEADSET_TYPE.QUEST_EYE;

                if (Permission.HasUserAuthorizedPermission("com.oculus.permission.FACE_TRACKING"))
                {
                    type = HEADSET_TYPE.QUEST_EYE_FACE;
                    usingFace = true;
                }
            }
        }
    }

}
