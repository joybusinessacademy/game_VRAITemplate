using System.Collections;
using GraphProcessor;
using Props;
using Props.PropInterfaces;
using SkillsVR.VisualElements;
using SkillsVRNodes.Scripts.Nodes;
using Unity.EditorCoroutines.Editor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using VisualElements;


namespace SkillsVRNodes.Editor.NodeViews
{
	[NodeCustomEditor(typeof(UnityEventNode))]
	public class UnityEventNodeView : BaseNodeView
	{
		public UnityEventNode AttachedNode => nodeTarget as UnityEventNode;

		private VisualElement unityEventContainer = new();
		public override VisualElement GetNodeVisualElement()
		{
			//VisualElement unityEventContainer = new();

			PropDropdown<IPropUnityEvent> dropdown = new("Event", AttachedNode.unityEventProp,
				newProp =>
				{
					AttachedNode.unityEventProp = newProp;

					if (newProp == null)
					{
						unityEventContainer.Clear();
						RefreshNode();
						return;
					}

					RefreshUnityEventItems();
				},true, typeof(UnityEventProp));

			return dropdown;
		}

		public override VisualElement GetInspectorVisualElement()
		{
			VisualElement visualElement = new();
			//VisualElement unityEventContainer = new();
			
			PropDropdown<IPropUnityEvent> dropdown = new("Event", AttachedNode.unityEventProp,
				newProp =>
				{
					AttachedNode.unityEventProp = newProp;

					if (newProp == null)
					{
						unityEventContainer.Clear();
						RefreshNode();
						return;
					}
					RefreshUnityEventItems();
				},true, typeof(UnityEventProp));

			FloatField duration = new()
			{
				value = AttachedNode.delay,
				label = "Duration",
				tooltip = "Amount of time to wait for next node after calling event"
			};
			duration.RegisterCallback<ChangeEvent<float>>(evt => AttachedNode.delay = evt.newValue);
			
			
			visualElement.Add(dropdown);
			RefreshUnityEventItems();
			visualElement.Add(unityEventContainer);
			visualElement.Add(AttachedNode.CustomToggle(nameof(UnityEventNode.waitForEvent)));
			visualElement.Add(duration);

			return visualElement;
		}

		private void RefreshUnityEventItems()
		{
			unityEventContainer.Clear();

			IPropUnityEvent sceneUnityEvent = PropManager.GetProp(AttachedNode.unityEventProp);
			if (sceneUnityEvent != null)
			{
				PropComponent component = sceneUnityEvent.GetPropComponent();
				unityEventContainer.Add(component.CustomField(nameof(component.propType), "Event Data"));
			}
		}
	}
}