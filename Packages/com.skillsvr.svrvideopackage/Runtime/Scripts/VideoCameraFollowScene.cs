using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class VideoCameraFollowScene : MonoBehaviour
{
	public Camera videoCamera;

	private void Awake()
	{
		if(videoCamera != null)
		{
			videoCamera.enabled = false;
			videoCamera.gameObject.SetActive(false);
		}
	}

#if UNITY_EDITOR
	VideoCameraFollowScene()
	{
		SceneView.duringSceneGui += DuringSceneGUI;
	}

	private void OnDestroy()
	{
		SceneView.duringSceneGui -= DuringSceneGUI;
	}

	private void OnValidate()
	{
		videoCamera = GetOrAddVideoCamera();
	}

	private Quaternion nextVideoCameraRotation;
	private bool shouldUpdateVideoCameraRotation;

	private void TryUpdateVideoCameraRotationFromSceneCamera()
	{
		if (videoCamera == null)
		{
			videoCamera = GetOrAddVideoCamera();
		}
		try
		{
			videoCamera.transform.rotation = nextVideoCameraRotation;
			shouldUpdateVideoCameraRotation = false;
		}
		catch (System.Exception e)
		{
			Debug.LogError(e);
		}
	}

	private void DuringSceneGUI(SceneView sceneView)
	{
		if (SceneView.lastActiveSceneView == null)
		{
			return;
		}
		var sceneCamera = SceneView.lastActiveSceneView.camera;
		if (null != sceneCamera && videoCamera != null)
		{
			// Cache the camera rotation for Late Update.
			// Directly set value to video camera may cause unity editor crash by internal rotation injection.
			nextVideoCameraRotation = sceneCamera.transform.rotation;
			if(nextVideoCameraRotation != videoCamera.transform.rotation)
				shouldUpdateVideoCameraRotation = true;
		}

		if(shouldUpdateVideoCameraRotation)
			TryUpdateVideoCameraRotationFromSceneCamera();
	}

	private Camera GetOrAddVideoCamera()
	{
		if (this == null)
		{
			SceneView.duringSceneGui -= DuringSceneGUI;
			return null;
		}

		Camera camera = this.transform.GetComponentInChildren<Camera>();
		if (camera == null)
		{
			GameObject go = new GameObject();
			go.transform.parent = this.transform;
			go.name = "VideoSceneCamera";
			camera = go.AddComponent<Camera>();
		}

		return camera;
	}
#endif
}