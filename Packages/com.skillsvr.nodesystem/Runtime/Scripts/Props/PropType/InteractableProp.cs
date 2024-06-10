using Props;
using Props.PropInterfaces;
using SkillsVRNodes.Scripts.Nodes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class InteractableProp : PropType, IPropGrabInteractable
{
    public override string name => "Interactable Prop";

    public bool HasHadInteractionInstance { get => hasHadInteractionInstance; set => hasHadInteractionInstance = value; }
    public bool BeingUsed { get => beingUsed; set => beingUsed = value; }
    public Vector3 IntendedWorldLocation { get => intendedWorldLocation; set => intendedWorldLocation = value; }
    public Vector3 OriginalWorldLocation { get => originalWorldLocation; set => originalWorldLocation = value; }

    public XRGrabInteractable grabInteractable;
    public XRRaySocketSelectTransformer raySocket;
    public XRGrabOffsetTransformer grabOffset;
    public XRGrabRotationTransformer grabRotation;
    public XRGrabScaleTransformer grabScale;
    public InteractableMover mover;
    public SocketableTag socketableTag;
    public SVRColorMaterialPropertyAffordanceReceiver colorReciever;


    public Rigidbody rigidBody;
    public Collider collider;

    public Color correctFeedback;
    public Color incorrectFeedback;

    /// <summary>
    /// This value will be used when props hav
    /// </summary>
    private Vector3 intendedWorldLocation = Vector3.zero;
    private Vector3 originalWorldLocation = Vector3.zero;
    private bool hasHadInteractionInstance;
    private bool beingUsed;
    private List<DragDropNode> nodesReferencingMe = new List<DragDropNode> ();

    public InteractableProp(PropComponent propComponent) : base(propComponent)
    {
    }

    public override void AutoConfigProp()
    {
        //base.AutoConfigProp();

        grabInteractable = propComponent.GetComponent<XRGrabInteractable>();
        raySocket = propComponent.GetComponent<XRRaySocketSelectTransformer>();
        grabOffset = propComponent.GetComponent<XRGrabOffsetTransformer>();
        grabRotation = propComponent.GetComponent<XRGrabRotationTransformer>();
        socketableTag = propComponent.GetComponent<SocketableTag>();
        colorReciever = propComponent.GetComponent<SVRColorMaterialPropertyAffordanceReceiver>();
        
    }
    public void MuteInScene(DragDropNode referencer = null)
    {
        if(nodesReferencingMe == null)
            nodesReferencingMe = new List<DragDropNode>();

        if (referencer != null)
        {
            if (nodesReferencingMe.Contains(referencer))
            {
                nodesReferencingMe.Remove(referencer);
            }
        }

        if (nodesReferencingMe.Count <= 0)
        {
            SetDropBehavior(1);
            collider.enabled = false;
        }
    }
    public void UnmuteInScene(DragDropNode referencer)
    {
        collider.enabled = true;
        nodesReferencingMe.Add(referencer);
    }
    public XRGrabInteractable GetIrabInteractable()
    {
        return grabInteractable;
    }

    public XRRaySocketSelectTransformer GetRaySocket()
    {
        return raySocket;
    }

    public XRGrabOffsetTransformer GetGrabOffset()
    {
        return grabOffset;
    }

    public XRGrabRotationTransformer GetGrabRotation()
    {
        return grabRotation;
    }

    public XRGrabScaleTransformer GetGrabScale()
    {
        return grabScale;
    }

    public InteractableMover GetMover()
    {
        return mover;
    }

    public SocketableTag GetSocketableTag()
    {
        return socketableTag;
    }

    public SVRColorMaterialPropertyAffordanceReceiver GetMatPropertyReciever()
    {
        return colorReciever;
    }
    public Collider GetCollider()
    {
        return collider;
    }

    public Rigidbody GetRigidBody()
    {
        return rigidBody;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="presetIndex"> 0: return to location, 1:hover, 2: drop / physics</param>
    public void SetDropBehavior(int presetIndex)
    {
        #region preset value details
        //switch (presetIndex)
        //{
        //    case 0: // return to location
        //        {
                  
        //            grabInteractable.throwOnDetach = false;
        //            rigidBody.isKinematic = true;
        //            rigidBody.useGravity = false;
        //            break;
        //        }

        //    case 1: // hover
        //        {

        //            grabInteractable.throwOnDetach = false;
        //            rigidBody.isKinematic = true;
        //            rigidBody.useGravity = false;
        //            break;
        //        }

        //    case 2: // drop / physics
        //        {

        //            grabInteractable.throwOnDetach = true;
        //            rigidBody.isKinematic = false;
        //            rigidBody.useGravity = true;
        //            break;
        //        }
        //}
        #endregion

        grabInteractable.throwOnDetach = (presetIndex == 2);
        rigidBody.isKinematic = (presetIndex != 2);
        rigidBody.useGravity = (presetIndex == 2);
    }

    public void ShowCorrect(float duration)
    {
        colorReciever.TriggerColorState(correctFeedback, duration);
    }

    public void ShowIncorrect(float duration)
    {
        colorReciever.TriggerColorState(incorrectFeedback, duration);
    }


}
