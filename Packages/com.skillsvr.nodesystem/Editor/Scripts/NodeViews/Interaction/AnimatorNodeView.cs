using System.Linq;
using GraphProcessor;
using Props;
using Props.PropInterfaces;
using SkillsVR.VisualElements;
using SkillsVRNodes.Scripts.Nodes;
using UnityEngine;
using UnityEngine.UIElements;
using VisualElements;

namespace SkillsVRNodes.Editor.NodeViews
{
	[NodeCustomEditor(typeof(AnimatorNode))]

	public class AnimatorNodeView : BaseNodeView
	{
		private VisualElement unityAnimatorContainer;
		private DropdownField animationDropdown;

		private AnimatorNode AttachedNode => AttachedNode<AnimatorNode>();
		public override VisualElement GetInspectorVisualElement()
		{
			VisualElement visualElement = new();
			
			PropDropdown<IPropAnimator> animatorDropdown = new("Animator", AttachedNode.animatorProp, evt =>
			{
				AttachedNode.animatorProp = evt;
				RefreshAnimatorDropdown();
			});
			visualElement.Add(animatorDropdown);
			unityAnimatorContainer = new VisualElement();
			visualElement.Add(unityAnimatorContainer);
			visualElement.Add(new Divider());
			
			animationDropdown = new DropdownField();
			animationDropdown.RegisterCallback<ChangeEvent<string>>(AnimationCallback);
			visualElement.Add(animationDropdown);
			RefreshAnimatorDropdown();

			return visualElement;
		}

		private void AnimationCallback(ChangeEvent<string> evt)
		{
			if (evt.newValue == NullString)
			{
				AttachedNode.stateNameHash = evt.newValue;
				AttachedNode.animatorState = AnimatorNode.AnimatorState.None;
				return;
			}

			string[] typeNamePair = evt.newValue.Split("/", 2);

			AttachedNode.animatorState = typeNamePair[0] switch
			{
				SetBoolString => AnimatorNode.AnimatorState.SetBool,
				SetIntString => AnimatorNode.AnimatorState.SetInt,
				SetFloatString => AnimatorNode.AnimatorState.SetFloat,
				SetTriggerString => AnimatorNode.AnimatorState.RunTrigger,
				_ => AttachedNode.animatorState
			};

			AttachedNode.stateNameHash = typeNamePair[1];
		}
		
		private const string NullString = "Null";
		private const string SetBoolString = "Set Bool";
		private const string SetIntString = "Set Int";
		private const string SetFloatString = "Set Float";
		private const string SetTriggerString = "Set Trigger";
		
		public void RefreshAnimatorDropdown()
		{
			animationDropdown.choices.Clear();

			IPropAnimator sceneAnimator = PropManager.GetProp(AttachedNode.animatorProp);

			
			if (sceneAnimator == null || sceneAnimator.GetAnimator().runtimeAnimatorController == null)
			{
				animationDropdown.value = "NO CONTROLLER SET";
				return;
			}

			animationDropdown.choices.Add("Null");

			AnimatorControllerParameter[] parameters = sceneAnimator.GetAnimator().parameters;
			
			foreach (AnimatorControllerParameter parameter in parameters.Where(p => p.type == AnimatorControllerParameterType.Bool))
			{
				animationDropdown.choices.Add($"{SetBoolString}/{parameter.name}");
			}
			foreach (AnimatorControllerParameter parameter in parameters.Where(p => p.type == AnimatorControllerParameterType.Int))
			{
				animationDropdown.choices.Add($"{SetIntString}/{parameter.name}");
			}
			foreach (AnimatorControllerParameter parameter in parameters.Where(p => p.type == AnimatorControllerParameterType.Float))
			{
				animationDropdown.choices.Add($"{SetFloatString}/{parameter.name}");
			}
			foreach (AnimatorControllerParameter parameter in parameters.Where(p => p.type == AnimatorControllerParameterType.Trigger))
			{
				animationDropdown.choices.Add($"{SetTriggerString}/{parameter.name}");
			}
			foreach (AnimationClip parameter in sceneAnimator.GetAnimator().runtimeAnimatorController.animationClips)
			{
				animationDropdown.choices.Add($"Play Clip/{parameter.name}");
			}
		}
	}
}