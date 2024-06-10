using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using RenderHeads.Media.AVProVideo;

namespace SkillsVR.VideoPackage
{
	public enum PlayMode
	{
		Normal,
		Loop,
		ABSection,
		ABSectionLoop,
		ABSectionThenCustomLoopSection,
	}

	[ExecuteInEditMode]
	[RequireComponent (typeof(VideoCameraFollowScene))]
	public class PanoramaVideoSystem : MonoBehaviour, IAdvancedVideoControl
	{
		public IAdvancedVideoPlayer currentVideoPlayer { get; private set; }

		public VideoPlayerItem videoPlayerItemSO;


		#region IAdvancedVideoControl Properties

		public bool CanPlay => null != currentVideoPlayer && currentVideoPlayer.CanPlay;
		public bool IsPlaying => currentVideoPlayer?.IsPlaying ?? false;

		/// <summary>
		/// The currently playing video asset location
		/// </summary>
		public string VideoAssetLocation
		{
			get => null == currentVideoPlayer ? string.Empty : currentVideoPlayer.VideoAssetLocation;
			set
			{
				if (null == currentVideoPlayer)
				{
					return;
				}
				currentVideoPlayer.VideoAssetLocation = value;
				RemoveAllTimeEvents();
			}
		}

		/// <summary>
		/// The new video asset location to switch to after the transition is done
		/// </summary>
		public string newVideoAssetLocation = "";
		public float CurrentPlayTime
		{
			get => currentVideoPlayer?.CurrentPlayTime ?? 0.0f;
			set => currentVideoPlayer?.Seek(value);
		}

		public float CurrentVideoDuration => currentVideoPlayer?.CurrentVideoDuration ?? 0.0f;

		#endregion IAdvancedVideoControl Properties
		
		public VideoTransitionType CurrentTransitionType { get;  set; }
		
		public bool WasPlaying { get; protected set; }

		public PlayMode playMode;
		public PlayMode previousPlayMode;

		public float ABSectionStartTime;
		public float ABSectionEndTime;

		public float customLoopSectionStartTime;
		public float customLoopSectionEndTime;

		/// <summary>
		/// Extandable custom IAdvancedVideoPlayer type.
		/// </summary>
		public Type customAdvancedVideoPlayerType;


		private int playSectionIndex = 0;

		private GameObject controllerObj;

		public float VirtualNorth
		{
			get => null == currentVideoPlayer || null == currentVideoPlayer.SkyBoxMaterial? 0.0f : currentVideoPlayer.SkyBoxMaterial.GetFloat(Rotation);
			set => currentVideoPlayer?.SkyBoxMaterial?.SetFloat(Rotation, value + currentVideoPlayer?.VirtualNorthAlignment??0.0f);
		}
		
		public int DataVersion => 1;

		public event Action onVideoTransitionStart;
		public event Action onFirstTimeReachPlayEndPoint;

		public event Action<int> onReachPlayEndPoint;

        protected Material prevSkyBoxMaterial;

        #region Time Events
        private float prevTime;

		[SerializeField]
		private GameObject videoCollisionObject;
		private class EventData
		{
			public float time;
			public Action callback;


			public bool onetimeEvent;

			public EventData(float triggerTime, Action customCallback, bool isOneTime = false)
			{
				time = triggerTime;
				callback = customCallback;
				onetimeEvent = isOneTime;
			}
		}

		private List<EventData> managedTimeEvents = new();

		public event Action<float> onVideoTimeChangedEvent;

		public void AddTimeEvent(float triggerTime, Action timeCallback)
		{
			if (null == timeCallback)
			{
				return;
			}
			managedTimeEvents.Add(new EventData(triggerTime, timeCallback, false));
		}

		public void AddOneOffTimeEvent(float triggerTime, Action timeCallback)
		{
			if (null == timeCallback)
			{
				return;
			}
			managedTimeEvents.Add(new EventData(triggerTime, timeCallback, true));
		}

		public void RemoveAllTimeEvents()
		{
			managedTimeEvents.Clear();
		}

		protected void UpdateTimeEvents()
		{
			float currTime = CurrentPlayTime;
			bool currPlaying = IsPlaying;

			bool startFrame = !WasPlaying && currPlaying;
			bool endFrame = WasPlaying && !currPlaying;
			bool canUpdate = (currTime != prevTime);
			if (!canUpdate)
			{
				prevTime = currTime;
				WasPlaying = currPlaying;
				return;
			}

			try
			{
				onVideoTimeChangedEvent?.Invoke(currTime);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}

			
			List<EventData> items = managedTimeEvents.Where(x => prevTime <= x.time && x.time < currTime).ToList();
			// In case of video loop to begin time, check loop and trigger anyevents between prev time to end time
			if (currTime < prevTime)
			{
				float endTime = CurrentVideoDuration;
				float diffToEnd = Mathf.Abs(endTime - prevTime);
				if (diffToEnd < 0.1f)
				{
					IEnumerable<EventData> endEvents = managedTimeEvents.Where(x => prevTime <= x.time && x.time < endTime + 0.05f);
					items.AddRange(endEvents);
				}
			}
			foreach (EventData item in items)
			{
				item.callback?.Invoke();
				if (item.onetimeEvent)
				{
					managedTimeEvents.Remove(item);
				}
			}

			prevTime = currTime;
			
			WasPlaying = currPlaying;
		}
		#endregion Time Events

		private void Awake()
		{
		}

		private void OnDestroy()
		{

		}

		private void OnEnable()
		{
			Init();
			SavePrevSkyBoxMaterial();
		}

		private void OnDisable()
		{
			DisableAllVideoPlayersInChildren();
			RestorePrevSkyBoxMaterial();
		}

		public void Reset()
		{
			Stop();
			RemoveAllTimeEvents();
			currentVideoPlayer?.Reset();
		}

		public void Update()
		{
			UpdateTimeEvents();
		}

		private void DisableAllVideoPlayersInChildren()
		{
			IAdvancedVideoPlayer[] items = GetComponentsInChildren<IAdvancedVideoPlayer>();
			foreach (IAdvancedVideoPlayer item in items)
			{
				Component videoComponent = item as Component;
				if (Application.isEditor)
				{
					DestroyImmediate(videoComponent.gameObject);
				}
				else
				{
					Destroy(videoComponent.gameObject);
				}
				
			}
		}

		private Type GetPlayerTypeFromEnvironment()
		{
			if (null != customAdvancedVideoPlayerType
				&& customAdvancedVideoPlayerType.IsClass 
				&& !customAdvancedVideoPlayerType.IsAbstract
				&& customAdvancedVideoPlayerType.GetInterfaces().Contains(typeof(IAdvancedVideoPlayer)))
			{
				return customAdvancedVideoPlayerType;
			}
#if AV_PRO
			if (Application.isPlaying)
			{
				return typeof(AdvancedAVProMediaPlayerController);
			}
#endif
			return typeof(AdvancedVideoPlayerController);
		}

		public void Init()
		{
			if (null == videoPlayerItemSO)
			{
				videoPlayerItemSO = GetScriptableData();
			}
			DisableAllVideoPlayersInChildren();
			RemoveAllTimeEvents();

			Type playerType = GetPlayerTypeFromEnvironment();
			currentVideoPlayer = GetComponentInChildren(playerType, true) as IAdvancedVideoPlayer;

			if (null != currentVideoPlayer)
			{
				controllerObj = (currentVideoPlayer as Component)?.gameObject;
				controllerObj.SetActive(true);
			}
			else
			{
				controllerObj = new GameObject();
				controllerObj.transform.SetParent(this.transform);
				currentVideoPlayer = controllerObj.AddComponent(playerType) as IAdvancedVideoPlayer;
			}
			currentVideoPlayer.SetupFromVideoData(videoPlayerItemSO);

			AddSphereCollision();
			InitTransition();
        }

		private void AddSphereCollision()
		{
			GameObject sphereForColliding = Resources.Load<GameObject>("SphereRevertedNormals");
			if (null != controllerObj && videoCollisionObject == null && sphereForColliding != null)
			{
				videoCollisionObject = Instantiate(sphereForColliding, Vector3.zero, Quaternion.identity);
				videoCollisionObject.transform.SetParent(controllerObj.transform.parent);
			}
		}

		private VideoPlayerItem GetScriptableData()
		{
			var item = Resources.LoadAll<VideoPlayerItem>("").FirstOrDefault();
			return item;
		}

		protected void RefreshPlayer()
		{
			if (null == videoPlayerItemSO)
			{
				videoPlayerItemSO = GetScriptableData();
			}
			currentVideoPlayer.RefreshFromVideoData(videoPlayerItemSO);
		}
		
		public void SavePrevSkyBoxMaterial()
		{
            prevSkyBoxMaterial = RenderSettings.skybox;
		}

		public void RestorePrevSkyBoxMaterial()
		{
			try
			{
                RenderSettings.skybox = prevSkyBoxMaterial;
            }
			catch { }
		}

		public void ApplySkyBoxMaterial()
		{
			RefreshPlayer();
            Material newMaterial = currentVideoPlayer?.SkyBoxMaterial ?? null;
            RenderSettings.skybox = newMaterial;
        }

		public void PlayFrom(float time, Action onPlayStart = null)
		{
            ApplySkyBoxMaterial();
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

		public void PreviewAt(float time, Action onSeekfinish = null)
		{
			ApplySkyBoxMaterial();
            currentVideoPlayer?.Seek(time, onSeekfinish);
		}

		public void Play(Action onPlayStart = null)
		{
            ApplySkyBoxMaterial();
            currentVideoPlayer?.Play(onPlayStart);
		}

		public void Pause()
		{
			currentVideoPlayer?.Pause();
		}

		public void Stop()
		{
			currentVideoPlayer?.Stop();
		}

		public void Seek(float time, Action onSeekFinish = null)
		{
            ApplySkyBoxMaterial();
            currentVideoPlayer?.Seek(time, onSeekFinish);
		}



		private int reachEndPointCount;
		public bool PlayWithConfig(Action onPlayStart = null)
		{
			if (!CanPlay
				|| !HasPlayableDuration()) 
			{ 
				OnEndPointNormal();
				return false;
			}

			SetPlaySection(ABSectionStartTime, ABSectionEndTime);
			SetCustomLoopSection(customLoopSectionStartTime, customLoopSectionEndTime);
			playSectionIndex = 0;
			reachEndPointCount = 0;

			if (0.0f >= CurrentVideoDuration)
			{
				Seek(ABSectionStartTime, () => { StartPlayConfig(onPlayStart); });
			}
			else
			{
				StartPlayConfig(onPlayStart);
			}
			
			return true;
		}

		private void StartPlayConfig(Action onPlayStart = null)
		{
			float minEndPointTime = ABSectionEndTime >= CurrentVideoDuration ? CurrentVideoDuration : ABSectionEndTime;
			switch (playMode)
			{
				case PlayMode.ABSection:
					AddTimeEvent(minEndPointTime, OnEndPointNormal);
					PlayFrom(ABSectionStartTime, onPlayStart);
					break;
				case PlayMode.ABSectionLoop:
					AddTimeEvent(minEndPointTime, OnEndPointABSectionLoop);
					PlayFrom(ABSectionStartTime, onPlayStart);
					break;
				case PlayMode.ABSectionThenCustomLoopSection:
					AddOneOffTimeEvent(minEndPointTime, OnEndPointCustomSectionLoop);
					float minLoopSectionEndPointTime = customLoopSectionEndTime > CurrentVideoDuration ? CurrentVideoDuration : customLoopSectionEndTime;
					AddTimeEvent(minLoopSectionEndPointTime, OnEndPointCustomSectionLoop);
					PlayFrom(ABSectionStartTime, onPlayStart);
					break;
				case PlayMode.Normal:
					AddTimeEvent(CurrentVideoDuration, OnEndPointNormal);
					PlayFrom(0, onPlayStart);
					break;
				case PlayMode.Loop:
					AddTimeEvent(CurrentVideoDuration, OnEndPointLoop);
					PlayFrom(0, onPlayStart);
					break;
				default:
					PlayFrom(0);
					break;
			}
		}

		private bool HasPlayableDuration()
		{
			if (!Mathf.Approximately(ABSectionEndTime, ABSectionStartTime))
			{
				return true;
			}
			switch (playMode)
			{
				case PlayMode.ABSection:
				case PlayMode.ABSectionLoop:
				case PlayMode.ABSectionThenCustomLoopSection:
					return false;
				case PlayMode.Normal:
				case PlayMode.Loop:
				default: return true;
			}
		}

		private void OnReachEndPointInternal()
		{
			SetFinalFrame();

			if (playMode == PlayMode.Loop || playMode == PlayMode.ABSectionLoop || playMode == PlayMode.ABSectionThenCustomLoopSection)
			{
				EndPointOfVideo();
				return;
			}

			switch (CurrentTransitionType)
			{
				case VideoTransitionType.NoTransition:
					EndPointOfVideo();
					break;
				case VideoTransitionType.Crossfade:
					{
						videoMaterial.SetFloat(AlternateTexWeight, 1);
						EndPointOfVideo();
					}
					break;
				case VideoTransitionType.FadeToBlack:
					StartCoroutine(JustFadeToBlack(true));
					break;
				default:
					break;
			}
		}

		private void EndPointOfVideo()
		{
			++reachEndPointCount;
			if (1 == reachEndPointCount)
			{
				previousPlayMode = playMode;

				onFirstTimeReachPlayEndPoint?.Invoke();
			}
			onReachPlayEndPoint?.Invoke(reachEndPointCount);
		}

		private void OnEndPointNormal()
		{
			Pause();
			OnReachEndPointInternal();
		}
		private void OnEndPointLoop()
		{
			OnReachEndPointInternal();
			PlayFrom(0);
		}
		private void OnEndPointABSectionLoop()
		{
			OnReachEndPointInternal();
			PlayFrom(ABSectionStartTime);
		}

		private void OnEndPointCustomSectionLoop()
		{
			OnReachEndPointInternal();
			switch (playSectionIndex)
			{
				case 0:
					PlayFrom(customLoopSectionStartTime);
					playSectionIndex++;
					break;
				case 1:
					PlayFrom(customLoopSectionStartTime);
					break;
				default: Stop(); break;
			}
			
		}
		public PanoramaVideoSystem SetPlaySection(float timeA, float timeB)
		{
			ABSectionStartTime = Mathf.Min(timeA, timeB);
			ABSectionStartTime = Mathf.Max(ABSectionStartTime, 0.0f);
			ABSectionEndTime = Mathf.Max(timeA, timeB, ABSectionStartTime);
			return this;
		}

		public PanoramaVideoSystem SetCustomLoopSection(float timeA, float timeB)
		{
			customLoopSectionStartTime = Mathf.Min(timeA, timeB);
			customLoopSectionStartTime = Mathf.Max(customLoopSectionStartTime, 0.0f);
			customLoopSectionEndTime = Mathf.Max(timeA, timeB, customLoopSectionStartTime);
			return this;
		}

		public PanoramaVideoSystem SetVideoLocation(string location)
		{
			if (string.IsNullOrWhiteSpace(VideoAssetLocation))
			{
				VideoAssetLocation = location;
			}
			newVideoAssetLocation = location;
			return this;
		}

		/// <summary>
		/// Switches the video to the new video asset location
		/// </summary>
		/// <remarks>This is done so that during a transition the playing video will stay the same</remarks>
		private void SwitchVideo()
		{
			VideoAssetLocation = newVideoAssetLocation;
		}

		public PanoramaVideoSystem SetPlayMode(PlayMode newMode)
		{
			playMode = newMode;
			return this;
		}

		public PanoramaVideoSystem SetImageType(VideoImageType imageType, VideoLayoutType layoutType)
		{
			currentVideoPlayer.ImageType = imageType;
			currentVideoPlayer.LayoutType = layoutType;
			return this;
		}

		public PanoramaVideoSystem SetupAudioVolume(float baseVideoVolume, float volumeBooster = 1.0f)
		{
			AudioSource audioSource = this.GetComponentInChildren<AudioSource>();
			if (audioSource != null)
			{
				audioSource.volume = baseVideoVolume;
				
				audioSource.outputAudioMixerGroup?.audioMixer?.SetFloat("Attenuation", volumeBooster);
			}
			return this;
		}

		public PanoramaVideoSystem SetupVideoPlayerItem(VideoPlayerItem newVideoPlayerData)
		{
			if (null != newVideoPlayerData && newVideoPlayerData == this.videoPlayerItemSO)
			{
				return this;
			}
			videoPlayerItemSO = null == newVideoPlayerData ? this.videoPlayerItemSO : newVideoPlayerData;
            if (null == videoPlayerItemSO)
            {
                videoPlayerItemSO = GetScriptableData();
            }
            InitTransition();
            currentVideoPlayer?.SetupFromVideoData(this.videoPlayerItemSO);
			return this;
		}

		public PanoramaVideoSystem SetVirtualNorth(float newVirtualNorth)
		{
			VirtualNorth = newVirtualNorth;
			return this;
		}

		public PanoramaVideoSystem SetTransitionType(VideoTransitionType transitionType)
		{
			CurrentTransitionType = transitionType;
            return this;
		}
		
		public PanoramaVideoSystem SetTransitionDuration(float lerpDuration)
		{
			this.lerpDuration = lerpDuration;
			return this;
		}

		public PanoramaVideoSystem SetCustomAudioClip(AudioClip customAudioClip)
		{
			AudioSource audioSource = GetComponentInChildren<AudioSource>();
			if (audioSource != null)
			{
				audioSource.clip = customAudioClip;
			}
			return this;
		}

		public PanoramaVideoSystem SetRenderTextureSize(int width, int height)
		{
			if(null != videoPlayerItemSO)
			{
				foreach (var item in videoPlayerItemSO.settings)
				{
					item.videoRenderTexture.Release();
					item.videoRenderTexture.width = width;
					item.videoRenderTexture.height = height;
					item.videoRenderTexture.Create();
				}
				if(videoPlayerItemSO.videoRenderAlternateTexture != null)
				{
					videoPlayerItemSO.videoRenderAlternateTexture.Release();
					videoPlayerItemSO.videoRenderAlternateTexture.width = width;
					videoPlayerItemSO.videoRenderAlternateTexture.height = height;
					videoPlayerItemSO.videoRenderAlternateTexture.Create();
				}
			}

			return this;
		}

        #region Video Transition
        private Coroutine sceneTransitionCoroutine;
        public Material videoMaterial;
        private RenderTexture alternateTexture;
		private static readonly int AlternateTexWeight = Shader.PropertyToID("_AlternateTexWeight");
		private static readonly int Rotation = Shader.PropertyToID("_Rotation");
		private float lerpDuration = 0.5f;

		private void InitTransition()
        {
			if (null != videoPlayerItemSO)
			{
				alternateTexture = videoPlayerItemSO.videoRenderAlternateTexture;
			}
			videoMaterial = currentVideoPlayer?.SkyBoxMaterial;
        }

        public void PreformVideoEnterTransitions()
        {
	        StartTransition();
        }
        
        private void StartTransition()
        {

			if(previousPlayMode == PlayMode.Loop || previousPlayMode == PlayMode.ABSectionLoop || previousPlayMode == PlayMode.ABSectionThenCustomLoopSection)
			{
				switch (CurrentTransitionType)
				{
					case VideoTransitionType.FadeToBlack:
						{
							videoMaterial.SetFloat(AlternateTexWeight, 0);
							VideoCut();
						}
						//StartCoroutine(JustFadeToBlack(false, ()=> StartCoroutine(JustFadeToClear())));
						break;
					case VideoTransitionType.Crossfade:
						StartCoroutine(JustFadeToClear());
						break;
					case VideoTransitionType.NoTransition:
						{
							videoMaterial.SetFloat(AlternateTexWeight, 0);
							VideoCut();
						}
						break;
					default:
						break;
				}
			}
			else
			{
				switch (CurrentTransitionType)
				{
					case VideoTransitionType.FadeToBlack:
						StartCoroutine(JustFadeToClear());
						break;
					case VideoTransitionType.Crossfade:
						StartCoroutine(CrossFade());
						break;
					case VideoTransitionType.NoTransition:
						{
							videoMaterial.SetFloat(AlternateTexWeight, 0);
							VideoCut();
						}
						break;
					default:
						break;
				}
			}
        }

		private IEnumerator JustFadeToBlack(bool endOfVideo, Action onFadeComplete = null)
		{
			yield return new WaitForEndOfFrame();

			// Creates a black texture
			Texture2D blackTexture = new(alternateTexture.width, alternateTexture.height);
			Color[] pixels = blackTexture.GetPixels();
			for (int i = 0; i < pixels.Length; i++)
			{
				pixels[i] = Color.black;
			}
			blackTexture.SetPixels(pixels);
			blackTexture.Apply();

			// Applies the black texture
			Graphics.Blit(blackTexture, alternateTexture);

			// Fades to the alt texture (black)
			float elapsedTime = 0f;
			while (elapsedTime < lerpDuration)
			{
				elapsedTime += Time.deltaTime;
				float currentValue = Mathf.Lerp(0f, 1f, elapsedTime / lerpDuration);

				videoMaterial.SetFloat(AlternateTexWeight, currentValue);

				yield return null;
			}

			if(endOfVideo)
				EndPointOfVideo();

			onFadeComplete?.Invoke();
		}

		private IEnumerator JustFadeToClear()
		{
			float elapsedTime = 0f;
			// Fades to the main texture
			elapsedTime = 0f;
			while (elapsedTime < lerpDuration)
			{
				elapsedTime += Time.deltaTime;
				float currentValue = Mathf.Lerp(0f, 1f, elapsedTime / lerpDuration);

				videoMaterial.SetFloat(AlternateTexWeight, 1 - currentValue);

				yield return null;
			}

		}

        private void SetFinalFrame()
        {
	        Material material = videoPlayerItemSO.settings.First().videoSkyboxMaterial;
	        //alternateTexture = new RenderTexture(material.mainTexture.width, material.mainTexture.height, 0);
	        Graphics.Blit(material.mainTexture, alternateTexture);
	        RenderTexture.active = null;

//	        material.mainTexture = alternateTexture;
        }
        
        private IEnumerator CrossFade()
        {
			// Material material = videoPlayerItemSO.settings.First().videoSkyboxMaterial;
			// alternateTexture.width = material.mainTexture.width;
			// alternateTexture.height = material.mainTexture.height;
			// Graphics.Blit(material.mainTexture, alternateTexture);
			// RenderTexture.active = null;

			// Sets the blend to be the coppied texture
			videoMaterial.SetFloat(AlternateTexWeight, 1);
	        
	        // Starts the play
	        //SwitchVideo();

	        // Fades to the main texture
	        float elapsedTime = 0f;
	        while (elapsedTime < lerpDuration)
	        {
		        elapsedTime += Time.deltaTime;
		        float currentValue = Mathf.Lerp(0f, 1f, elapsedTime / lerpDuration);

		        videoMaterial.SetFloat(AlternateTexWeight, 1 - currentValue);

		        yield return null;
	        }
        }
        
        private void VideoCut()
        {
	        SwitchVideo();
        }

		#endregion Video Transition
	}
}