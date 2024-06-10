using Scripts.VisualElements;
using System;
using System.CodeDom;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor.Graphs;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
	[CustomDataValidation(typeof(SurveyNodeView))]
	public class SurveyNodeViewValidation : AbstractNodeViewValidation<SurveyNodeView>
	{
		public override VisualElement OnGetVisualSourceFromPath(string path)
		{
			throw new NotImplementedException();
		}

		public override void OnValidate()
		{
		}

	}
}
