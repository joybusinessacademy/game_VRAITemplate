using System;
using System.Collections.Generic;
using GraphProcessor;
using SkillsVRNodes.Scripts.Nodes;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
	[CustomDataValidation(typeof(CounterNode))]
	public class CounterNodeValidation : AbstractNodeViewValidation<BaseNodeView>
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
