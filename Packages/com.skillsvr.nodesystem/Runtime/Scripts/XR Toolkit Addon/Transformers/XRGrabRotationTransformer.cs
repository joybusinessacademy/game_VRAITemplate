using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class XRGrabRotationTransformer : XRSkillsBaseTransformer
{
    public RotationAxis rotateAround;
    public float rotationSpeed;
    public bool autoRotate;

    private IXRSelectInteractor interactor;
    private XRRayInteractor rayInteractor;
    private bool isInSocket;
    private float rotateAngle;
    private Quaternion currentRotation;

    private Transform localTransform;

    public override void Process(XRGrabInteractable grabInteractable, XRInteractionUpdateOrder.UpdatePhase updatePhase, ref Pose targetPose, ref Vector3 localScale)
    {
        if (QueryPriority(grabInteractable))
        {
            if (!isInSocket)
                UpdateTargetRotation(grabInteractable, ref targetPose, ref localScale);
        }
    }

    private void UpdateTargetRotation(XRGrabInteractable grabInteractable, ref Pose targetPose, ref Vector3 localScale)
    {
        if (rotateAround == RotationAxis.INTERACTOR_UP)
            targetPose.rotation = interactor.transform.rotation;

        if (rotateAround == RotationAxis.LOCAL_UP)
        {
            targetPose.rotation = localTransform.rotation;
            return;
        }

        if (rotateAround == RotationAxis.WORLD_UP)
            targetPose.rotation.eulerAngles = Vector3.up;


        if (autoRotate)
        {
            rotateAngle += 1 * (rotationSpeed * Time.deltaTime);
            rotateAngle = (rotateAngle % 360 + 360) % 360;

            currentRotation = Quaternion.AngleAxis(rotateAngle, Vector3.up);
            targetPose.rotation = targetPose.rotation * currentRotation;
        }
        else
        {

#if UNITY_EDITOR
            ControlRotationInEditor(ref targetPose);
            return;
#endif

            if (rayInteractor.allowAnchorControl && rayInteractor.hasSelection)
            {

                if (rayInteractor.TryGetComponent<XRController>(out XRController m_DeviceBasedController))
                {
                    if (m_DeviceBasedController.inputDevice.isValid)
                    {
                        m_DeviceBasedController.inputDevice.IsPressed(m_DeviceBasedController.rotateObjectLeft, out var leftPressed, m_DeviceBasedController.axisToPressThreshold);
                        m_DeviceBasedController.inputDevice.IsPressed(m_DeviceBasedController.rotateObjectRight, out var rightPressed, m_DeviceBasedController.axisToPressThreshold);
                        if (leftPressed || rightPressed)
                        {
                            var directionAmount = leftPressed ? -1f : 1f;

                            if (Mathf.Approximately(directionAmount, 0f))
                                return;

                            rotateAngle += directionAmount * (rotationSpeed * Time.deltaTime);

                            rotateAngle = (rotateAngle % 360 + 360) % 360;

                            currentRotation = Quaternion.AngleAxis(rotateAngle, Vector3.up);
                            targetPose.rotation = targetPose.rotation * currentRotation;
                        }
                    }
                }
            }
        }

    }

    private void ControlRotationInEditor(ref Pose targetPose)
    {
        if (Input.GetKey(KeyCode.G))
        {
            var directionAmount = -1;

            if (Mathf.Approximately(directionAmount, 0f))
                return;

            rotateAngle += directionAmount * (rotationSpeed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.H))
        {
            var directionAmount = 1;

            if (Mathf.Approximately(directionAmount, 0f))
                return;

            rotateAngle += directionAmount * (rotationSpeed * Time.deltaTime);
        }

        rotateAngle = (rotateAngle % 360 + 360) % 360;

        currentRotation = Quaternion.AngleAxis(rotateAngle, Vector3.up);
        targetPose.rotation = targetPose.rotation * currentRotation;
    }

    public override void OnGrab(XRGrabInteractable grabInteractable)
    {
        base.OnGrab(grabInteractable);
        interactor = grabInteractable.interactorsSelecting[0];
        isInSocket = true;

        foreach (var item in grabInteractable.interactorsSelecting)
        {
            if (item as XRRayInteractor)
            {
                interactor = item;
                rayInteractor = interactor.transform.gameObject.GetComponent<XRRayInteractor>();

                if (rotateAround == RotationAxis.LOCAL_UP)
                {  
                    if (localTransform == null)
                    {
                        localTransform = new GameObject("Local Up").transform;
                    }
                    localTransform.parent = item.transform;
                    localTransform.rotation = transform.rotation;
                    localTransform.position = transform.position;
                }

                isInSocket = false;
            }
        }
    }



    public override void OnLink(XRGrabInteractable grabInteractable)
    {
        base.OnLink(grabInteractable);
    }


    public override void OnUnlink(XRGrabInteractable grabInteractable)
    {
        base.OnUnlink(grabInteractable);
        rotateAngle = 0;
    }
}


public enum RotationAxis
{
    WORLD_UP,
    INTERACTOR_UP,
    LOCAL_UP
}
