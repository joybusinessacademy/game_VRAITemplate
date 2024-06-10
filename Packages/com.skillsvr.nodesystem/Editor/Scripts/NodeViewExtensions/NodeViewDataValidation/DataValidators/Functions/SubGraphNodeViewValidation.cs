using GraphProcessor;
using SkillsVRNodes.Editor.Graph;
using SkillsVRNodes.Managers;
using SkillsVRNodes.Scripts.Nodes;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
	[CustomDataValidation(typeof(SubGraphNodeView))]
	public class SubGraphNodeViewValidation : AbstractNodeViewValidation<SubGraphNodeView>
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
