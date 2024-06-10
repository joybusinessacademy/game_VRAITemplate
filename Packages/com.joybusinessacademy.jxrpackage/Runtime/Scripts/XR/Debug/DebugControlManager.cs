using UnityEngine;

namespace JBA.XRPlayerPackage.XRDebug
{
    public class DebugControlManager : MonoBehaviour
    {

#region Input Strings
        public static string occulusLeftStickButton = "Oculus_CrossPlatform_PrimaryThumbstick";
        public static string occulusLeftStickHorizontalAxis = "Oculus_CrossPlatform_PrimaryThumbstickHorizontal";
        public static string occulusLeftStickVerticalAxis = "Oculus_CrossPlatform_PrimaryThumbstickVertical";

        public static string occulusRightStickButton = "Oculus_CrossPlatform_SecondaryThumbstick";
        public static string occulusRightStickHorizontalAxis = "Oculus_CrossPlatform_SecondaryThumbstickHorizontal";
        public static string occulusRightStickVerticalAxis = "Oculus_CrossPlatform_SecondaryThumbstickVertical";
        #endregion
        public bool debugMode = false;
        public GameObject debugObject;
        public GameObject vrDebugObject;
        public KeyCode debugToggleKey = KeyCode.BackQuote;

        private void Reset()
        {
            debugObject = debugObject ?? transform.childCount > 0 ? transform.GetChild(0).gameObject : null;
            vrDebugObject = vrDebugObject ?? transform.childCount > 1 ? transform.GetChild(1).gameObject : null;
        }

        private void Awake()
        {
            Reset();

#if !UNITY_EDITOR && UNITY_ANDROID
            debugMode = false;
#endif
            SetDebugMode(debugMode);
        }

        private void Update()
        {
            if (Input.GetKeyDown(debugToggleKey) || Input.GetButton("Oculus_CrossPlatform_SecondaryThumbstick") && Input.GetButtonDown("Oculus_CrossPlatform_PrimaryThumbstick"))
            {
                debugMode = !debugMode;

                SetDebugMode(debugMode);
            }

            if (vrDebugObject)
            {
            }
        }

        public void SetDebugMode(bool active = true)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                vrDebugObject?.gameObject.SetActive(active);
            }
            else
            {
                debugObject?.gameObject.SetActive(active);
            }
        }
    }
}