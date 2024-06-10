using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRGrabScaleTransformer : XRSkillsBaseTransformer
{

    public Vector3 scaleOnGrab;    
    private Vector3 originalScale;
    private Vector3 appliedScale;

    public override void Process(XRGrabInteractable grabInteractable, XRInteractionUpdateOrder.UpdatePhase updatePhase, ref Pose targetPose, ref Vector3 localScale)
    {
        if (QueryPriority(grabInteractable))
        {
            if (appliedScale != Vector3.zero)
                localScale = appliedScale;
        }
    }

    public override void OnLink(XRGrabInteractable grabInteractable)
    {
        originalScale = gameObject.transform.localScale;
        appliedScale = originalScale;
        base.OnLink(grabInteractable);
    }

    public override void OnGrabCountChanged(XRGrabInteractable grabInteractable, Pose targetPose, Vector3 localScale)
    {
        foreach (var item in grabInteractable.interactorsSelecting)
        {
            if(item as XRRayInteractor)
            {
                (item as XRRayInteractor).selectExited.AddListener(OnObjectDroppedByHand);
                OnObjectGrabbedByHand();
            }
        }

        base.OnGrabCountChanged(grabInteractable, targetPose, localScale);
    }

    private void OnObjectGrabbedByHand()
    {
        appliedScale = scaleOnGrab;
    }

    private void OnObjectDroppedByHand(SelectExitEventArgs obj)
    {
        obj.interactorObject.selectExited.RemoveListener( OnObjectDroppedByHand);
        appliedScale = originalScale;
        (obj.interactableObject as XRGrabInteractable).transform.localScale = appliedScale;
    }
}
