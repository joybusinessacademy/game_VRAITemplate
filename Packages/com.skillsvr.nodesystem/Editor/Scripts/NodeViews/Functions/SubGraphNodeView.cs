using GraphProcessor;
using SkillsVRNodes.Editor.Graph;
using SkillsVRNodes.Managers;
using SkillsVRNodes.Scripts.Nodes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews
{
	[NodeCustomEditor(typeof(SubGraphNode))]
	public class SubGraphNodeView : BaseNodeView
	{
		private DropdownField graphDropdown;
		private TextField text;
		private DropdownField characterDropdown;

		public override void Enable()
		{
			// Gets the node
			if (nodeTarget is not SubGraphNode dialogueNode)
			{
				return;
			}

			// Dropdown for selecting line
			graphDropdown = new DropdownField();

			graphDropdown.choices.Add("null");
			graphDropdown.index = 1;
			if (ScriptableObjectManager.GetAllInstances<SubGraph>().Count != 0)
			{
				foreach (SubGraph dialogue in ScriptableObjectManager.GetAllInstances<SubGraph>())
				{
					graphDropdown.choices.Add(dialogue.name);
				}

				graphDropdown.value = dialogueNode.subQuest ? dialogueNode.subQuest.name : "null";
			}
			graphDropdown.RegisterCallback<ChangeEvent<string>>(evt =>
			{
				dialogueNode.subQuest = ScriptableObjectManager.GetAllInstances<SubGraph>().Find(t => t.name.Equals(evt.newValue));
			});

			controlsContainer.Add(graphDropdown);
			
			

			Button button = new Button
			{
				style =
				{
					flexDirection = new StyleEnum<FlexDirection>() { value = FlexDirection.Row },
					backgroundColor = new StyleColor()
					{
						value = new Color(0.1f, 0.1f, 0.1f)
					},
					paddingLeft = 0
				}
			};
			button.clicked += () => SkillsVRGraphWindow.OpenGraph(dialogueNode.subQuest);
			
			// Icon
			Image image = new()
			{
				image = Resources.Load<Texture2D>("Icon/NewView"),
				style = { width = 25, height = 25 }
			};
			Label label = new()
			{
				text = "Open Graph",
				style = { marginRight = 50}
			};
			// Icon
			Image image2 = new()
			{
				image = Resources.Load<Texture2D>("Icon/Quest"),
				style = { width = 20, height = 25 }
			};
			
			button.Add(image);
			button.Add(label);
			button.Add(image2);
			
			

			controlsContainer.Add(button);
		}
	}
}