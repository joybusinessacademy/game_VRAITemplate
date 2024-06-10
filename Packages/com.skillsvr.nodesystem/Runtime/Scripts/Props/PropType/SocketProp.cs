using Props;
using Props.PropInterfaces;
using SkillsVRNodes.Scripts.Nodes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SocketProp : PropType, IPropSocketInteractor
{
    public override string name => "Interactable Prop";

    public bool BeingUsed { get => beingUsed; set => beingUsed = value; }

    public GroupSocketFilter filter;
    public XRSocketInteractor socket;
    public SVRColorMaterialPropertyAffordanceReceiver colorReciever;
    public Rigidbody rigidBody;
    public Collider collider;

    public Color correctFeedback;
    public Color incorrectFeedback;

    private bool beingUsed;
    private List<DragDropNode> nodesReferencingMe = new List<DragDropNode>();

    public SocketProp(PropComponent propComponent) : base(propComponent)
    {
    }

    public override void AutoConfigProp()
    {
        //base.AutoConfigProp();

        filter = propComponent.GetComponent<GroupSocketFilter>();
        socket = propComponent.GetComponent<XRSocketInteractor>();

        colorReciever = propComponent.GetComponent<SVRColorMaterialPropertyAffordanceReceiver>();
    }

    public void MuteInScene(DragDropNode referencer = null)
    {
        if (nodesReferencingMe == null)
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
            collider.enabled = false;
        }
    }
    public void UnmuteInScene(DragDropNode referencer)
    {
        collider.enabled = true;
        nodesReferencingMe.Add(referencer);
    }

    public GroupSocketFilter GetSocketFilter()
    {
        return filter;
    }

    public XRSocketInteractor GetSocketInteractor()
    {
        return socket;
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

    public void ShowCorrect(float duration)
    {
        colorReciever.TriggerColorState(correctFeedback, duration);
    }

    public void ShowIncorrect(float duration)
    {
        colorReciever.TriggerColorState(incorrectFeedback, duration);
    }


}
