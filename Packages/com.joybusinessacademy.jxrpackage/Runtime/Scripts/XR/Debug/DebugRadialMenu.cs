
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JBA.XRPlayerPackage.XRDebug
{
    public class DebugRadialMenu : MonoBehaviour
    {
        public JRRadialMenu radialMenu;
        public Transform handToFollow;

        private void Awake()
        {
#if (JBADEBUG || (!ENVIRONMENT_PRODUCTION && !ENVIRONMENT_STAGING))
            radialMenu = radialMenu ?? FindObjectOfType<JRRadialMenu>();
#endif

        }

        private void OnEnable()
        {
            radialMenu?.SetParent(gameObject.transform);

            if (handToFollow && radialMenu)
            {
                radialMenu.SetParent(handToFollow);
                radialMenu.gameObject.transform.localPosition = Vector3.zero + (handToFollow.up * 0.2f);
                radialMenu.gameObject.transform.localEulerAngles = Vector3.zero;
                radialMenu.gameObject.transform.localScale = new Vector3(0.002f, 0.002f, 0.002f);
            }
        }

#if (JBADEBUG || (!ENVIRONMENT_PRODUCTION && !ENVIRONMENT_STAGING))
        private void Update()
        {
            if (!radialMenu)
                return;

            if (Input.GetButtonDown(DebugControlManager.occulusRightStickButton))
                radialMenu.gameObject.SetActive(true);
        }
#endif

        public bool Validate(List<string> outputMessages)
        {
            bool isValid = true;

            if (!radialMenu)
            {
                outputMessages.Add("radialMenu menu is null in " + gameObject.name);
                isValid = false;
            }

            if (!handToFollow && transform.parent?.parent?.parent)
            {
                outputMessages.Add("handToFollow is null in " + gameObject.name);
                isValid = false;
            }

            return isValid;
        }
    }
}
