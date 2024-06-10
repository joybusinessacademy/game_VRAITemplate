using Props;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Rendering;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;

[System.Serializable]
public class InteractablePropComponentData
{
     public InteractableProp prop;

    public void GenInteractable(GameObject targetPrefab)
    {
        PropComponent propScript = (PropComponent)targetPrefab.AddComponent<PropComponent>();
        propScript.propType = new InteractableProp(propScript);
        prop = propScript.propType as InteractableProp;


        //interactable
        prop.grabInteractable = targetPrefab.AddComponent<XRGrabInteractable>();
        GameObject attachObject = new GameObject("Attach Point");
        attachObject.transform.parent = targetPrefab.transform;
        prop.grabInteractable.attachTransform = attachObject.transform;
        prop.raySocket = targetPrefab.AddComponent<XRRaySocketSelectTransformer>();
        if(targetPrefab.TryGetComponent<XRGrabOffsetTransformer>(out XRGrabOffsetTransformer gt)){
            prop.grabOffset = gt;
        }
        else
        {
            prop.grabOffset = targetPrefab.AddComponent<XRGrabOffsetTransformer>();
        }
        prop.grabRotation = targetPrefab.AddComponent<XRGrabRotationTransformer>();
        prop.grabScale = targetPrefab.AddComponent<XRGrabScaleTransformer>();
        prop.mover = targetPrefab.AddComponent<InteractableMover>();
        prop.socketableTag = targetPrefab.AddComponent<SocketableTag>();
        prop.rigidBody = targetPrefab.GetComponent<Rigidbody>();
        prop.rigidBody.useGravity = false;
        prop.rigidBody.isKinematic = true;

        //feedback system
        GameObject feedbackObject = new GameObject("Feedback System");
        feedbackObject.transform.parent = targetPrefab.transform;

        var stateProvidor = feedbackObject.AddComponent<XRInteractableAffordanceStateProvider>();
        stateProvidor.interactableSource = prop.grabInteractable;

        var affordanceReceiver = feedbackObject.AddComponent<SVRColorMaterialPropertyAffordanceReceiver>();
        affordanceReceiver.affordanceStateProvider = stateProvidor;
        affordanceReceiver.materialPropertyBlockHelper = feedbackObject.GetComponent<MaterialPropertyBlockHelper>();//should be added auto
        affordanceReceiver.colorPropertyName = "_BaseColor";
        affordanceReceiver.materialPropertyBlockHelper.rendererTarget = targetPrefab.GetComponent<MeshRenderer>();

        prop.colorReciever = affordanceReceiver;
        prop.correctFeedback = Color.green;
        prop.incorrectFeedback = Color.red;



    }



}

