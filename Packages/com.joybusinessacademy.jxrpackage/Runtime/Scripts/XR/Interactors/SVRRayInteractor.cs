using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Linq;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace SVR.XRPlayerPackage
{
    public class SVRRayInteractor : XRRayInteractor, ILineRenderable
    {
        private XRUIInputModule refInputModule = null;
        private EventSystem eventSystem;
        private const string inputModule = "m_InputModule";

        public bool interactNearest = true;

        protected override void Awake()
        {

            var attachGO = new GameObject($"[{gameObject.name}] Copy Attach");
            attachGO.transform.SetParent(transform);
            attachGO.transform.position = attachTransform.position;

            rayOriginTransform = attachGO.transform;
            base.Awake();
        }

        private void FindXRInputModule()
        {
            refInputModule = refInputModule ? refInputModule : typeof(XRRayInteractor).GetField(inputModule, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(this) as XRUIInputModule;
            if (refInputModule)
            {
                if (refInputModule != null)
                {
                    eventSystem = eventSystem ? eventSystem : refInputModule.GetComponent<EventSystem>();
                }
            }
            else
            {
                eventSystem = EventSystem.current;
            }
        }

        private bool CheckUIInteraction(IXRHoverInteractable interactable)
        {
            FindXRInputModule();

            // ui is the closest
            if (enableUIInteraction && interactNearest && TryGetUIModel(out TrackedDeviceModel model))
            {
                TrackedDeviceEventData temp = new TrackedDeviceEventData(eventSystem);
                model.CopyTo(temp);

                if (temp.pointerEnter != null)
                {
                    return false;
                }
            }

            return !interactNearest || interactablesHovered.FirstOrDefault() == interactable || interactablesHovered.FirstOrDefault() == null;
        }
        
        /// <summary>Determines if the interactable is valid for hover this frame.</summary>
        /// <param name="interactable">Interactable to check.</param>
        /// <returns><c>true</c> if the interactable can be hovered over this frame.</returns>
        public override bool CanHover(IXRHoverInteractable interactable)
        {
            return CheckUIInteraction(interactable) && base.CanHover(interactable);
        }

        public override bool CanSelect(XRBaseInteractable interactable)
        {
            return CheckUIInteraction(interactable) && base.CanSelect(interactable);
        }

        public bool GetLinePoints(ref Vector3[] linePoints, ref int noPoints)
        {
            GetValidTargets(new List<XRBaseInteractable>());
            return base.GetLinePoints(ref linePoints, out noPoints);
        }

        public bool TryGetHitInfo(ref Vector3 position, ref Vector3 normal, ref int positionInLine, ref bool isValidTarget)
        {
            return base.TryGetHitInfo(out position, out normal, out positionInLine, out isValidTarget);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (GetComponent<XRInteractorLineVisual>() != null && GetComponent<XRInteractorLineVisual>().reticle != null)
            {
                GetComponent<XRInteractorLineVisual>().reticle.SetActive(false);
            }
        }
    }
}
