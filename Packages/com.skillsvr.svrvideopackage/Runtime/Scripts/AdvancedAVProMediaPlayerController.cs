using System;
using UnityEngine;
using UnityEngine.Video;
using System.IO;
using System.Collections.Generic;
using System.Linq;

#if AV_PRO
using RenderHeads.Media.AVProVideo;

namespace SkillsVR.VideoPackage
{
	public class AdvancedAVProMediaPlayerController : MonoBehaviour, IAdvancedVideoPlayer
	{
		#region IAdvancedVideoControl Properties
		public bool CanPlay
		{
			get
			{
				return null != mediaPlayer 
					&& !string.IsNullOrWhiteSpace(VideoAssetLocation) 
					&& (LoadingSuccess || mediaPlayer.Control.CanPlay());
			}
		}

		public bool LoadingSuccess { get; private set; }

		public MediaPlayer.FileLocation FileLocation { get; set; } = MediaPlayer.FileLocation.RelativeToPersistentDataFolder;

        public string VideoAssetLocation
		{
			get
			{
				return null == mediaPlayer? null: mediaPlayer.m_VideoPath;
			}
			set
			{
				LoadingSuccess = false;
				if (null == mediaPlayer || string.IsNullOrWhiteSpace(value))
				{
					return;
				}
				string fileLocationStr = value;
				if (!Application.isEditor)
				{
                    fileLocationStr = Path.GetFileName(fileLocationStr);
                }
				else
				{
					FileLocation = MediaPlayer.FileLocation.RelativeToProjectFolder;
				}
                LoadingSuccess = mediaPlayer.OpenVideoFromFile(FileLocation, fileLocationStr, false);
			}
		}

		public float CurrentPlayTime
		{
			get => null == mediaPlayer? 0.0f : mediaPlayer.Control.GetCurrentTimeMs() * 0.001f;
			set => Seek(value);
		}
		public float CurrentVideoDuration
		{
			get => null == mediaPlayer? 0.0f : mediaPlayer.Info.GetDurationMs() * 0.001f;
		}
		public bool IsPlaying => null == mediaPlayer ? false: mediaPlayer.Control.IsPlaying() ;

		#endregion IAdvancedVideoControl Properties


		#region IAdvancedVideoPlayer Properties
		public VideoImageType ImageType
		{
			get
			{
				return GetVideoImage();
			}
			set
			{
				SetVideoImage(value);
			}
		}

		public VideoLayoutType LayoutType
		{
			get
			{
				return GetVideoLayoutType();
			}
			set
			{
				SetVideoLayoutType(value);
			}
		}

		public AudioSource AudioSource => mediaPlayer?.GetComponent<AudioSource>();
		public GameObject CustomPresetPrefabGameObject { get; set; }
		public RenderTexture CurrentRenderTexture { get; set; }
		public Material SkyBoxMaterial { get; set; }

		public int VirtualNorthAlignment { get; protected set; }

		#endregion IAdvancedVideoPlayer Properties

		protected VideoPlayerEventDelegate videoPlayerEvents;

		protected MediaPlayer mediaPlayer;

		protected GameObject instanceObjectRoot;



		#region IAdvancedVideoControl Methods
		public T GetPlayerInstance<T>() where T : class
		{
			if (mediaPlayer is T)
			{
				return mediaPlayer as T;
			}
			return null;
		}

		public Type GetPlayerInstanceType()
		{
			return mediaPlayer?.GetType();
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

		public void SetupFromVideoData(VideoPlayerItem videoPlayerData)
		{
			if (null == videoPlayerData)
			{
				return;
			}

			if (null != instanceObjectRoot)
			{
				GameObject.DestroyImmediate(instanceObjectRoot);
				instanceObjectRoot = null;
			}

			if (null != mediaPlayer)
			{
				GameObject.DestroyImmediate(mediaPlayer);
				mediaPlayer = null;
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
			FileLocation = GetFileLocationFromPlayerItem(videoPlayerData);
			var settings = videoPlayerData.GetSettingByName("AVPro");
			if (null == settings)
			{
                return;
            }
            CustomPresetPrefabGameObject = settings.sphereVideoPrefab;
            SkyBoxMaterial = settings.videoSkyboxMaterial;
			VirtualNorthAlignment = settings.virtualNorthAlignment;
        }

		private MediaPlayer.FileLocation GetFileLocationFromPlayerItem(VideoPlayerItem videoPlayerData)
		{
			if (null == videoPlayerData)
			{
				return MediaPlayer.FileLocation.RelativeToPersistentDataFolder;
			}
			var location = videoPlayerData.videoLocationType;
            switch (location)
            {
                case VideoPlayerItem.VideoLocation.AbsolutePathOrURL: 
					return MediaPlayer.FileLocation.AbsolutePathOrURL;
                case VideoPlayerItem.VideoLocation.RelativeToProjectFolder:
					return MediaPlayer.FileLocation.RelativeToProjectFolder;
                case VideoPlayerItem.VideoLocation.RelativeToStreamingAssetsFolder:
					return MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder;
                case VideoPlayerItem.VideoLocation.RelativeToDataFolder:
					return MediaPlayer.FileLocation.RelativeToDataFolder;
                case VideoPlayerItem.VideoLocation.RelativeToPersistentDataFolder:
                default:
					return MediaPlayer.FileLocation.RelativeToPersistentDataFolder;
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
			if  (null  != callback)
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
		#endregion IAdvancedVideoControl Methods



		#region IAdvancedVideoControl Methods
		public void Play(Action onPlayStart = null)
		{
			if (null != onPlayStart)
			{
				RegisterOneTimeCallback(VideoPlayerEventType.Started, (p, k, e) => { onPlayStart?.Invoke(); });
			}
            PlayInternal();
		}

		private void PlayInternal()
		{
			SetupMediaPlayer();
			mediaPlayer?.Play();
		}

		public void Pause()
		{
			mediaPlayer?.Pause();
		}


		public void Seek(float timeInSec, Action onSeekFinish = null)
		{
			if (null != onSeekFinish)
			{
				RegisterOneTimeCallback(VideoPlayerEventType.FinishedSeeking, (p, k, e) => {
					onSeekFinish?.Invoke();
				});
			}

			if(timeInSec != 0)
			{
				timeInSec = Mathf.Clamp(timeInSec, 0.0f, CurrentVideoDuration);
				mediaPlayer?.Control.SeekFast(timeInSec * 1000);
			}
			else
				mediaPlayer?.Control.Rewind();

		}

		public void Stop()
		{
			mediaPlayer?.Stop();
		}

		public void Reset()
		{
			Stop();
		}
		#endregion IAdvancedVideoControl Methods

		#region MonoBehaviour
		public void Awake()
		{
		}

		public void OnEnable()
		{
			InitPlayer();
		}
		public void OnDisable()
		{
			DeinitPlayer();
		}
		public void Start()
		{
			InitPlayer();
		}

		public void OnDestroy()
		{
		}
		#endregion MonoBehivour


		#region Init & Setup
		private void InitPlayer()
		{
			if (null == mediaPlayer)
			{
				mediaPlayer = CreateMediaPlayer();
				SetupMediaPlayer();
				mediaPlayer.Events.AddListener(OnMediaPlayerEvents);
			}
		}

		private void DeinitPlayer()
		{
			if (null != mediaPlayer)
			{
				mediaPlayer.Events.RemoveListener(OnMediaPlayerEvents);
			}
		}

		private MediaPlayer CreateMediaPlayer()
		{
			MediaPlayer player = null;
			if (null != CustomPresetPrefabGameObject)
			{
				instanceObjectRoot = GameObject.Instantiate(CustomPresetPrefabGameObject, this.transform);
				player = instanceObjectRoot.GetComponentInChildren<MediaPlayer>(true);
			}
			if (null == player)
			{
				player = this.gameObject.AddComponent<MediaPlayer>();
			}
			return player;
		}

		protected void SetupMediaPlayer()
		{
			if (null == mediaPlayer)
			{
				return;
			}
			mediaPlayer.m_AutoOpen = false;
			mediaPlayer.m_AutoStart = false;
			mediaPlayer.m_Loop = true;
		}
		#endregion Init & Setup

		#region Media Player Event Callbacks
		private void OnMediaPlayerEvents(MediaPlayer arg0, MediaPlayerEvent.EventType arg1, ErrorCode arg2)
		{
			VideoPlayerEventType interfaceEventType = MediaPlayerEventToVideoPlayerEvent(arg1);
			string error = VideoPlayerEventType.UnknowEvent == interfaceEventType ? arg1.ToString() : arg2.ToString();
			videoPlayerEvents?.Invoke(this, interfaceEventType, error);
		}

		private VideoPlayerEventType MediaPlayerEventToVideoPlayerEvent(MediaPlayerEvent.EventType mediaPlayerEventType)
		{
			switch (mediaPlayerEventType)
			{
				case MediaPlayerEvent.EventType.ReadyToPlay: return VideoPlayerEventType.ReadyToPlay;
				case MediaPlayerEvent.EventType.Started: return VideoPlayerEventType.Started;
				case MediaPlayerEvent.EventType.FinishedPlaying: return VideoPlayerEventType.FinishedPlaying;
				case MediaPlayerEvent.EventType.Closing: return VideoPlayerEventType.Closing;
				case MediaPlayerEvent.EventType.Error: return VideoPlayerEventType.Error;
				case MediaPlayerEvent.EventType.StartedSeeking: return VideoPlayerEventType.StartedSeeking;
				case MediaPlayerEvent.EventType.FinishedSeeking: return VideoPlayerEventType.FinishedSeeking;
				default: return VideoPlayerEventType.UnknowEvent;
			}
		}
		#endregion Media Player Event Callbacks


		private void SetVideoLayoutType(VideoLayoutType videoLayoutType)
		{

#if UNITY_EDITOR
			mediaPlayer.m_StereoPacking = StereoPacking.None;
			return;
#endif

			switch (videoLayoutType)
			{
				case VideoLayoutType.None:
					mediaPlayer.m_StereoPacking = StereoPacking.None;
					break;
				case VideoLayoutType.SideBySide:
					mediaPlayer.m_StereoPacking = StereoPacking.LeftRight;
					break;
				case VideoLayoutType.OverUnder:
					mediaPlayer.m_StereoPacking = StereoPacking.TopBottom;
					break;
				default:
					mediaPlayer.m_StereoPacking = StereoPacking.None;
					break;
			}
		}
		private VideoLayoutType GetVideoLayoutType()
		{
			if (null == mediaPlayer)
			{
				return VideoLayoutType.None;
			}
			switch (mediaPlayer.m_StereoPacking)
			{
				case StereoPacking.None: return VideoLayoutType.None;
				case StereoPacking.TopBottom: return VideoLayoutType.OverUnder;
				case StereoPacking.LeftRight: return VideoLayoutType.SideBySide;
				default: return VideoLayoutType.None;
			}
		}

		private void SetVideoImage(VideoImageType videoImageType)
		{
			switch (videoImageType)
			{
				case VideoImageType.ThreeSixtyImage:
					mediaPlayer.VideoLayoutMapping = VideoMapping.EquiRectangular360;
					break;
				case VideoImageType.OneEightyImage:
					mediaPlayer.VideoLayoutMapping = VideoMapping.EquiRectangular180;
					break;
				default:
					mediaPlayer.VideoLayoutMapping = VideoMapping.Unknown;
					break;
			}
		}

		private VideoImageType GetVideoImage()
		{
			if (null == mediaPlayer)
			{
				return VideoImageType.Unknown;
			}

			switch (mediaPlayer.VideoLayoutMapping)
			{
				case VideoMapping.EquiRectangular360: return VideoImageType.ThreeSixtyImage;
				case VideoMapping.EquiRectangular180: return VideoImageType.OneEightyImage;
				default: return VideoImageType.Unknown;
			}
		}
		
	}
}

#endif