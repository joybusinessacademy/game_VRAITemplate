using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using System.Reflection;
using UnityEngine.Events;

#if DOTween
using DG.Tweening;
#endif

namespace JBA.XRPlayerPackage.XRDebug
{
    [DefaultExecutionOrder(XRInteractionUpdateOrder.k_Controllers + 1)]
    public class DebugUserHands : MonoBehaviour
    {
        public bool canMoveLeftHand = false;
        public bool canMoveRightHand = false;
        
        public SVRHands leftHand;
        public SVRHands rightHand;

        public List<GameObject> leftHandReferences = new List<GameObject>();
        public List<GameObject> rightHandReferences = new List<GameObject>();

        [SerializeField]
        private Camera mainCamera;

        private GameObject cameraGameObject => mainCamera ? mainCamera.gameObject : null;

        /// <summary>
        /// Apply subtle tween & sound effects when
        /// grabbing / dropping objects via the debug controller.
        /// </summary>
        public bool usePickupFlourishes;
        public float movementSpeed = 0.5f;
        
        public KeyCode alternateLeftControl = KeyCode.Alpha1;
        public KeyCode alternateRightControl = KeyCode.Alpha2;
        
        public KeyCode resetHandPositionKey = KeyCode.R;
        public KeyCode resetObjectOrientationKey = KeyCode.T;
        public KeyCode moveHandsToGrabbableKey = KeyCode.Mouse0;
        public KeyCode leftGrabKey = KeyCode.LeftControl;
        public KeyCode rightGrabKey = KeyCode.LeftAlt;
        public KeyCode debugBothHandsKey = KeyCode.LeftShift;
        public KeyCode debugLeftHandsKey = KeyCode.LeftBracket;
        public KeyCode debugRightHandsKey = KeyCode.RightBracket;

        private List<XRBaseInteractable> previousValidTargets = new List<XRBaseInteractable>();
        public DebugVRControlUI debugUI;

        public UnityEvent OnDefaultLeftHand;
        public UnityEvent OnDefaultRightHand;

        public UnityEvent OnAlternateLeftHand;
        public UnityEvent OnAlternateRightHand;


        private void Awake()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            leftHandReferences.ForEach(k => k.transform.SetParent(leftHand.transform));
            rightHandReferences.ForEach(k => k.transform.SetParent(rightHand.transform));
            leftHand.ResetTransform();
            rightHand.ResetTransform();
#else
            enabled = false;
#endif
        }

        private void OnSelectActivate(XRController controller, string key = "select")
        {
            controller.GetComponent<XRBaseController>().GetControllerState(out var controllerState);
            switch (key)
            {
                case "select":
                    controllerState.selectInteractionState = new InteractionState
                    {
                        activatedThisFrame = true,
                        active = true
                    }; ;
                    break;

                case "uiPress":
                    controllerState.uiPressInteractionState = new InteractionState
                    {
                        activatedThisFrame = true,
                        active = true
                    }; ;
                    break;

                case "activate":
                    controllerState.activateInteractionState = new InteractionState
                    {
                        activatedThisFrame = true,
                        active = true
                    }; ;
                    break;
            }

            MethodInfo updateTrackingInput = typeof(XRBaseController).GetMethod("UpdateTrackingInput", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo applyControllerState = typeof(XRBaseController).GetMethod("ApplyControllerState", BindingFlags.NonPublic | BindingFlags.Instance);

            updateTrackingInput.Invoke(controller, new object[] { controllerState });
            applyControllerState.Invoke(controller, new object[] { XRInteractionUpdateOrder.UpdatePhase.Dynamic, controllerState });
        }


        private void Update()
        {
            // Grab with Hands.
            {
                if (canMoveLeftHand)
                {
                    GrabOrRelease(leftHand, Input.GetKey(leftGrabKey), ref previousState);
                }

                if (canMoveRightHand)
                {
                    GrabOrRelease(rightHand, Input.GetKey(rightGrabKey), ref previousState);
                }
            }

            if (Input.GetKey(KeyCode.Space))
            {
                XRController controller = GameObject.Find("RightPointController").gameObject.GetComponent<XRController>();
                OnSelectActivate(controller, "select");
                OnSelectActivate(controller, "uiPress");
            }

            if (Input.GetKey(KeyCode.I))
            {
                XRController controller = Input.GetKey(leftGrabKey) ? leftHand.GetComponent<XRController>() : rightHand.GetComponent<XRController>();
                OnSelectActivate(controller, "activate");
            }

            // Reset Hands.
            {
                if (Input.GetKeyDown(resetHandPositionKey))
                {
                    leftHand.ResetTransform();
                    rightHand.ResetTransform();
                }
            }

            // Project Hand(s) out.
            {
                if (Input.GetKey(moveHandsToGrabbableKey))
                {
                    RaycastHit[] raycastHitTargets = Physics.RaycastAll(mainCamera.ScreenPointToRay(Input.mousePosition));

                    RaycastHit raycastHit = raycastHitTargets.ToList().Find(k => k.transform.gameObject.GetComponent<XRBaseInteractable>());
                    if (raycastHit.collider)
                    {
                        if (canMoveLeftHand)
                        {
                            leftHand.transform.position = raycastHit.point;
                        }
                        if (canMoveRightHand)
                        {
                            rightHand.transform.position = raycastHit.point;
                        }
                    }
                }
            }

            // Hover over object with Hands.
            {
                if (leftHand.ValidTargets.Count > 0 && !leftHand.ValidTargets.SequenceEqual(previousValidTargets))
                {
                    debugUI?.UpdateUI();
                }

                previousValidTargets = leftHand.ValidTargets;
            }

            // Activate Hands.
            {
                if (Input.GetKeyDown(debugLeftHandsKey))
                {
                    canMoveLeftHand = true;
                    canMoveRightHand = false;
                }

                if (Input.GetKeyDown(debugRightHandsKey))
                {
                    canMoveLeftHand = false;
                    canMoveRightHand = true;
                }

                if (Input.GetKeyDown(debugBothHandsKey))
                {
                    bool toggle = canMoveLeftHand;

                    canMoveLeftHand = !toggle;
                    canMoveRightHand = !toggle;
                }
            }

            // Move Hands.
            {
                if (canMoveLeftHand)
                    UpdatePosition(leftHand.transform);

                if (canMoveRightHand)
                    UpdatePosition(rightHand.transform);
            }
        }

        private bool previousState;

        private void OnEnable()
        {
            canMoveLeftHand = true;
            canMoveRightHand = true;
        }

        private void OnDisable()
        {
            ResetHandsPosition();
        }

        private void UpdatePosition(Transform hand)
        {
            float movementSpeed = this.movementSpeed * Time.deltaTime;

            float forwardKey = Input.GetKey(KeyCode.UpArrow) ? 1 : Input.GetKey(KeyCode.DownArrow) ? -1 : 0;
            float sidewaysKey = Input.GetKey(KeyCode.RightArrow) ? 1 : Input.GetKey(KeyCode.LeftArrow) ? -1 : 0;
            float deltaMouseScrollWheel = Input.GetAxis("Mouse ScrollWheel") * 100f;
            float upwardKey = (Input.GetKey(KeyCode.Keypad0) || Input.GetKey(KeyCode.Insert)) ? 1 : Input.GetKey(KeyCode.RightControl) ? -1 : 0;

            Quaternion rotation = cameraGameObject.transform.rotation;
            Vector3 fwdMove = rotation * Vector3.forward * ((forwardKey + deltaMouseScrollWheel) * movementSpeed);
            Vector3 strafeMove = rotation * Vector3.right * (sidewaysKey * movementSpeed);
            Vector3 upwdMove = rotation * Vector3.up * (upwardKey * movementSpeed);

            hand.position += fwdMove + strafeMove + upwdMove;
        }

        private void GrabOrRelease(SVRHands wrapperHand, bool valid, ref bool previousState)
        {
            XRController hand = wrapperHand.GetComponent<XRController>();

            if (valid)
            {
                OnSelectActivate(hand, "select");
            }

            if (previousState == valid || wrapperHand.InteractingWith == null)
            {
                return;
            }

            previousState = valid;

            XRBaseInteractable previousObject = wrapperHand.InteractingWith;

            // Have we grabbed or released something?
            if (previousObject != wrapperHand.InteractingWith)
            {
                debugUI.UpdateUI();
            }

            if (usePickupFlourishes)
            { // DOTween flair.
#if DOTween
                bool hasGrabbed = wrapperHand.InteractingWith != null;

                if (hasGrabbed)
                {
                    wrapperHand.InteractingWith.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 15, 1).SetAutoKill();

                    { // audio
                        var clip = Resources.Load<AudioClip>("Pickup_Heart");
                        var audioSource = this.GetOrAddComponent<AudioSource>();
                        audioSource.pitch = 1f;
                        audioSource.PlayOneShot(clip);
                    }
                }
                else if (previousObject)
                {
                    previousObject.transform.DOBlendablePunchRotation(Vector3.up * 15f, 0.2f, 20, 0.5f).SetAutoKill();

                    { // audio
                        var clip = Resources.Load<AudioClip>("Pickup_Heart");
                        var audioSource = this.GetOrAddComponent<AudioSource>();
                        audioSource.pitch = 0.75f;
                        audioSource.PlayOneShot(clip);
                    }
                }
#endif
            }
        }

        public void ResetHandsPosition()
        {
            if (canMoveLeftHand)
            {
                leftHand.transform.localPosition = new Vector3(0, -0.15f, 0.25f);
                leftHand.transform.localEulerAngles = Vector3.zero;
            }

            if (canMoveRightHand)
            {
                rightHand.transform.localPosition = new Vector3(0, -0.15f, 0.25f);
                rightHand.transform.localEulerAngles = Vector3.zero;
            }
        }
    }
}