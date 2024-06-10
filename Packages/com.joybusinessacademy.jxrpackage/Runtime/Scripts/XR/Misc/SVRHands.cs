using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace JBA.XRPlayerPackage
{
    [RequireComponent(typeof(XRDirectInteractor))]
    public class SVRHands : MonoBehaviour
    {
        [SerializeField]
        private XRDirectInteractor directInteractor;
        public XRBaseInteractable InteractingWith => directInteractor != null ? directInteractor.selectTarget : null;

        [SerializeField]
        private List<ButtonStates> buttonMap = new List<ButtonStates>();
        private Dictionary<ButtonStates, bool> buttonMapPreviousState = new Dictionary<ButtonStates, bool>();

        private Material handMaterialInstance;

        [SerializeField] private Renderer handRenderer;
        [SerializeField] private Color active;
        [SerializeField] private Color idle;

        [SerializeField] public HandsAnimatorControl selfAnimator;

        public List<XRBaseInteractable> ValidTargets
        {
            get
            {
                List<XRBaseInteractable> valid = new List<XRBaseInteractable>();
                if (Application.isPlaying == false)
                    return valid;

                directInteractor.GetValidTargets(valid);
                return valid;
            }
        }

        private Dictionary<GameObject, int> allChildGameObjectsLayerPair = new Dictionary<GameObject, int>();

        public ButtonStates GetButtonState(InputHelpers.Button button)
        {
            return buttonMap.Find(k => k.button == button);
        }

        private void Awake()
        {
            directInteractor = directInteractor ?? GetComponent<XRDirectInteractor>();
            buttonMap.ForEach(k => buttonMapPreviousState.Add(k, false));

            handMaterialInstance = handRenderer.material;
            handRenderer.sharedMaterial = handMaterialInstance;

            var allChildGameObjects = transform.GetComponentsInChildren<Transform>(true).ToList().Select(k => k.gameObject).ToList();
            allChildGameObjects.ForEach(k =>
            {
                allChildGameObjectsLayerPair.Add(k, k.layer);
            });
        }

        public void ResetTransform()
        {
            transform.localPosition = new Vector3(0, -0.15f, 0.25f);
            transform.localEulerAngles = Vector3.zero;
        }

        public void SetHandState(bool active)
        {
            //handMaterialInstance.SetColor("_BaseColor", active ? this.active : idle);
        }

        private void OnApplicationFocus(bool focus)
        {
            if (Application.platform != RuntimePlatform.Android)
                return;

            allChildGameObjectsLayerPair.ToList().ForEach(pair =>
            {
                pair.Key.layer = focus ? pair.Value : LayerMask.NameToLayer("DontRender");
            });
        }

        public void UpdateButtons(InputDevice inputDevice)
        {
            buttonMap.ForEach(b =>
            {
                inputDevice.IsPressed(b.button, out bool jx);
                if (buttonMapPreviousState[b] != jx)
                {
                    if (jx)
                        b.onPressed.Invoke();
                    else
                        b.onRelease.Invoke();

                    buttonMapPreviousState[b] = jx;
                }
            });
        }
        
        public void Toggle(InputHelpers.Button button)
        {
            var b = buttonMap.Find(k => k.button == button);
            bool jx = !buttonMapPreviousState[b];
            if (buttonMapPreviousState[b] != jx)
            {
                if (jx)
                    b.onPressed.Invoke();
                else
                    b.onRelease.Invoke();

                buttonMapPreviousState[b] = jx;
            }
        }
        
        public void Press(InputHelpers.Button button)
        {
            var b = buttonMap.Find(k => k.button == button);
            if (b != null)
                b.onPressed.Invoke();
        }

        [System.Serializable]
        public class ButtonStates
        {
            public InputHelpers.Button button;
            public UnityEvent onPressed;
            public UnityEvent onRelease;
        }
    }
}
