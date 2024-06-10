using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Props;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Scripts.Props
{
	public static class PropDragAndDrop
	{
		private static PropComponent currentPropComponent;

		public static PropComponent CurrentPropComponent => currentPropComponent;

		public static void StartMouseDrag(PropComponent propComponent)
		{
			currentPropComponent = propComponent;
			SaveCurrentObjectMaterials();
			SceneView.lastActiveSceneView.rootVisualElement.RegisterCallback<MouseMoveEvent>(MouseMoveCallback);
			SceneView.lastActiveSceneView.rootVisualElement.RegisterCallback<MouseUpEvent>(TryDropObject);
			
			//SceneView.lastActiveSceneView.rootVisualElement.RegisterCallback<KeyDownEvent>(EscapeKey);
		}

		public static Action OnClearDrop = () => { };
		
		private static Vector3 ForwardDirection(Vector3 hitNormal)
		{
			Vector3 direction = Vector3.forward;

			if(CurrentPropComponent.dropPosition == PropComponent.DropPosition.Wall ||
				CurrentPropComponent.dropPosition == PropComponent.DropPosition.Any)
			{
				direction = -hitNormal;
			}

			return direction;
		}
		
		private static void TryDropObject(MouseUpEvent evt)
		{
			if (evt.pressedButtons == 1)
			{
				return;
			}
			
			Vector2 mousePosition = evt.mousePosition * GetScreenScale();
			mousePosition.y = SceneView.lastActiveSceneView.camera.pixelHeight - mousePosition.y + 40;
				
			Ray ray = SceneView.lastActiveSceneView.camera.ScreenPointToRay(mousePosition);
			LayerMask mask = LayerMask.GetMask("Editor");
			bool didHit = Physics.Raycast(ray, out RaycastHit hit, 1000, mask);
				
			if (didHit && ValidateDrop(hit))
			{
				DropObject(hit.point, ForwardDirection(hit.normal));
			}

			ClearPreviewObject();
			
			Selection.activeObject = null;
		}


		private static void SaveCurrentObjectMaterials()
		{
			if (currentPreviewObject == null)
			{
				return;
			}
			
			currentMaterials = new List<Material[]>();
			foreach (Renderer renderer in CurrentPropComponent.GetComponentsInChildren<Renderer>())
			{
				currentMaterials.Add(renderer.sharedMaterials);
			}
		}
		private static List<Material[]> currentMaterials = new();
		private static void UpdatePreviewMaterials(bool isError)
		{
			if (currentPreviewObject == null)
			{
				return;
			}

			if (isError)
			{
				Material errorMat = Resources.Load<Material>("Materials/ErrorRed");

				Renderer[] children = currentPreviewObject.GetComponentsInChildren<Renderer>();
				foreach (Renderer renderer in children)
				{
					for (int index = 0; index < renderer.sharedMaterials.Length; index++)
					{
						renderer.sharedMaterials[index] = errorMat;
					}
					renderer.sharedMaterial = errorMat;
				}
			}
			else
			{
				Renderer[] children = currentPreviewObject.GetComponentsInChildren<Renderer>();
				for (int i = 0; i < children.Length; i++)
				{
					children[i].sharedMaterials = currentMaterials[i];
				}
			}
		}
		
		private static void MouseMoveCallback(MouseMoveEvent evt)
		{
			var mousePosition = evt.mousePosition * GetScreenScale();
			mousePosition.y = SceneView.lastActiveSceneView.camera.pixelHeight - mousePosition.y + 40;

			Ray ray = SceneView.lastActiveSceneView.camera.ScreenPointToRay(mousePosition);
			LayerMask mask = LayerMask.GetMask("Editor");
			bool didHit = Physics.Raycast(ray, out RaycastHit hit, 1000, mask);

			if (!didHit)
			{
				return;
			}


			UpdatePreviewMaterials(!ValidateDrop(hit));

			DragPreview(hit.point, ForwardDirection(hit.normal));
		}
		
		private static bool ValidateDrop(RaycastHit hit)
		{
			switch (CurrentPropComponent.dropPosition)
			{
				case PropComponent.DropPosition.Floor:
					if (hit.normal.y > 0.8)
					{
						return true;
					}

					break;
				case PropComponent.DropPosition.Wall:
					if (hit.normal.y > -0.2 && hit.normal.y < 0.2)
					{
						return true;
					}

					break;
				case PropComponent.DropPosition.Ceiling:
					if (hit.normal.y < -0.8)
					{
						return true;
					}

					break;
				case PropComponent.DropPosition.GroundOnly:
					if (hit.normal.y > 0.8 && hit.point.y < 0.3f)
					{
						return true;
					}

					break;
				case PropComponent.DropPosition.TableOnly:
					if (hit.normal.y > 0.8 && hit.point.y > 0.3f)
					{
						return true;
					}
					break;
				case PropComponent.DropPosition.Any:
					return true;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return false;
		}

		private static void DropObject(Vector3 position, Vector3 forward = default)
		{
            
            PropComponent newPropComponent = PrefabUtility.InstantiatePrefab(CurrentPropComponent, PropManager.Instance.transform) as PropComponent;
			PrefabUtility.UnpackPrefabInstance(newPropComponent.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            if (newPropComponent == null)
			{
				return;
			}
            
			Undo.RegisterCreatedObjectUndo(newPropComponent.gameObject, "Create Prop");
			newPropComponent.transform.position = position;
			if (forward != default)
			{
				newPropComponent.transform.rotation = Quaternion.LookRotation(forward);
			}

			EditorSceneManager.MarkSceneDirty(newPropComponent.gameObject.scene);
			Selection.objects = new Object[] { newPropComponent.gameObject };

			SceneView.lastActiveSceneView.showGrid = false;


            ClearPreviewObject();
		}


		public static PropComponent currentPreviewObject;

		public static void ClearPreviewObject()
		{
			OnClearDrop.Invoke();
			
			if (currentPreviewObject != null)
			{
				Object.DestroyImmediate(currentPreviewObject.gameObject);
			}

			if (SceneView.lastActiveSceneView != null && SceneView.lastActiveSceneView.rootVisualElement != null)
			{
				SceneView.lastActiveSceneView.rootVisualElement.UnregisterCallback<MouseMoveEvent>(MouseMoveCallback);
				SceneView.lastActiveSceneView.rootVisualElement.UnregisterCallback<MouseUpEvent>(TryDropObject);
			}

			currentPropComponent = null;

			//SceneView.lastActiveSceneView.rootVisualElement.UnregisterCallback<KeyDownEvent>(EscapeKey);
		}

		private static void EscapeKey(KeyDownEvent evt)
		{
			if (evt.keyCode == KeyCode.Escape)
			{
				ClearPreviewObject();
			}
		}


		public static void StartDragPreview()
		{
			currentPreviewObject = Object.Instantiate(CurrentPropComponent.gameObject).GetComponent<PropComponent>();
			SaveCurrentObjectMaterials();

			if (currentPreviewObject.GetComponentInChildren<PropGizmo>() != null)
				currentPreviewObject.GetComponentInChildren<PropGizmo>().drawAlways = true;

			//Selection.activeGameObject = currentPreviewObject.gameObject;
		}
		
		private static void DragPreview(Vector3 dragPosition, Vector3 forward)
		{
			if (!currentPreviewObject)
			{
				StartDragPreview();
			}
			
			currentPreviewObject.transform.position = dragPosition;
			if (forward != default)
			{
				currentPreviewObject.transform.rotation = Quaternion.LookRotation(forward);
			}
		}

		public static float GetScreenScale()
		{
			Type utilityType = typeof(GUIUtility);
			PropertyInfo[] allProps = utilityType.GetProperties(BindingFlags.Static | BindingFlags.NonPublic);
			PropertyInfo property = allProps.First(m => m.Name == "pixelsPerPoint");
			float pixelsPerPoint = (float)property.GetValue(null);
			return pixelsPerPoint;
		}
	}
}
