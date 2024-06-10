using GraphProcessor;
using SkillsVRNodes.Scripts.Nodes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
	[CustomDataValidation(typeof(GoToStartNodeView))]
	public class GoToStartNodeViewValidation : AbstractNodeViewValidation<GoToStartNodeView>
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
