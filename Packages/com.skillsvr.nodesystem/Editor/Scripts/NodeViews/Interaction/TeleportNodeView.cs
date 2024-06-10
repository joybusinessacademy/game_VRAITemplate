using GraphProcessor;
using Scripts.VisualElements;
using SkillsVR.VisualElements;
using SkillsVRNodes.Scripts.Nodes;
using System;
using System.Collections;
using System.Collections.Generic;
using Props;
using Props.PropInterfaces;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using VisualElements;

namespace SkillsVRNodes.Editor.NodeViews
{
    [NodeCustomEditor(typeof(TeleportNode))]
    public class TeleportNodeView : ScriptableSpawnerNodeView<SpawnerTeleport,
		ITeleportSystem, TeleportData>
	{
        public TeleportNode AttachedNode => nodeTarget as TeleportNode;
		
		public bool fade = true;
		public float fadeDuration = 0.5f;
		public Color fadeColor = Color.black;

		public override VisualElement GetInspectorVisualElement()
		{
			var visualElement = new VisualElement();

			TeleportNode spawnerNode = AttachedNode;

			PropDropdown<IPropPlayerPosition> sceneElementDropdown = new("Position", spawnerNode.spawnPosition.propGUID, elementName => spawnerNode.spawnPosition.propGUID = elementName?.propGUID,true, typeof(TeleportProp));
			visualElement.Add(sceneElementDropdown);

			EnumField teleportTypeField = new("Teleport Type", (TeleportNode.TeleportTypes)AttachedNode.MechanicData.teleportType)
			{
				value = (TeleportNode.TeleportTypes)AttachedNode.MechanicData.teleportType
			};

			teleportTypeField.RegisterValueChangedCallback(OnTeleportTypeChanged);

			visualElement.Add(teleportTypeField);
			
			return visualElement;
		}

		private void OnTeleportTypeChanged(ChangeEvent<Enum> evt)
		{
			AttachedNode.MechanicData.teleportType = (int)(TeleportNode.TeleportTypes)evt.newValue;

			switch ((TeleportNode.TeleportTypes)evt.newValue)
			{
				case TeleportNode.TeleportTypes.TeleportImmediately:
					AttachedNode.MechanicData.teleportType = 0;
					break;
				case TeleportNode.TeleportTypes.EnableTeleporterAndWait:
					AttachedNode.MechanicData.teleportType = 1;
					break;
				case TeleportNode.TeleportTypes.EnableTeleporterAndContinue:
					AttachedNode.MechanicData.teleportType = 2;
					break;
				default:
					break;
			}
		}
    }
}
