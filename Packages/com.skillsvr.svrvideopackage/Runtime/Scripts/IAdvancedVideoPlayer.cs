using System;
using UnityEngine;
using UnityEngine.Video;

namespace SkillsVR.VideoPackage
{

	public delegate void VideoPlayerEventDelegate(IAdvancedVideoPlayer videoPlayer, VideoPlayerEventType eventType, string error);


	public interface IAdvancedVideoControl
	{
		string VideoAssetLocation { get; set; }
		float CurrentPlayTime { get; set; }
		float CurrentVideoDuration { get; }

		bool IsPlaying { get; }

		bool CanPlay { get; }
		void Seek(float timeInSec, Action onSeekFinish = null);
		void Play(Action onPlayStart = null);
		void Pause();
		void Stop();
		void Reset();
	}

	public interface IAdvancedVideoPlayer : IAdvancedVideoControl
	{
		AudioSource AudioSource { get; }
		GameObject CustomPresetPrefabGameObject { get; set; }
		RenderTexture CurrentRenderTexture { get; set; }
		Material SkyBoxMaterial { get; set; }
		VideoImageType ImageType { get; set; }
		VideoLayoutType LayoutType { get; set; }

		int VirtualNorthAlignment { get; }

		void AddListener(VideoPlayerEventDelegate callback);
		void RemoveListener(VideoPlayerEventDelegate callback);
		void RemoveAllListeners();
		void RegisterOneTimeCallback(VideoPlayerEventType eventType, VideoPlayerEventDelegate callback);
		void UnregisterOneTimeCallback(VideoPlayerEventDelegate callback);

		void SetupFromVideoData(VideoPlayerItem videoPlayerItem);
		void RefreshFromVideoData(VideoPlayerItem videoPlayerData);

        T GetPlayerInstance<T>() where T : class;
		Type GetPlayerInstanceType();
	}

	public enum VideoPlayerEventType
	{
		ReadyToPlay,        // Triggered when the video is loaded and ready to play
		Started,            // Triggered when the playback starts
		FinishedPlaying,    // Triggered when a non-looping video has finished playing
		Closing,            // Triggered when the media is closed
		Error,              // Triggered when an error occurs
		StartedSeeking,     // Triggered when seeking begins
		FinishedSeeking,    // Triggered when seeking has finished
		UnknowEvent,
	}

	public enum VideoTransitionType
	{
		NoTransition,
		Crossfade,
		FadeToBlack
	}

	public enum VideoImageType
	{
		ThreeSixtyImage = 180,
		OneEightyImage = 360,
		Unknown
	}

	public enum VideoLayoutType
	{
		None,
		SideBySide,
		OverUnder
	}
}

