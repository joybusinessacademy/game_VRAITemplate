using GraphProcessor;
using SkillsVR.Mechanic.MechanicSystems.DeepBreath;
using SkillsVR.VisualElements;
using SkillsVRNodes.Scripts.Nodes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Props.PropInterfaces;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews
{
	[NodeCustomEditor(typeof(BreathingNode))]
	public class BreathingNodeView : SpawnerNodeView<SpawnerDeepBreath, IDeepBreathSystem, DeepBreathData>
	{
		public BreathingNode AttachedNode => nodeTarget as BreathingNode;
		public override VisualElement GetNodeVisualElement()
		{
			return null;
		}
		public override VisualElement GetInspectorVisualElement()
		{
			VisualElement visualElement = new();
			visualElement.Add(CreateTransformDropdown<IPropPanel>(AttachedNode));
			visualElement.Add(CreateStyleTypeDropdown());
			visualElement.Add(new Divider());
			
			visualElement.Add(GetStyleVisualElement());

			return visualElement;
		}

		private VisualElement GetStyleVisualElement()
		{
			VisualElement visualElement = new VisualElement();
			switch (AttachedNode.MechanicData.style)
			{
				case 0:
					visualElement.Add(ShowControllerSelectionDropdown());
					visualElement.Add(AttachedNode.MechanicData.CustomFloatField(nameof(AttachedNode.MechanicData.duration), "Breathe Cycle (seconds)"));
					visualElement.Add(AttachedNode.MechanicData.CustomToggle(nameof(AttachedNode.MechanicData.autoBrathOut), "Auto Breath Out"));
					visualElement.Add(AttachedNode.MechanicData.CustomToggle(nameof(AttachedNode.MechanicData.autoHideAfterSuccess))); 
					break;
				
				case 1:
					visualElement.Add(AttachedNode.MechanicData.CustomFloatField(nameof(AttachedNode.MechanicData.breathIn), "Breathe In (seconds)"));
					visualElement.Add(AttachedNode.MechanicData.CustomFloatField(nameof(AttachedNode.MechanicData.breatheOut), "Breathe Out (seconds)"));
					visualElement.Add(AttachedNode.MechanicData.CustomFloatField(nameof(AttachedNode.MechanicData.style2Timeout), "Timeout (seconds)"));
					break;
			}

			return visualElement;
		}

		private VisualElement CreateStyleTypeDropdown()
		{
			List<string> values = new string[] {"Breathe Style 1", "Breathe Style 2"}.ToList();

			DropdownField dropdown = new DropdownField("Style Type: ", values, 0)
			{
				value = values[AttachedNode.MechanicData.style]
			};
			dropdown.RegisterValueChangedCallback((evt) =>
			{
				int index = values.IndexOf(evt.newValue);
				AttachedNode.MechanicData.style = index;
				RefreshNode();
			});

			return dropdown;
		}

		private VisualElement ShowControllerSelectionDropdown()
		{
			List<string> keyArray = AttachedNode.inputIdsRemap.Keys.ToList();

			DropdownField dropdown = new DropdownField("Select Input Type: ",keyArray,0);
			string valueToFind = AttachedNode.inputIds;

			foreach (var item in AttachedNode.inputIdsRemap)
			{
				if (item.Value == valueToFind)
				{
					dropdown.value = item.Key;
					break;
				}
			}

			dropdown.RegisterValueChangedCallback((evt) =>
			{
				AttachedNode.inputIds = AttachedNode.inputIdsRemap[evt.newValue];
			});

			return dropdown;
		}
	}
}