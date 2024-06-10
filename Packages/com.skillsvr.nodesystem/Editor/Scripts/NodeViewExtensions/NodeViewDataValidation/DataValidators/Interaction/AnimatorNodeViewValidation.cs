using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using SkillsVR.VisualElements;
using SkillsVRNodes.Scripts.Nodes;
using UnityEngine;
using UnityEngine.UIElements;
using VisualElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
	[CustomDataValidation(typeof(AnimatorNodeView))]
	public class AnimatorNodeViewValidation : AbstractNodeViewValidation<AnimatorNodeView>
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
