using SkillsVRNodes.Editor.NodeViews.Validation.Impl;
using SkillsVRNodes.Editor.NodeViews.Validation;
using SkillsVRNodes.Editor.NodeViews;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SkillsVRNodes.Editor.NodeViews.Telemetry;
using UnityEngine.UIElements;
using SkillsVRNodes.Scripts.Nodes;


[CustomDataValidation(typeof(EyeTrackingNodeView))]
public class EyeTrackingNodeViewValidation : AbstractNodeViewValidation<EyeTrackingNodeView>
{
    public override VisualElement OnGetVisualSourceFromPath(string path)
    {
        return null;
    }

    public override void OnValidate()
    {
        var node = TargetNodeView.AttachedNode<EyeTrackingNode>();

        ErrorIf(node.objectInteractableInterfaces.Count <= 0, "Prop Error", "Please ensure you are seleting at least one prop for Eye tracking");
        ErrorIf(!CheckAllInteractables(node), "Prop Error", "Please ensure you are seleting at least one prop for Eye tracking");
        
        
        ErrorIf(node.currentCompletePreset == 1 && node.targetLookingTime > node.totalTimeToLookAt, "Settings Error" ,"Please ensure that the target look time is shorter than the total time to look at");
    }

    private bool CheckAllInteractables(EyeTrackingNode node)
    {
        for (int i = 0; i < node.objectInteractableInterfaces.Count; i++)
        {
            if (node.objectInteractableInterfaces[i].GetPropName() == null)
                return false;
        }

        return true;
    }
}
