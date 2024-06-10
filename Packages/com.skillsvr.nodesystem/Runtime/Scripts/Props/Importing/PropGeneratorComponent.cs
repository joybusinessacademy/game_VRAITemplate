using Props;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Rendering;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;

[System.Serializable]
public class PropGeneratorComponent : MonoBehaviour
{
    public PropDataScriptable source;

    private GameObject targetPrefab;
    public void Initialise(PropDataScriptable _source)
    {
        source = _source;
    }

    public void Generate(GameObject _targetPrefab)
    {
        targetPrefab = _targetPrefab;

        switch (source.propType.typeChoice)
        {
            case 0://static

                break;
            case 1://interactabel
                source.intertactableProp.GenInteractable(targetPrefab);
                GenInteractableColliderData();
                break;
            case 2://socket
                source.socketProp.GenSocket(targetPrefab);
                GenSocketColliderData();
                break;
        }

        
    }

    public void UpdatePropDetails()
    {
        switch (source.propType.typeChoice)
        {
            case 0://static

                break;
            case 1://interactabel
                UpdateInteractablePropDetails();
                break;
            case 2://socket
                UpdateSocketPropDetails();
                break;
        }
    }

    public void UpdateColliderData()
    {
        switch (source.propType.typeChoice)
        {
            case 0://static

                break;
            case 1://interactabel
                UpdateInteractableColliderData();
                PushUpdateAttachPoint(source.intertactableProp.prop.collider as BoxCollider,
                    source.intertactableProp.prop.grabInteractable.attachTransform);
                break;
            case 2://socket
                UpdateSocketColliderData();
                PushUpdateAttachPoint(source.socketProp.prop.collider as BoxCollider,
                    source.socketProp.prop.socket.attachTransform);
                break;
        }
    }

    private void GenInteractableColliderData()
    {
        if (source.colliderData == null)
            source.colliderData = new PropColliderData();

        source.colliderData.CreateInteractableColliderData(targetPrefab, source.intertactableProp.prop);
    }
    private void GenSocketColliderData()
    {
        if (source.colliderData == null)
            source.colliderData = new PropColliderData();

        source.colliderData.CreateSocketColliderData(targetPrefab, source.socketProp.prop);
    }


    private void UpdateInteractablePropDetails()
    {
        PropComponent actualPropComponent = source.intertactableProp.prop.GetPropComponent();
        PushUpdatePropDetails(actualPropComponent);
    }
    private void UpdateSocketPropDetails()
    {
        PropComponent actualPropComponent = source.socketProp.prop.GetPropComponent();
        PushUpdatePropDetails(actualPropComponent);
    }

    private void PushUpdatePropDetails(PropComponent actualPropComponent)
    {
        actualPropComponent.PropName = source.propDetails.name;
        actualPropComponent.propDescription = source.propDetails.description;
        actualPropComponent.dropPosition = (PropComponent.DropPosition)source.propType.dropPositionChoice;
    }

    private void UpdateInteractableColliderData()
    {
        PushUpdateColliderData(source.intertactableProp.prop.collider as BoxCollider);
    }
    private void UpdateSocketColliderData()
    {
        PushUpdateColliderData(source.socketProp.prop.collider as BoxCollider);
    }

    private void PushUpdateColliderData(BoxCollider col)
    {
        if (col != null)
        {
            source.colliderData.SetColliderData(col.size, col.center);
        }
    }

    private void PushUpdateAttachPoint(BoxCollider col, Transform attachTransfrom)
    { 
         attachTransfrom.position = col.bounds.center;
    }

    public PropDetailData PropDetails { get => source.propDetails; set => source.propDetails = value; }
    public PropTypeData PropType { get => source.propType; set => source.propType = value; }
   
}



