using System.Collections.Generic;
using GraphProcessor;
using SkillsVRNodes.Managers;
using SkillsVRNodes.Scripts.Nodes;
using UnityEditor;
using UnityEngine.UIElements;
using DialogExporter;
using Scripts.VisualElements;
using UnityEngine;
using SkillsVRNodes.Scripts;
using SkillsVR.VisualElements;
using VisualElements;
using System.Linq;
using SkillsVRNodes.Audio;
using Props.PropInterfaces;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
	[CustomDataValidation(typeof(DialogueNodeView))]
	public class DialogueNodeViewValidation : AbstractNodeViewValidation<DialogueNodeView>
	{
		

		public override void OnValidate()
		{
			var node = TargetNodeView.AttachedNode<DialogueNode>();

			string dialogKeyName = "Dialogue";
			string characterKeyName = "Character";

			ErrorIf(IsNull(node.dialogueAsset), dialogKeyName, "Dialogue cannot be null. Create or select a dialogue.");
			ErrorIf(IsMissingAsset(node.dialogueAsset), dialogKeyName, "Dialogue asset is already removed. Create or select a new dialogue.");

			// Dialogue could be played without character
			if (!IsInvalidName(node.dialoguePosition.GetPropName()))
			{
				CheckPropGuid<IPropAudioSource>(node.dialoguePosition, dialogKeyName, "Character Missing. Assign a character for the dialogue to use");
				//Check1V1NamedSceneObjectBinding<SceneNPC>(
				//	node.dialoguePosition,
				//	x => x.elementName == node.dialoguePosition, 
				//	characterKeyName, "");
			}
			
		}

		public override VisualElement OnGetVisualSourceFromPath(string path)
		{
			switch(path)
			{
				case "Dialogue": return TargetNodeView.Q("scriptable-object-dropdown");
				case "Character": return TargetNodeView.Q("scene-element-dropdown").Q<DropdownField>();
				default:return null;
			}
		}
	}
}
