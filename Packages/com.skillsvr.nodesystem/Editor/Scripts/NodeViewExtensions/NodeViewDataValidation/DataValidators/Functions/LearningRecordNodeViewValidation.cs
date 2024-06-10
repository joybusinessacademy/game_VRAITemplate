using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GraphProcessor;
using Scripts.VisualElements;
using SkillsVRNodes.Scripts.Nodes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
	[CustomDataValidation(typeof(LearningRecordNodeView))]
	public class LearningRecordNodeViewValidation : AbstractNodeViewValidation<LearningRecordNodeView>
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
