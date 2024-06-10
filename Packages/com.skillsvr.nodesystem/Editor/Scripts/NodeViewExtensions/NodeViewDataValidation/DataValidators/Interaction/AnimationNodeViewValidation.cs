using GraphProcessor;
//using NUnit.Framework.Constraints;
using Scripts.VisualElements;
using SkillsVR.VisualElements;
using SkillsVRNodes.Scripts.Nodes;
using System.Collections.Generic;
using Props;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VisualElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
	[CustomDataValidation(typeof(AnimationNodeView))]
	public class AnimationNodeViewValidation : AbstractNodeViewValidation<AnimationNodeView>
	{
		public override void OnValidate()
		{
			var node = TargetNodeView.AttachedNode<AnimationNode>();

			string sceneAnimationKeyName = "SceneAnimation";
			string AnimationAssetKeyName = "AnimationAsset";

			Check1V1NamedSceneObjectBinding<PropComponent>(node.animationProp, 
				x => node.animationProp.GetProp().GetPropComponent() == x, 
				sceneAnimationKeyName, 
				"Assign NPC name cannot be none. Select or create a npc.");

			ErrorIf(IsNull(node.animation), AnimationAssetKeyName, "Animation cannot be none. Select an animation clip.");
			ErrorIf(IsMissingAsset(node.animation), AnimationAssetKeyName, "Animation asset is already removed.");
		}

		public override VisualElement OnGetVisualSourceFromPath(string path)
		{
			switch (path)
			{
				case "SceneAnimation": return TargetNodeView.QueryInChain<SceneElementDropdown<SceneAnimation>, DropdownField>();
				case "AnimationAsset": return TargetNodeView.QueryInChain<AssetDropdown<AnimationClip>, DropdownField>();
				default: break;
			}
			return null;
		}
	}
}
