using System.Collections;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Video;

namespace SkillsVR.VideoPackage
{
	public class VideoPlayerEditorViewController : IVideoPlayerEditorViewController
	{
		public VideoPlayer videoPlayer { get; set; }
		public bool forceUpdateView { get; set; }

		private bool restorePlayAfterRefocus;

		private EditorCoroutine repaintSceneViewCoroutine;

		private bool wasPlaying;

		public void StartUpdateInEditor()
		{
			StopUpdateInEditor();
			repaintSceneViewCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(RunEditorUpdateLoop());
		}
		public void StopUpdateInEditor()
		{
			if (null != repaintSceneViewCoroutine)
			{
				EditorCoroutineUtility.StopCoroutine(repaintSceneViewCoroutine);
				repaintSceneViewCoroutine = null;
			}
		}

		protected IEnumerator RunEditorUpdateLoop()
		{
			while (true)
			{
				yield return null;
				OnUpdateInEditor();
			}
		}

		protected void OnUpdateInEditor()
		{
			if (null == videoPlayer)
			{
				return;
			}
			if (videoPlayer.isPlaying && !InternalEditorUtility.isApplicationActive)
			{
				restorePlayAfterRefocus = true;
				videoPlayer.Pause();
			}
			if (restorePlayAfterRefocus && InternalEditorUtility.isApplicationActive)
			{
				restorePlayAfterRefocus = false;
				videoPlayer.Play();
			}

			if (forceUpdateView || videoPlayer.isPlaying)
			{
				RepaintVideoView();
			}

			if (wasPlaying && !videoPlayer.isPlaying)
			{
				forceUpdateView = false;
			}
			wasPlaying = videoPlayer.isPlaying;
		}

		protected void RepaintVideoView()
		{
			if (null != videoPlayer)
			{
				EditorUtility.SetDirty(videoPlayer);
			}
			SceneView.RepaintAll();
		}
	}
}

