using GraphProcessor;
using Props.PropInterfaces;
using SkillsVRNodes.Scripts.Nodes;
using UnityEngine.UIElements;
using VisualElements;

namespace SkillsVRNodes.Editor.NodeViews
{
	[NodeCustomEditor(typeof(PropEnableNode))]
	public class PropEnableNodeView : BaseNodeView
	{
		private PropEnableNode AttachedNode => AttachedNode<PropEnableNode>();

		
		public override VisualElement GetInspectorVisualElement()
		{
			var visualElement = new VisualElement();
			PropDropdown<IBaseProp> propDropdown = new("Prop", AttachedNode.propName, evt =>
			{
				AttachedNode.propName = evt;
			});
			visualElement.Add(propDropdown);
			
			visualElement.Add(AttachedNode.CustomToggle(nameof(AttachedNode.setStateToEnabled), "Enable"));

			return visualElement;
		}
	}
}