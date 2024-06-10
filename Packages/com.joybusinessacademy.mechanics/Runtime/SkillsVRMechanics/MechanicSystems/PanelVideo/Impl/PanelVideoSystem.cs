using SkillsVR.Mechanic.Core;
using SkillsVR.Mechanic.Core.Impl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using SkillsVR.VideoPackage;

#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif

namespace SkillsVR.Mechanic.MechanicSystems.PanelVideo
{
	public class PanelVideoSystem : AbstractMechanicSystemBehivour<PanelVideoData>, IPanelVideoSystem
	{
		[Header("References")]
		public GameObject panelVideoVisual;
		public IAdvancedVideoPlayer currentVideoPlayer { get; private set; }
		public VideoPlayer editorVideoPlayer;
		public RawImage videoImage;
		public Button skipButton;

		private RenderTexture renderTexture;

		private bool startedVideo = false;

		//External Items
		private GameObject externalSpawnedObject;

		protected override void OnEnable()
		{
			base.OnEnable();

			currentVideoPlayer = GetComponentInChildren<IAdvancedVideoPlayer>();
		}

		public void PlayFrom(float time, Action onPlayStart = null)
		{
			if (time == 0)
			{
				currentVideoPlayer?.Play(onPlayStart);
			}
			else
			{
				currentVideoPlayer?.Seek(time, () =>
				{
					currentVideoPlayer?.Play(onPlayStart);
				});
			}
		}

		protected override void OnReceiveEvent(IMechanicSystemEvent systemEvent)
		{
			switch (systemEvent.eventKey)
			{
				case MechSysEvent.OnStart:
					if (mechanicData.usingExternalPlayer)
						PlayExternal();
					else if (editorVideoPlayer != null && editorVideoPlayer.url != string.Empty)
					{
						if(Application.isPlaying)
						{
							if (currentVideoPlayer != null && mechanicData.startTimeVideo != 0)
								PlayFrom(mechanicData.startTimeVideo);
							else
								currentVideoPlayer.Play();
						}
					}

					startedVideo = true;
					break;
				case MechSysEvent.OnStop:
					break;
				case MechSysEvent.BeforeStart:
					GenerateAndSetRenderTexture();
					break;
				default:
					break;
			}
		}

		private void SetVideoClip()
		{
			if (mechanicData.videoClipLocation == string.Empty)
				return;

			editorVideoPlayer.source = VideoSource.Url;
			editorVideoPlayer.url = mechanicData.videoClipLocation;
		}

		private void GenerateAndSetRenderTexture()
		{
			// Get the video's dimensions
			int videoWidth = (int)mechanicData.videoWidth;
			int videoHeight = (int)mechanicData.videoHeight;

			// Create a Render Texture based on video dimensions
			renderTexture = new RenderTexture(videoWidth, videoHeight, 0);

			// Assign the Render Texture to the RawImage
			videoImage.texture = renderTexture;

			if(currentVideoPlayer != null)
				currentVideoPlayer.CurrentRenderTexture = renderTexture;

			// Set the Video Player's target texture to the Render Texture
			editorVideoPlayer.targetTexture = renderTexture;

			//Draw Black
			Texture2D whiteTexture = new Texture2D(videoWidth, videoHeight);
			Color[] pixels = whiteTexture.GetPixels();
			for (int i = 0; i < pixels.Length; i++)
			{
				pixels[i] = Color.white;
			}
			whiteTexture.SetPixels(pixels);
			whiteTexture.Apply();

			Graphics.Blit(whiteTexture, renderTexture);
		}

		private void SetGenericVideoData()
		{
			editorVideoPlayer.isLooping = mechanicData.loopVideo;
			editorVideoPlayer.SetDirectAudioVolume(0, mechanicData.volumeVideo);
		}

		public override void SetMechanicData()
		{
			base.SetMechanicData();

			SetAVProData();

			SetSkipButton();

			if (Application.isPlaying && (mechanicData.usingExternalPlayer || editorVideoPlayer == null))
			{
				panelVideoVisual.SetActive(false);
				return;
			}
			else if(externalSpawnedObject != null)
			{
				externalSpawnedObject.SetActive(false);
			}

			SetVideoClip();

			if (editorVideoPlayer.url == String.Empty)
			{
				panelVideoVisual.SetActive(false);
				return;
			}

			SetGenericVideoData();

			if(Application.isPlaying)
				StartCoroutine(WaitForGeneration());
			else
			{
#if UNITY_EDITOR
				EditorCoroutineUtility.StartCoroutineOwnerless(WaitForGeneration());
#endif
			}

		}

		private IEnumerator WaitForGeneration()
		{
			yield return new WaitForEndOfFrame();
			GenerateAndSetRenderTexture();
		}

		private void SetAVProData()
		{
#if AV_PRO
			mechanicData.usingExternalPlayer = true;

			GameObject loadedPanelPrefab = Resources.Load<GameObject>("PanelVideo");

			if (loadedPanelPrefab == null)
				return;

			//SPAWNING OBJECT
			externalSpawnedObject = Instantiate(loadedPanelPrefab);

			externalSpawnedObject.transform.parent = this.transform;
			externalSpawnedObject.transform.position = this.transform.parent.position;
			externalSpawnedObject.transform.rotation = Quaternion.identity;

			SetExternalData();
#else
			mechanicData.usingExternalPlayer = false;
#endif
		}

		private void SetExternalData()
		{
			string methodName = "SetDataForMediaPlayer";
			//Loop , Video Location
			object[] methodParameters = new object[] { mechanicData.loopVideo, mechanicData.videoClipLocation, mechanicData.startTimeVideo };

			MethodInfo methodInfo = externalSpawnedObject.GetComponents<Component>()
			.Select(component => component.GetType().GetMethod(methodName))
			.FirstOrDefault(m => m != null);

			if (methodInfo != null)
			{
				methodInfo.Invoke(externalSpawnedObject.GetComponent(methodInfo.DeclaringType), methodParameters);
			}
		}

		private void PlayExternal()
		{
			string methodName = "PlayVideo";

			var methodInfo = externalSpawnedObject.GetComponents<Component>()
			.Select(component => component.GetType().GetMethod(methodName))
			.FirstOrDefault(m => m != null);

			if (methodInfo != null)
			{
				methodInfo.Invoke(externalSpawnedObject.GetComponent(methodInfo.DeclaringType), null);
			}
		}

		private void StopExternal()
		{
			string methodName = "StopVideo";

			var methodInfo = externalSpawnedObject.GetComponents<Component>()
			.Select(component => component.GetType().GetMethod(methodName))
			.FirstOrDefault(m => m != null);

			if (methodInfo != null)
			{
				methodInfo.Invoke(externalSpawnedObject.GetComponent(methodInfo.DeclaringType), null);
			}
		}

		private void SetSkipButton()
		{
			skipButton.gameObject.SetActive(mechanicData.showSkipButton);
			skipButton.onClick.AddListener(OnSkipButtonPressed);
		}

		private void OnSkipButtonPressed()
		{
			FinishedVideo();
		}

		private void FinishedVideo()
		{
			if (mechanicData.usingExternalPlayer)
			{
				StopExternal();
				externalSpawnedObject.SetActive(false);
			}
			else
				editorVideoPlayer.Stop();

			startedVideo = false;
			panelVideoVisual.SetActive(false);

			StopMechanic();
			TriggerEvent(PanelVideoEvent.VideoFinished);
		}

		protected override void Update()
		{
			base.Update();

			if (!startedVideo)
				return;

			if (mechanicData.usingExternalPlayer)
			{
				float currentExternalTime = GetExternalVideoTime();

				if ((currentExternalTime / 1000) >= mechanicData.endTimeVideo)
					FinishedVideo();
			}
			else
			{
				if (editorVideoPlayer.time >= mechanicData.endTimeVideo)
					FinishedVideo();
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();

			if (renderTexture != null)
			{
				renderTexture.Release();
				renderTexture = null;
			}

			if (skipButton != null)
				skipButton.onClick.RemoveListener(OnSkipButtonPressed);
		}

		private float GetExternalVideoTime()
		{
			float externalVideoTime = -1;
			string targetPropertyName = "currentVideoTime";

			var componentWithProperty = externalSpawnedObject.GetComponents<Component>()
			.FirstOrDefault(component => component != null &&
			component.GetType().GetProperty(targetPropertyName) != null);

			if (componentWithProperty != null)
			{
				Type componentType = componentWithProperty.GetType();
				PropertyInfo propertyInfo = componentType.GetProperty(targetPropertyName);
				object propertyValue = propertyInfo.GetValue(componentWithProperty, null);
				externalVideoTime = (float)propertyValue;
			}
			else
			{
				Debug.LogError($"Property '{targetPropertyName}' not found in any component on the GameObject.");
			}

			return externalVideoTime;
		}

	}
}
