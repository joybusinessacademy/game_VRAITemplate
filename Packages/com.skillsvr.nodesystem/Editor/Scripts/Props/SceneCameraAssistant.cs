using SkillsVRNodes;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace SceneNavigation
{
	public static class SceneCameraAssistant
	{
		// [Shortcut("Move Cam Forward", typeof(SceneView), KeyCode.W)]
		// public static void MoveCameraForward()
		// {
		// 	PanPivot(new Vector2(0.1f, 0));
		// }
		//
		//
		// [Shortcut("Move Cam Back", typeof(SceneView), KeyCode.S)]
		// public static void MoveCameraBack()
		// {
		// 	PanPivot(new Vector2(-0.1f, 0));
		// }
		//
		// [Shortcut("Move Cam Right", typeof(SceneView), KeyCode.D)]
		// public static void MoveCameraRight()
		// {
		// 	PanPivot(new Vector2(0, -0.1f));
		// }
		//
		// [Shortcut("Move Cam Left", typeof(SceneView), KeyCode.A)]
		// public static void MoveCameraLeft()
		// {
		// 	PanPivot(new Vector2(0, 0.1f));
		// }
		//
		// [Shortcut("Rotate Cam Right", typeof(SceneView), KeyCode.E)]
		// public static void RotateRight()
		// {
		// 	RotateCamera(new Vector2(-45, 0));
		//
		// }
		//
		// [Shortcut("Rotate Cam Left", typeof(SceneView), KeyCode.Q)]
		// public static void RotateLeft()
		// {
		// 	RotateCamera(new Vector2(45, 0));
		// }

		private static void RotateCamera(Vector2 rotateAmount)
		{
			float x = SceneView.lastActiveSceneView.camera.transform.rotation.eulerAngles.x;
			float y = SceneView.lastActiveSceneView.camera.transform.rotation.eulerAngles.y;
			y += rotateAmount.x;
			x += rotateAmount.y;
			x = Mathf.Clamp(x, 0, 80);
			SceneView.lastActiveSceneView.LookAtDirect(CurrentPivot, Quaternion.Euler(x, y, 0));
		}
		
		private static void PanPivot(Vector2 moveAmount)
		{
			Vector3 forwardPos = SceneView.lastActiveSceneView.camera.transform.forward;
			forwardPos.y = 0;
			forwardPos = forwardPos.normalized * moveAmount.y;
			
			
			Vector3 rightPos = SceneView.lastActiveSceneView.camera.transform.right;
			rightPos.y = 0;
			rightPos = rightPos.normalized * -moveAmount.x;

			CurrentPivot += rightPos + forwardPos;
		}

		public static bool OverrideCam = false;

		public static void EventIntercept(SceneView view)
		{
			if (!OverrideCam)
			{
				return;
			}
			

			Event currentEvent = Event.current;

			if (currentEvent.type is EventType.MouseDown or EventType.MouseDrag)
			{
				switch (currentEvent.button)
				{
					case 1:
						RotateCamera(currentEvent.delta / 4);
						currentEvent.Use();
						break;
					case 2:
						PanPivot(currentEvent.delta * 0.02f);
						currentEvent.Use();
						break;
				}
			}

			if (currentEvent.type == EventType.ScrollWheel)
			{
				Zoom(currentEvent.delta.y * 2);
				currentEvent.Use();
			}
		}
		
		private static void Zoom(float zoomAmount)
		{
			float number = SceneView.lastActiveSceneView.cameraDistance / 2;
			number += zoomAmount / 10;

			number = Mathf.Clamp(number, 1, 4f);

			var rotation = SceneView.lastActiveSceneView.camera.transform.rotation.eulerAngles;
			//rotation.x = number * 20;
			SceneView.lastActiveSceneView.LookAtDirect(CurrentPivot, Quaternion.Euler(rotation), number);
		}


		public static Vector3 CurrentPivot
		{
			get {
				SceneView sceneView = SceneView.lastActiveSceneView;
				if (sceneView != null)
					return SceneView.lastActiveSceneView.pivot;
				else
					return Vector3.zero;
			}
			set {
				SceneView sceneView = SceneView.lastActiveSceneView;
				if(sceneView != null)
					SceneView.lastActiveSceneView.pivot = value;
			} 
		}

		[InitializeOnLoadMethod]
		public static void ResetPivot()
		{
			CurrentPivot = Vector3.zero;
		}

		public static Vector3 DefaultCameraPivot = Vector3.up;
		
		public static void SetCameraRotation(Quaternion rotation)
		{
			Vector3 transform = SceneView.lastActiveSceneView.camera.transform.position + rotation * Vector3.forward * SceneView.lastActiveSceneView.cameraDistance;
			SceneView.lastActiveSceneView.LookAtDirect(transform, rotation);
		}

		public static void SetCameraTransform(Vector3 position, Quaternion rotation)
		{
			Vector3 transform = position + rotation * Vector3.forward * SceneView.lastActiveSceneView.cameraDistance;
			SceneView.lastActiveSceneView.LookAt(transform, rotation);
		}

		public static void LookAt(Transform transform)
		{
			Quaternion newRotation = Quaternion.LookRotation(transform.position - SceneView.lastActiveSceneView.camera.transform.position);
			SetCameraRotation(newRotation);
		}

		public static void SetCameraToPlayer()
		{
			SceneView.lastActiveSceneView.isRotationLocked = false;

			SceneView.lastActiveSceneView.orthographic = false;
			PlayerSpawnPosition player = Object.FindObjectOfType<PlayerSpawnPosition>();

			if (player == null)
			{
				return;
			}

			SetCameraTransform(player.transform.position + Vector3.up * 1.6f, player.transform.rotation);
		}
		
		public static void SetCameraToTopdown()
		{
			SceneView.lastActiveSceneView.orthographic = true;
			SceneView.lastActiveSceneView.LookAt(Vector3.zero, Quaternion.Euler(90, 0, 0));
			SceneView.lastActiveSceneView.isRotationLocked = true;
		}
		
		public static void SetCameraTo3rdPerson()
		{
			SceneView.lastActiveSceneView.isRotationLocked = false;
			SceneView.lastActiveSceneView.orthographic = false;
			PlayerSpawnPosition player = Object.FindObjectOfType<PlayerSpawnPosition>();

			if (player == null)
			{
				return;
			}

			SetCameraTransform(player.transform.position + new Vector3(-1.5f, 2, -1.5f), Quaternion.Euler(Vector3.up * 45));
		}
	}
}