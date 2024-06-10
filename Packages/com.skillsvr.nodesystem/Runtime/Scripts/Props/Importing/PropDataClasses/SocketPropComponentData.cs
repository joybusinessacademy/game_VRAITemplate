using Props;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Rendering;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;

[System.Serializable]
public class SocketPropComponentData 
{
    public SocketProp prop;


    public void GenSocket(GameObject targetPrefab)
    {

        PropComponent propScript = (PropComponent)targetPrefab.AddComponent<PropComponent>();
        propScript.propType = new SocketProp(propScript);
        prop = propScript.propType as SocketProp;
            //socket
            prop.socket = targetPrefab.AddComponent<XRSocketInteractor>();
            GameObject attachpoint = new GameObject("Attach Point");
            attachpoint.transform.parent = targetPrefab.transform;
            prop.socket.attachTransform = attachpoint.transform;
            prop.filter = targetPrefab.AddComponent<GroupSocketFilter>();
            prop.rigidBody = targetPrefab.AddComponent<Rigidbody>();
            prop.rigidBody.useGravity = false;
            prop.rigidBody.isKinematic = true;


            //feedback System
            GameObject feedbackObject = new GameObject("Feedback System");
            feedbackObject.transform.parent = targetPrefab.transform;

            var stateProvidor = feedbackObject.AddComponent<XRInteractorAffordanceStateProvider>();
            stateProvidor.interactorSource = prop.socket;

            var affordanceReceiver = feedbackObject.AddComponent<SVRColorMaterialPropertyAffordanceReceiver>();
            affordanceReceiver.affordanceStateProvider = stateProvidor;
            affordanceReceiver.materialPropertyBlockHelper = feedbackObject.GetComponent<MaterialPropertyBlockHelper>();//should be added auto
            affordanceReceiver.colorPropertyName = "_BaseColor";
            affordanceReceiver.materialPropertyBlockHelper.rendererTarget = targetPrefab.GetComponent<MeshRenderer>();

            prop.colorReciever = affordanceReceiver;
            prop.correctFeedback = Color.green;
            prop.incorrectFeedback = Color.red;


       // }
    }
}
