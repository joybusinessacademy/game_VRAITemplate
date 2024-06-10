using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

[RequireComponent(typeof(XRGrabOffsetTransformer))]
public class XRRaySocketSelectTransformer : XRSkillsBaseTransformer
{
    bool isHeldByRay;
    private XRSocketInteractor socketInteractorRestingIn;
    private XRSocketInteractor socketFoundByRayWhileHolding;
    private XRRayInteractor rayInteractor;

    [SerializeField] private GameObject associatedCollider;
    public GameObject AssociatedCollider { get => associatedCollider; set => associatedCollider = value; }

    public delegate void FeedbackSignal(IXRInteractable interactable, IXRInteractor interactor);
    public FeedbackSignal onObjectDropped = (x, y) => { };
    public bool enterSocketOnRayhitPos;
    
    private Vector3 worldRayHitPos;


    public override void Process(XRGrabInteractable grabInteractable, XRInteractionUpdateOrder.UpdatePhase updatePhase, ref Pose targetPose, ref Vector3 localScale)
    {
        if (QueryPriority(grabInteractable))
        {
            if (isHeldByRay)
                CastRayForSocket(grabInteractable, ref targetPose);
        }
    }

    private void CastRayForSocket(XRGrabInteractable grabInteractable, ref Pose targetPose)
    {
        RaycastHit[] hits;
        hits = Physics.RaycastAll(rayInteractor.transform.position, rayInteractor.transform.forward, 100.0F);

        var lastFoundSocket = socketFoundByRayWhileHolding;
        socketFoundByRayWhileHolding = null;

        foreach (var raycastHit in hits)
        {
            if (raycastHit.collider.gameObject.TryGetComponent<XRSocketInteractor>(out XRSocketInteractor socket))
            {
                if (TryQueryFilterAcceptance(grabInteractable, socket))
                {
                    socketFoundByRayWhileHolding = socket;
                    associatedCollider.transform.position = socket.attachTransform.transform.position; //raycastHit.point;
                    worldRayHitPos = raycastHit.point;
                    break;
                }
            }
        }

        if(socketFoundByRayWhileHolding==null)
            associatedCollider.transform.localPosition = Vector3.zero;
    }

    private void OnDrop(SelectExitEventArgs arg0)
    {
        //if we just dropped by a socket
        if (arg0.interactorObject as XRSocketInteractor != null)
        {
            return;
        }

        associatedCollider.transform.localPosition = Vector3.zero;

        if (socketFoundByRayWhileHolding != null)
        {
            SetPriority(arg0.interactableObject as XRGrabInteractable, false);
            //here, move the attach transfrom of the socket, if the node setting is true 
            if(enterSocketOnRayhitPos)
            {
                socketFoundByRayWhileHolding.attachTransform.position = worldRayHitPos;
            }

            socketFoundByRayWhileHolding.interactionManager.SelectEnter(socketFoundByRayWhileHolding, arg0.interactableObject);
            isHeldByRay = false;
            socketFoundByRayWhileHolding = null;
        }
        else
       { //case for dropping object with no ray detected socket registered
            onObjectDropped.Invoke(arg0.interactableObject, null);
        }
    }

    public override void OnLink(XRGrabInteractable grabInteractable)
    {
        grabInteractable.selectExited.AddListener(OnDrop);     
        base.OnLink(grabInteractable);
    }

    public override void OnUnlink(XRGrabInteractable grabInteractable)
    {
        grabInteractable.selectExited.RemoveListener(OnDrop);
        base.OnUnlink(grabInteractable);
    }

    public override void OnGrabCountChanged(XRGrabInteractable grabInteractable, Pose targetPose, Vector3 localScale)
    {
        foreach (var item in grabInteractable.interactorsSelecting)
        {
            if (item as XRSocketInteractor)
            {
                
                isHeldByRay = false;
                socketInteractorRestingIn = item as XRSocketInteractor;
                if (item.transform.gameObject.TryGetComponent<Collider>(out Collider collider))
                {
                    collider.enabled = false;
                    //socket logic takes priority so end if we ever find one
                    break;
                }
            }

            if (item as XRRayInteractor)

            {
                rayInteractor = item as XRRayInteractor;
                isHeldByRay = true;
            }
        }

        if (isHeldByRay && socketInteractorRestingIn != null)
        {
            if (socketInteractorRestingIn.gameObject.TryGetComponent<Collider>(out Collider collider))
            {
                collider.enabled = true;
            }

            socketInteractorRestingIn = null;
        }

        base.OnGrabCountChanged(grabInteractable, targetPose, localScale);
    }

}
