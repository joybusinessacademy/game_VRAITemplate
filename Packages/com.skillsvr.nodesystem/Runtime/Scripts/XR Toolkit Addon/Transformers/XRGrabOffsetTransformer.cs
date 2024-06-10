using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class XRGrabOffsetTransformer : XRSkillsBaseTransformer
{

    [Tooltip("Offset from the grabbing hand's attach point")]
    public Vector3 grabOffset = new Vector3();


    [Tooltip("If interactable is not set to instantanious movement, this speed describes it's rate of movement. Overrides the Attach Ease In Time of the interactable")]
    public float attachEaseInSpeed = 5;

    private IXRSelectInteractor interactor;
    private bool isInSocket;
    private Vector3 currentOffsetPosition;
    


    public override void Process(XRGrabInteractable grabInteractable, XRInteractionUpdateOrder.UpdatePhase updatePhase, ref Pose targetPose, ref Vector3 localScale)
    {
        if (QueryPriority(grabInteractable))
        {
            if (!isInSocket)
                UpdateTargetPosition(grabInteractable, ref targetPose, ref localScale);
            else
                targetPose.position = currentOffsetPosition;
        }


    }


    private void UpdateTargetPosition(XRGrabInteractable grabInteractable, ref Pose targetPose, ref Vector3 localScale)
    {

        currentOffsetPosition = interactor.transform.forward * grabOffset.z;
        currentOffsetPosition += interactor.transform.right * grabOffset.x;
        currentOffsetPosition += interactor.transform.up * grabOffset.y;
        currentOffsetPosition = interactor.transform.position + currentOffsetPosition;

        if (grabInteractable.movementType == XRBaseInteractable.MovementType.Instantaneous)
        {
            targetPose.position = currentOffsetPosition;
        }
        else
        {
            targetPose.position = Vector3.Lerp(targetPose.position, currentOffsetPosition, Time.deltaTime * attachEaseInSpeed);
        }
    }

    public override void OnGrab(XRGrabInteractable grabInteractable)
    {
        base.OnGrab(grabInteractable);

        grabInteractable.attachEaseInTime = 1;
        foreach (var item in grabInteractable.interactorsSelecting)
        {
            if (item as XRRayInteractor)
            {
                interactor = item;
            }
        }
    }

    public override void OnGrabCountChanged(XRGrabInteractable grabInteractable, Pose targetPose, Vector3 localScale)
    {
        isInSocket = false;
        foreach (var item in grabInteractable.interactorsSelecting)
        {
            if (item as XRSocketInteractor)
            {
                isInSocket = true;
                currentOffsetPosition = ((item as XRSocketInteractor).attachTransform == null ? Vector3.zero : (item as XRSocketInteractor).attachTransform.transform.position);
                break;
            }
        }       

        base.OnGrabCountChanged(grabInteractable, targetPose, localScale);
    }
}
