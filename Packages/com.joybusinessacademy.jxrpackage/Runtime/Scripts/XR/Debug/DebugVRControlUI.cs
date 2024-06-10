using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JBA.XRPlayerPackage.XRDebug
{
    public class DebugVRControlUI : MonoBehaviour
    {
        public DebugControlManager debugControlManager;
        public DebugCameraControl debugCameraControl;
        public DebugUserHands debugUserHands;
        
        public Text displayText;

        private string debugKey;
        private string cameraControlKey;
        private string cameraAccelerateKey;
        private string debugBothHandsKey;
        private string leftGrabKey;
        private string rightGrabKey;
        private string debugResetHands;
        private string debugLeftHand;
        private string debugRightHand;
        private string snapToNearestGrabbableObject;
        private string debugHandsDetails;
        private string currentlyHeldObject;
        private string grabCandidates;
        private string debugRaycastDetails;
        private string debugSkipPhaseDetails = string.Empty;

        private void Reset()
        {
            debugControlManager = debugControlManager ?? FindObjectOfType<DebugControlManager>();
            debugCameraControl = debugCameraControl ?? FindObjectOfType<DebugCameraControl>();
            debugUserHands = debugUserHands ?? FindObjectOfType<DebugUserHands>();

            displayText = displayText ?? GetComponentInChildren<Text>();

            UpdateUI();
        }

        private void OnEnable()
        {
            Reset();
        }
        
        public void UpdateUI()
        {
            #region In VR
            if (Application.platform == RuntimePlatform.Android)
            {
                {
                    debugKey = string.Format("Stop Debug Mode - 'Hold <b>Right Stick</b> and Press <b>Left Stick</b>'");
                    cameraControlKey = string.Format("\nMovement (Forward/Back/Left/Right) - '<b>Left Stick</b>'\nMovement (Up/Down) - '<b>Right Stick (Up/Down)</b>'");
                    cameraControlKey += string.Format("\nRotate (Left/Right) - '<b>Right Stick (Left/Right)</b>'");
                    cameraAccelerateKey = string.Format("\nAccelerate - 'Hold <b>Left Stick</b>'");

                    displayText.text = debugKey + cameraControlKey + cameraAccelerateKey + debugSkipPhaseDetails;
                }
                return;
            }
            #endregion

            #region On Desktop
            // Debug Control Manager.
            {
                debugKey = debugControlManager.debugToggleKey.ToString();
                debugKey = string.Format("Stop Debug Mode - '<b>{0}</b>'", debugKey);
            }

            // Debug Camera
            {
                cameraControlKey = debugCameraControl.cameraKey.ToString();
                cameraAccelerateKey = debugCameraControl.accelerationKey.ToString();
                cameraControlKey = string.Format("\nMOVEMENT CONTROLS\nMove Character -'<b>{0}</b>' HOLD + <b>WASDQE</b>\nAccelerate - '<b>{1}</b>'\n", cameraControlKey, cameraAccelerateKey);
            }

            // Debug hands.
            {
                debugBothHandsKey = debugUserHands.debugBothHandsKey.ToString();
                leftGrabKey = debugUserHands.leftGrabKey.ToString();
                rightGrabKey = debugUserHands.rightGrabKey.ToString();
                debugResetHands = debugUserHands.resetHandPositionKey.ToString();
                debugLeftHand = debugUserHands.debugLeftHandsKey.ToString();
                debugRightHand = debugUserHands.debugRightHandsKey.ToString();
                snapToNearestGrabbableObject = debugUserHands.moveHandsToGrabbableKey.ToString();

                debugHandsDetails = string.Format("\n\nHAND CONTROLS\nDebug Both Hands(Toggle) - <b>{0}</b>" +
                    "\nMove Hand(s) - <b>Arrow Keys, RightControl and Keypad0</b>" +
                    "\nLeft Grab Key - <b>{1}</b>\nRight Grab Key - <b>{2}</b>\nReset Hands Position - <b>{3}</b>\nDebug Left Hand - <b>{4}</b>\nDebug Right Hand - <b>{5}</b>" +
                    "\nSnap to Nearest Grabbable Object - <b>{6}</b>\n"
                    , debugBothHandsKey, leftGrabKey, rightGrabKey, debugResetHands, debugLeftHand, debugRightHand, snapToNearestGrabbableObject);
            }

            // Currently held object.
            if (debugUserHands.leftHand.InteractingWith || debugUserHands.leftHand.InteractingWith)
            {
                currentlyHeldObject = string.Format("Holding: <b>{0}</b>\n",
                    (debugUserHands.leftHand.InteractingWith ? debugUserHands.leftHand.InteractingWith.name : ""));
            }

            // Grab candidates.
            {
                grabCandidates = string.Format("Can grab: <b>{0}</b>\n", ToStringOfObjectsWithLinebreaks(debugUserHands.leftHand.ValidTargets));
            }


            if (displayText)
                displayText.text = debugKey + cameraControlKey + debugHandsDetails + debugRaycastDetails + currentlyHeldObject + grabCandidates + debugSkipPhaseDetails;

            #endregion
        }

        public string ToStringOfObjectsWithLinebreaks<T>(ICollection<T> objects)
        {
            string objectsString = "";

            foreach (var obj in objects)
                objectsString += obj.ToString() + "\n";

            return objectsString.TrimEnd();
        }
    }
}

