using SkillsVRNodes.Scripts.Nodes;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
	[CustomDataValidation(typeof(PropEnableNodeView))]
	public class PropEnableNodeViewValidation : AbstractNodeViewValidation<PropEnableNodeView>
	{
		public override void OnValidate()
		{
			var node = TargetNodeView.AttachedNode<PropEnableNode>();
			string propGuidPath = "Prop";
			CheckPropGuid(node.propName, propGuidPath, propGuidPath);
		}

		public override VisualElement OnGetVisualSourceFromPath(string path)
		{
			switch(path)
			{
				case "Prop": return TargetNodeView.Q("prop-dropdown");
				default: return null;
			}
		}

		
	}
}
