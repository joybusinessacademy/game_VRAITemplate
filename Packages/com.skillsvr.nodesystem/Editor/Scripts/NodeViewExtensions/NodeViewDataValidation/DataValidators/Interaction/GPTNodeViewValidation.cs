using GraphProcessor;
using Scripts.VisualElements;
using SkillsVR.VisualElements;
using SkillsVRNodes.Scripts.Nodes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VisualElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
	[CustomDataValidation(typeof(GPTNodeView))]
	public class GPTNodeViewValidation : AbstractNodeViewValidation<GPTNodeView>
	{
		public override void OnValidate()
		{
		
		}
		public override VisualElement OnGetVisualSourceFromPath(string path)
		{
			return null;
		}
	}
}
