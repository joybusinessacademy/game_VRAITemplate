using GraphProcessor;
using Props;
using Props.PropInterfaces;
using Scripts.VisualElements;
using SkillsVR.VisualElements;
using SkillsVRNodes.Scripts.Nodes;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VisualElements;

namespace SkillsVRNodes.Editor.NodeViews
{
	[NodeCustomEditor(typeof(AnimationNode))]

	public class AnimationNodeView : BaseNodeView
	{
		private AnimationNode AttachedNode => AttachedNode<AnimationNode>();

		private UnityEditor.Editor gameObjectEditor;

		private AnimationPreview animationPreview;

		public override VisualElement GetNodeVisualElement()
		{
			var visualElement = new VisualElement();
			
			visualElement.Add(new TextLabel("Character", AttachedNode.animationProp?.GetProp() != null ? AttachedNode.animationProp.GetPropName() : null));
			visualElement.Add(new TextLabel("Animation", AttachedNode.animation != null ? AttachedNode.animation.name : null));
			
			return visualElement;
		}

		public override VisualElement GetInspectorVisualElement()
		{
			var visualElement = new VisualElement();
			PropDropdown<IPropAnimator> animatorDropdown = new("Assign NPC from scene: ", AttachedNode.animationProp, evt =>
			{
				AttachedNode.animationProp = evt;

				RefreshNode();
			});
			visualElement.Add(animatorDropdown);
			visualElement.Add(new Divider());
			
			if (AttachedNode.animationProp != null)
			{
				AnimationPreview animationPreview = ShowAnimationPreview();
				visualElement.Add(ShowAnimationClipField(animationPreview));
				visualElement.Add(animationPreview);

				if (AttachedNode.animationProp == null)
				{
					return visualElement;
				}

				animationPreview.ChangeGameObject(PropManager.GetProp(AttachedNode.animationProp)?.GetPropComponent()?.gameObject);
			}

			return visualElement;
		}

		private AnimationPreview ShowAnimationPreview()
		{
			IPropAnimator sceneAnimation = PropManager.GetProp(AttachedNode.animationProp);

			AnimationPreview animationPreview = new(sceneAnimation?.GetPropComponent()?.gameObject, AttachedNode.animation);

			return animationPreview;
		}

		private VisualElement ShowAnimationClipField(AnimationPreview animationPreview)
		{
			AssetDropdown<AnimationClip> animationClipDropdown = new(clip =>
			{
				AttachedNode.animation = clip;

				if (clip != null)
					animationPreview.ChangeAnimationClip(AttachedNode.animation);
			}, AttachedNode.animation, "Select Animation: ", "CharacterAnimation");

			return animationClipDropdown;
		}

		public override void Disable()
		{
			Object.DestroyImmediate(gameObjectEditor);
			base.Disable();
		}


		// public VisualElement CreateAnimationField()
		// {
		// 	ObjectField objField = new("Animation Clip:")
		// 	{
		// 		objectType = typeof(AnimationClip),
		// 		value = AttachedNode.animation
		// 	};
		// 	objField.RegisterCallback<ChangeEvent<Object>>(e =>
		// 	{
		// 		AttachedNode.animation = objField.value as AnimationClip;
		// 		animationPreview.ChangeAnimationClip(AttachedNode.animation);
		// 	});
		//
		// 	return objField;
		// }
	}
}