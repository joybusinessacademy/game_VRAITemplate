using System;
using System.Collections;
using System.Reflection;
using GraphProcessor;
using Props;
using Props.PropInterfaces;
using SceneNavigation;
using SkillsVR.Mechanic.Core;
using SkillsVR.UnityExtenstion;
using SkillsVRNodes.Scripts.Nodes;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VisualElements;

namespace SkillsVRNodes.Editor.NodeViews
{
	[NodeCustomEditor(typeof(SpawnerNode<, , >))]
	public class SpawnerNodeView<TSpawner, TInterface, TData> : BaseNodeView
		where TSpawner : AbstractMechanicSpawner<TInterface, TData>
		where TInterface : IMechanicSystem<TData>
		where TData : new()
	{
		public override void Enable()
		{
			if (AttachedNode.spawnPosition.IsNullOrEmpty())
			{
				string propCenter = PropManager.GetGUIDByName<IPropPanel>("Relative To Player/Front Center");

				if (!string.IsNullOrWhiteSpace(propCenter))
				{
					AttachedNode.spawnPosition = propCenter;
				}
			}
			base.Enable();
		}

		protected static VisualElement CreateTransformDropdown<TPropType>(SpawnerNode<TSpawner, TInterface, TData> spawnerNode) where TPropType : class, IPropTransform
		{
			VisualElement transformDropdowns = new();

			PropDropdown<TPropType> sceneElementDropdown = null;
			
			if (spawnerNode.spawnPosition.IsNullOrEmpty())
			{
				string propCenter = PropManager.GetGUIDByName<IPropPanel>("Relative To Player/Front Center");

				if (!string.IsNullOrWhiteSpace(propCenter))
				{
					sceneElementDropdown = new PropDropdown<TPropType>("Position: ", propCenter, elementName => spawnerNode.spawnPosition.propGUID = elementName?.propGUID,true, typeof(PanelProp));
					spawnerNode.spawnPosition = propCenter;
				}
			}

			sceneElementDropdown ??= new("Position: ", (string)spawnerNode.spawnPosition, elementName => spawnerNode.spawnPosition.propGUID = elementName?.propGUID, true, typeof(PanelProp));
			
			//Toggle panoramaLocation = new Toggle("Use 360 Video Transform");
			////panoramaLocation.style.alignSelf = Align.Center;
			//panoramaLocation.RegisterValueChangedCallback(e => {
			//	spawnerNode.panoramaSpawnPosition = e.newValue;
			//});

			//panoramaLocation.value = spawnerNode.panoramaSpawnPosition;


			transformDropdowns.Add(sceneElementDropdown);
			//transformDropdowns.Add(panoramaLocation);

			transformDropdowns.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
			return transformDropdowns;
		}

		public SpawnerNode<TSpawner, TInterface, TData> AttachedNode => nodeTarget as SpawnerNode<TSpawner, TInterface, TData>;


		protected override void OnDoubleClick()
		{
#if UNITY_EDITOR
			if (Application.isPlaying)
				return;
#endif

			if(AttachedNode.mechanicObject == null)
				SpawnPreviewMechanic();

			if (AttachedNode.mechanicObject)
				SceneCameraAssistant.LookAt(AttachedNode.mechanicObject.transform);
		}

		public void SpawnPreviewMechanic()
		{
			AttachedNode.SpawnObject();

			MethodInfo evt = AttachedNode.mechanicSpawner.GetType().GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
			SkillsVRGraphWindowEditorUpdate.RegisterGameObject(AttachedNode.mechanicObject);

			EditorTestComponent.AddComponentToGameObject(AttachedNode.mechanicSpawner.gameObject, this);

			if (evt == null)
				return;

			evt.Invoke(AttachedNode.mechanicSpawner, new object[] { });

			AttachedNode.mechanicSpawner.transform.parent = AttachedNode.mechanicObject.transform;
			AttachedNode.mechanicSpawner.name = AttachedNode.mechanicSpawner.name + "_PREVIEW";
			AttachedNode.mechanicSpawner.tag = "EditorOnly";
			//AttachedNode.mechanicSpawner.StartMechanic();

			EditorCoroutineUtility.StartCoroutineOwnerless(WaitForTargetSystem(()=> AttachedNode.mechanicSpawner.StartMechanic()));

			CanvasGroup canvasGroup = AttachedNode.mechanicSpawner.GetComponentInChildren<CanvasGroup>();
			if (canvasGroup)
				canvasGroup.alpha = 1f;

			SceneVisibilityManager.instance.DisablePicking(AttachedNode.mechanicSpawner.gameObject, true);
		}

		private IEnumerator WaitForTargetSystem(Action startMech)
		{
			while(!AttachedNode.mechanicSpawner.ready)
				yield return null;

			startMech?.Invoke();
		}

		// For Previewing
		public override void OnSelected()
		{
#if UNITY_EDITOR
			if (Application.isPlaying)
				return;
#endif			
			base.OnSelected();

			//Uncomment for - Single Click Implementation
			//SpawnPreviewMechanic();
		}
		
		public override void OnUnselected()
		{
#if UNITY_EDITOR
			if (Application.isPlaying)
				return;
#endif		
			base.OnUnselected();

			if (AttachedNode.mechanicObject)
				SkillsVRGraphWindowEditorUpdate.RemoveUpdatePreview(AttachedNode.mechanicObject, true);

		}
	}
}
