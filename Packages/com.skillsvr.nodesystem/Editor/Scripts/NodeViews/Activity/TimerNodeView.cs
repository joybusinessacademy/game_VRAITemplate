using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using Props.PropInterfaces;
using Scripts.VisualElements;
using SkillsVR.VisualElements;
using SkillsVRNodes.Editor.Graph;
using SkillsVRNodes.Scripts.Nodes;
using Unity.EditorCoroutines.Editor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine.UIElements;
using VisualElements;

namespace SkillsVRNodes.Editor.NodeViews
{
	[NodeCustomEditor(typeof(TimerNode))]
	public class TimerNodeView : SpawnerNodeView<SpawnerTimer, ITimerSystem, TimerData>
	{
		public TimerNode AttachedNode => nodeTarget as TimerNode;

		private FloatField timerCountdownInSeconds;
		public override VisualElement GetNodeVisualElement()
		{
			return null;
		}
		
		public override VisualElement GetInspectorVisualElement()
		{
			var visualElement = new VisualElement();
			visualElement.Add(CreateTransformDropdown<IPropPanel>(AttachedNode));
			visualElement.Add(new Divider());
			
			var countDownToggle = new Toggle("Count Downwards to Target: ");
			countDownToggle.tooltip = "Ticked: Will Count Down \n Unticked: Will count up from zero";


			countDownToggle.value = AttachedNode.mechanicData.countDownTime;
			countDownToggle.RegisterValueChangedCallback(evt =>
			{
				AttachedNode.mechanicData.countDownTime = evt.newValue;	
			});

			visualElement.Add(countDownToggle);
			
			FloatField timerCountdownInSeconds = new("Amount of Time (In Seconds): ");

			if (AttachedNode.mechanicData.amountOfTimeInSeconds == 0)
				timerCountdownInSeconds.value = 10;
			else
				timerCountdownInSeconds.value = AttachedNode.mechanicData.amountOfTimeInSeconds;
			timerCountdownInSeconds.RegisterValueChangedCallback(evt =>
			{
				AttachedNode.mechanicData.amountOfTimeInSeconds = evt.newValue;
			});

			visualElement.Add(timerCountdownInSeconds);
			
			return visualElement;
		}


	}
}
