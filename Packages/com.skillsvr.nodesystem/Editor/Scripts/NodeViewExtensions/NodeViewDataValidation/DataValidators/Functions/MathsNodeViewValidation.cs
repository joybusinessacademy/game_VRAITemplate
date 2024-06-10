using GraphProcessor;
using SkillsVRNodes.Scripts.Nodes;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using VisualElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
	[CustomDataValidation(typeof(MathsNodeView))]
	public class MathsNodeViewValidation : AbstractNodeViewValidation<MathsNodeView>
	{
		public override VisualElement OnGetVisualSourceFromPath(string path)
		{
            return null;
        }

		public override void OnValidate()
		{
			
		}
	}
}
