using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphProcessor;
using Props;
using Scripts.VisualElements;
using SkillsVR.VisualElements;
using SkillsVRNodes.Scripts.Nodes;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
	[CustomDataValidation(typeof(UnityEventNodeView))]
	public class UnityEventNodeViewValidation : AbstractNodeViewValidation<UnityEventNodeView>
	{
		public override void OnValidate()
		{
			var node = TargetNodeView.AttachedNode<UnityEventNode>();

			string eventKeyName = "Event";
			string durationName = "Duration";
			string eventListName = "EventCallbackList";

			bool invalidEventKey = node.unityEventProp.propGUID.IsNullOrWhitespace();
			ErrorIf(invalidEventKey, eventKeyName, "Event cannot be none. Select or create an event.");
			
			if (!invalidEventKey)
			{
				var obj = PropManager.GetProp(node.unityEventProp);
				ErrorIf(IsNull(obj), eventKeyName, "Event scene object not exists. Select or create an new event.\r\nEvent Name: " + node.unityEventProp);

				if (null != obj)
				{
					WarningIf(obj.GetUnityEvent().GetPersistentEventCount() <=0, eventListName, "Add event to start.");
				}
			}
			
			ErrorIf(node.delay < 0.0f, durationName, "Duration must no less than 0. Current is " + node.delay);
			WarningIf(node.delay > 20.0f, durationName, "Duration may too long. Current is " + node.delay);
			
			
		}

		public override VisualElement OnGetVisualSourceFromPath(string path)
		{
			switch (path)
			{
				case "Event": return TargetNodeView.Q<DropdownField>("float-variable-dropdown");
				case "Duration": return TargetNodeView.Q<FloatField>();
				case "EventCallbackList": return TargetNodeView.Q<IMGUIContainer>();
				default: break;
			}

			return null;
		}

	}
}
