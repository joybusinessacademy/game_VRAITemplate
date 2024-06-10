using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class XRSkillsBaseTransformer : XRBaseGrabTransformer
{
    /// <summary>
    /// If this bool is true, can be used to compare importance compared to all other SKILLS based transformers. Normalised state is all are false, therefore
    /// the linked order is respected.
    /// </summary>
    private bool overridePriority = false;

    public delegate void InteractionEvent(IXRInteractable interactable, IXRInteractor interactor);

    public override void Process(XRGrabInteractable grabInteractable, XRInteractionUpdateOrder.UpdatePhase updatePhase, ref Pose targetPose, ref Vector3 localScale)
    {     
    }

    protected void ForcePriority(XRGrabInteractable grabInteractable)
    {       
        NormalizePriority(grabInteractable);
        overridePriority = true;      
    }

    protected void SetPriority(XRGrabInteractable grabInteractable, bool val)
    {
        overridePriority = val;
    }

    protected void NormalizePriority(XRGrabInteractable grabInteractable)
    {
        List<IXRGrabTransformer> currentTransformers = new List<IXRGrabTransformer>();
        grabInteractable.GetSingleGrabTransformers(currentTransformers);

        foreach (var item in currentTransformers)
        {
            if (item as XRSkillsBaseTransformer)
            {
                (item as XRSkillsBaseTransformer).overridePriority = false;
            }
        }
    }

    protected bool QueryPriority(XRGrabInteractable grabInteractable)
    {
        if (overridePriority)
            return true;

        List<IXRGrabTransformer> currentTransformers = new List<IXRGrabTransformer>();
        grabInteractable.GetSingleGrabTransformers(currentTransformers);

        foreach (var item in currentTransformers)
        {
            if (item as XRSkillsBaseTransformer && (item as XRSkillsBaseTransformer) != this)
            {
                if ((item as XRSkillsBaseTransformer).overridePriority)
                    return false;
            }
        }

        return true; 
    }


    protected bool TryQueryFilterAcceptance(XRGrabInteractable grabInteractable, XRSocketInteractor socketInteractor)
    {
        if (socketInteractor.startingTargetFilter as GroupSocketFilter && grabInteractable.gameObject.TryGetComponent<SocketableTag>(out SocketableTag filterType))
        {
            if ((socketInteractor.startingTargetFilter as GroupSocketFilter).filterTypes.ContainsTag(filterType.tag))
            {
                return true;
            }
            return false;
        }
        else
            return true;
    }
}
