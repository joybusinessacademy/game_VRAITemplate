using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;

namespace SkillsVR.VideoPackage
{

	// AdvancedVideoPlayerController
	// Mostly play video with unity VideoPlayer in edit mode.
	// For better performance, video is set to loop, and override stop to pause.
	[ExecuteInEditMode]
	public class AdvancedVideoPlayerController : MonoBehaviour, IAdvancedVideoPlayer
	{
		public AudioSource AudioSource => videoPlayer?.GetComponent<AudioSource>();

		bool wasPlayed;
		public bool CanPlay
		{
			get
			{
				return null != videoPlayer && !string.IsNullOrWhiteSpace(VideoAssetLocation) 
					&& (videoPlayer.canSetTime || wasPlayed);
			}
		}
		public string VideoAssetLocation
		{
			get
			{
				if (null == videoPlayer)
				{
					return string.Empty;
				}
				return videoPlayer.url;
			}
			set
			{
				Stop();
				wasPlayed = false;
				if (null == videoPlayer)
				{
					return;
				}
				if (string.IsNullOrWhiteSpace(value))
				{
					videoPlayer.clip = null;
					videoPlayer.url = null;
					return;
				}
				string url = value;
				if (!Application.isEditor)
				{
					string fileName = Path.GetFileName(value);
					url = Path.Combine(Application.streamingAssetsPath, fileName);
				}
				
				videoPlayer.source = VideoSource.Url;
				videoPlayer.url = url;
				try
				{
					videoPlayer.Prepare();
				}
				catch(Exception e)
				{
					videoPlayerEvents.Invoke(this, VideoPlayerEventType.Error, e.Message);
				}
			}
		}

		public int VirtualNorthAlignment { get; protected set; }
		public RenderTexture CurrentRenderTexture { get; set; }

		private float timeWithEndPoint;
		public float CurrentPlayTime
		{
			get
			{
				if (IsPlaying)
				{
					timeWithEndPoint = (float)videoPlayer.time;
				}

				return timeWithEndPoint;
			}
			set
			{
				Seek(value);
			}
		}

		public float CurrentVideoDuration
		{
			get
			{
				if (null == videoPlayer)
				{
					return 0.0f;
				}
				return (float)videoPlayer.length;
			}
		}

		public bool IsPlaying => null == videoPlayer ? false : videoPlayer.isPlaying;

		public VideoImageType ImageType {
			get {
				return GetVideoImage();
			}
			set {
				SetVideoImage(value);
			}
		}

		public VideoLayoutType LayoutType {
			get {
				return GetVideoLayoutType();
			}
			set {
				SetVideoLayoutType(value);
			}
		}

		public GameObject CustomPresetPrefabGameObject { get; set; }

		private Material mySkyBoxMaterial;
		public Material SkyBoxMaterial
		{
			get
			{
				return mySkyBoxMaterial;
			}
			set
			{
				mySkyBoxMaterial = value;
				if (null != mySkyBoxMaterial)
				{
					mySkyBoxMaterial?.SetFloat("_Weight", 0);
				}
			}
		}

		protected VideoPlayer videoPlayer;
		protected VideoPlayerEventDelegate videoPlayerEvents;

		private bool playOnReady;

		protected IVideoPlayerEditorViewController editorViewController;

		protected bool userPlay;
		protected bool internalIsSeeking;

		public T GetPlayerInstance<T>() where T : class
		{
			if (videoPlayer is T)
			{
				return videoPlayer as T;
			}
			return null;
		}

		public Type GetPlayerInstanceType()
		{
			return videoPlayer?.GetType();
		}

		protected void Awake()
		{
		}

		protected void OnDestroy()
		{
			Stop();
			editorViewController = null;
		}

		protected void OnEnable()
		{
			InitEditorViewController();
			InitPlayer();
			Reset();
			editorViewController?.StartUpdateInEditor();
		}

		protected void Start()
		{
			InitPlayer();
		}

		protected void Update()
		{
			OnUpdateDelayInvokeSeekCallback();
			if (!wasPlayed && IsPlaying)
			{
				wasPlayed = true;
			}
		}

		private void InitEditorViewController()
		{
			if (!Application.isEditor)
			{
				return;
			}
			string editorViewControllerTypeName = "SkillsVR.VideoPackage.VideoPlayerEditorViewController";
			editorViewController = GetInstanceFromTypeName(editorViewControllerTypeName) as IVideoPlayerEditorViewController;
			if (null == editorViewController)
			{
				Debug.LogError("Cannot find " + editorViewControllerTypeName);
				return;
			}
			editorViewController.videoPlayer = videoPlayer;
		}

		protected void OnDisable()
		{
			UnRegisterVideoPlayerEvents();
			editorViewController?.StopUpdateInEditor();
		}

		public void Reset()
		{
			Stop();
		}

		private VideoPlayer CreateMediaPlayer()
		{
			VideoPlayer player = null;
			if (null != CustomPresetPrefabGameObject)
			{
				var prefabInstance = GameObject.Instantiate(CustomPresetPrefabGameObject, this.transform);
				player = prefabInstance.GetComponentInChildren<VideoPlayer>(true);
			}

			if (null == player)
			{
				player = this.gameObject.AddComponent<VideoPlayer>();
			}

			return player;
		}

		protected void SetupVideoPlayer()
		{
			if (null == videoPlayer)
			{
				return;
			}
			videoPlayer.renderMode = VideoRenderMode.RenderTexture;
			videoPlayer.skipOnDrop = false;
			videoPlayer.waitForFirstFrame = true;
			videoPlayer.playOnAwake = false;
			videoPlayer.isLooping = true;
			videoPlayer.targetTexture = CurrentRenderTexture;
		}

		public void Play(Action onComplete = null)
		{
			if (IsPlaying)
			{
				return;
			}
			userPlay = true;
			SetupVideoPlayer();

			RegisterOneTimeCallback(VideoPlayerEventType.Started, (p, k, e) => {
				onComplete?.Invoke();
			});
			if (null != videoPlayer)
			{
				videoPlayer.Play();
			}

			playOnReady = !videoPlayer.isPrepared;
			RequestForceUpdateView(true);
		}

		public void Pause()
		{
			if (null != videoPlayer)
			{
				videoPlayer.Pause();
			}
			userPlay = false;
			RequestForceUpdateView(false);
		}

		public void Seek(float timeInSec, Action onComplete = null)
		{
			if (null == videoPlayer)
			{
				onComplete?.Invoke();
				return;
			}

			// Seek start event;
			videoPlayerEvents?.Invoke(this, VideoPlayerEventType.StartedSeeking, string.Empty);


			bool requestPlayVideoToRefreshEvents = true;
			if (videoPlayer.isPrepared)
			{
				// Seek if video is ready.
				timeInSec = Mathf.Clamp(timeInSec, 0.0f, CurrentVideoDuration);
				requestPlayVideoToRefreshEvents = SeekInterial(timeInSec, onComplete);
			}
			else 
			{
				// Wait video ready then seek, require Play() to load video.
				RegisterOneTimeCallback(VideoPlayerEventType.Started, (a, b, c) => { SeekInterial(timeInSec, onComplete); });
			}
            // Play video for seeking. Otherwise the finish seek event will never be called.
            if (requestPlayVideoToRefreshEvents && !IsPlaying)
            {
                videoPlayer.Play();
				RequestForceUpdateView(true);
			}
        }

		/// <summary>
		/// Seek time in clip duration.
		/// </summary>
		/// <param name="timeInSec">time to seek</param>
		/// <param name="onComplete">on complete callback</param>
		/// <returns>true = Require videoPlayer.Play() to update seek proc.</returns>
		private bool SeekInterial(float timeInSec, Action onComplete = null)
		{
			timeInSec = Mathf.Clamp(timeInSec, 0.0f, CurrentVideoDuration);
			targetSeekingTime = timeInSec;
			internalIsSeeking = true;

			if (CurrTimeApprTo(timeInSec))
			{
				internalIsSeeking = false;
				if (!userPlay)
				{
					Pause();
				}
				videoPlayerEvents?.Invoke(this, VideoPlayerEventType.FinishedSeeking, null);
				onComplete?.Invoke();
				return false;
			}

			RegisterOneTimeCallback(VideoPlayerEventType.FinishedSeeking, (p, k, e) => {
				if (null != onComplete)
				{
					delaySeekCallbacks += onComplete;
				}
			});
			videoPlayer.time = timeInSec;
			return true;
		}

		private event Action delaySeekCallbacks;
		private float targetSeekingTime;
		private void OnUpdateDelayInvokeSeekCallback()
		{
			if (!internalIsSeeking)
			{
				return;
			}

			if (!CurrTimeApprTo(targetSeekingTime))
			{
				return;
			}

			internalIsSeeking = false;
			if (!userPlay)
			{
				Pause();
			}

			delaySeekCallbacks?.Invoke();
			delaySeekCallbacks = null;
		}

		private bool CurrTimeApprTo(float time)
		{
			return Mathf.Abs(CurrentPlayTime - time) < 0.1f;
		}

		public void Stop()
		{
			if (IsPlaying)
			{
				wasPlayed = IsPlaying;
			}
			userPlay = false;
			if (null != videoPlayer)
			{
				videoPlayer.Pause();
			}
			RequestForceUpdateView(false);
		}

		private void RequestForceUpdateView(bool shouldUpdate)
		{
			if (null != editorViewController)
			{
				editorViewController.forceUpdateView = shouldUpdate;
			}
		}
		public void AddListener(VideoPlayerEventDelegate callback)
		{
			if (null != callback)
			{
				videoPlayerEvents += callback;
			}
		}

		public void RemoveAllListeners()
		{
			videoPlayerEvents = null;
		}

		public void RemoveListener(VideoPlayerEventDelegate callback)
		{
			if (null != callback)
			{
				videoPlayerEvents -= callback;
			}
		}

		private Dictionary<VideoPlayerEventDelegate, VideoPlayerEventDelegate> managedOneTimeCallbacks = new Dictionary<VideoPlayerEventDelegate, VideoPlayerEventDelegate>();
		public void RegisterOneTimeCallback(VideoPlayerEventType eventType, VideoPlayerEventDelegate callback)
		{
			VideoPlayerEventDelegate persistanceCallback = null;
			VideoPlayerEventDelegate oneTimeCallback = (player, eventKey, error) => {
				if (eventKey != eventType)
				{
					return;
				}
				RemoveListener(persistanceCallback);
				callback?.Invoke(player, eventKey, error);
			};
			persistanceCallback = oneTimeCallback;
			if (null != callback)
			{
				managedOneTimeCallbacks.Add(persistanceCallback, callback);
			}
			AddListener(oneTimeCallback);
		}


		public void UnregisterOneTimeCallback(VideoPlayerEventDelegate callback)
		{
			if (null == callback)
			{
				return;
			}
			var managedItem = managedOneTimeCallbacks.FirstOrDefault(x => x.Value == callback);
			if (null == managedItem.Key)
			{
				return;
			}
			managedOneTimeCallbacks.Remove(managedItem.Key);
			RemoveListener(managedItem.Key);
		}

		void RegisterVideoPlayerEvents()
		{
			UnRegisterVideoPlayerEvents();
			if (null == videoPlayer)
			{
				return;
			}
			videoPlayer.prepareCompleted += OnVideoPlayerPrepareCompleted;
			videoPlayer.seekCompleted += OnVideoPlayerSeekCompleted;
			videoPlayer.started += OnVideoPlayerStarted;
			videoPlayer.loopPointReached += OnVideoLoopPointReached;
		}

		private void OnVideoLoopPointReached(VideoPlayer source)
		{
			timeWithEndPoint = CurrentVideoDuration;
		}

		void UnRegisterVideoPlayerEvents()
		{
			if (null == videoPlayer)
			{
				return;
			}
			videoPlayer.prepareCompleted -= OnVideoPlayerPrepareCompleted;
			videoPlayer.seekCompleted -= OnVideoPlayerSeekCompleted;
			videoPlayer.started -= OnVideoPlayerStarted;
			videoPlayer.loopPointReached -= OnVideoLoopPointReached;
		}



		private void OnVideoPlayerStarted(VideoPlayer source)
		{
			videoPlayerEvents?.Invoke(this, VideoPlayerEventType.Started, string.Empty);
		}

		private void OnVideoPlayerSeekCompleted(VideoPlayer source)
		{
			videoPlayerEvents?.Invoke(this, VideoPlayerEventType.FinishedSeeking, string.Empty);
		}

		private void OnVideoPlayerPrepareCompleted(VideoPlayer source)
		{
			if (playOnReady)
			{
				playOnReady = false;
				videoPlayer?.Play();
			}
			videoPlayerEvents?.Invoke(this, VideoPlayerEventType.ReadyToPlay, string.Empty);
		}

		private Type GetTypeFromTypeName(string typeName)
		{
			if (string.IsNullOrWhiteSpace(typeName))
			{
				return null;
			}
			var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
			Type type = allAssemblies
				.FirstOrDefault(a => null != a.GetType(typeName))?
				.GetType(typeName);
			return type;
		}

		private object GetInstanceFromTypeName(string typeName)
		{
			var type = GetTypeFromTypeName(typeName);
			if (null == type)
			{
				return null;
			}
			return Activator.CreateInstance(type);
		}

		public void SetupFromVideoData(VideoPlayerItem videoPlayerData)
		{
			if (null == videoPlayerData)
			{
				return;
			}

			if (null != videoPlayer)
			{
				GameObject.DestroyImmediate(videoPlayer);
				videoPlayer = null;
			}

			RefreshFromVideoData(videoPlayerData);
            InitPlayer();
		}


        public void RefreshFromVideoData(VideoPlayerItem videoPlayerData)
        {
            if (null == videoPlayerData)
            {
                return;
            }

			var settings = videoPlayerData.GetSettingByName("VideoPlayer");
			if (null == settings)
			{
				return;
			}

			CurrentRenderTexture = settings.videoRenderTexture;
			SkyBoxMaterial = settings.videoSkyboxMaterial;
			VirtualNorthAlignment = settings.virtualNorthAlignment;
        }

        void InitPlayer()
		{
			if (null == videoPlayer)
			{
				videoPlayer = GetComponent<VideoPlayer>();
			}
			if (null == videoPlayer)
			{
				videoPlayer = CreateMediaPlayer();
			}
			SetupVideoPlayer();
			RegisterVideoPlayerEvents();
			if (null != editorViewController)
			{
				editorViewController.videoPlayer = videoPlayer;
			}
		}

		private void SetVideoLayoutType(VideoLayoutType videoLayoutType)
		{
			switch (videoLayoutType)
			{
				case VideoLayoutType.None:
					if (null != mySkyBoxMaterial)
						mySkyBoxMaterial?.SetFloat("_Layout", 0);
					break;
				case VideoLayoutType.SideBySide:
					if (null != mySkyBoxMaterial)
						mySkyBoxMaterial?.SetFloat("_Layout", 1);
					break;
				case VideoLayoutType.OverUnder:
					if (null != mySkyBoxMaterial)
						mySkyBoxMaterial?.SetFloat("_Layout", 2);
					break;
				default:
					if (null != mySkyBoxMaterial)
						mySkyBoxMaterial?.SetFloat("_Layout", 0);
					break;
			}
		}
		private VideoLayoutType GetVideoLayoutType()
		{
			float videoLayout = 0;

			if (null != mySkyBoxMaterial)
			{
				videoLayout = (float)(mySkyBoxMaterial?.GetFloat("_Layout"));
			}
			switch (videoLayout)
			{
				case 0: return VideoLayoutType.None;
				case 1: return VideoLayoutType.SideBySide;
				case 2: return VideoLayoutType.OverUnder;
				default: return VideoLayoutType.None;
			}
		}

		private void SetVideoImage(VideoImageType videoImageType)
		{
			switch (videoImageType)
			{
				case VideoImageType.ThreeSixtyImage:
					if (null != mySkyBoxMaterial)
						mySkyBoxMaterial?.SetFloat("_ImageType", 0);
					break;
				case VideoImageType.OneEightyImage:
					if (null != mySkyBoxMaterial)
						mySkyBoxMaterial?.SetFloat("_ImageType", 1);
					break;
				default:
					if (null != mySkyBoxMaterial)
						mySkyBoxMaterial?.SetFloat("_ImageType", 0);
					break;
			}
		}

		private VideoImageType GetVideoImage()
		{
			float videoImage = 0;

			if (null != mySkyBoxMaterial)
			{
				videoImage = (float)(mySkyBoxMaterial?.GetFloat("_ImageType"));
			}

			switch (videoImage)
			{
				case 0: return VideoImageType.ThreeSixtyImage;
				case 1: return VideoImageType.OneEightyImage;
				default: return VideoImageType.Unknown;
			}
		}
	}
}

