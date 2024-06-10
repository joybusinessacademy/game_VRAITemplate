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
	[CustomDataValidation(typeof(TeleportNodeView))]
	public class TeleportNodeViewValidation : AbstractNodeViewValidation<TeleportNodeView>
	{
		public override void OnValidate()
		{
			CheckTeleportPosition();
		}
		public override VisualElement OnGetVisualSourceFromPath(string path)
		{
			switch(path)
			{
				case "SpawnPosition": return TargetNodeView.Q("scene-element-dropdown").Q<DropdownField>();
				default: return null;
			}
		}
	}
}
