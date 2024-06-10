using RenderHeads.Media.AVProVideo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PanelVideoController : MonoBehaviour
{
	public MediaPlayer mediaPlayer;
	public bool someBool = false;

	public float currentVideoTime {get {return mediaPlayer.Control.GetCurrentTimeMs();}}

	private float offsetStartTime = 0;

	public bool UserTriggerPlay { get; protected set; }
	public bool IsReady { get; protected set; }

	public void SetDataForMediaPlayer(bool loopVideo, string videoLocation, float startTime)
	{
		mediaPlayer.m_AutoStart = false;
		mediaPlayer.m_Loop = loopVideo;
		offsetStartTime = startTime;

		IsReady = false;
		UserTriggerPlay = false;

		mediaPlayer.Events.RemoveListener(OnMediaPlayerEvents);
		mediaPlayer.Events.AddListener(OnMediaPlayerEvents);
#if UNITY_EDITOR
		// Assign the VideoClip to the MediaPlayer component
		mediaPlayer.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToProjectFolder, videoLocation, false);
#else
		//string prefix = "VideoNodeExamples/";
		int lastIndex = videoLocation.LastIndexOf('/');
		string fileName = videoLocation.Substring(lastIndex + 1); // Extract the file name after the last '/'
		string convertedPath = fileName;

		// Assign the VideoClip to the MediaPlayer component
		mediaPlayer.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToPersistentDataFolder, convertedPath, false);			
#endif
			
	}

	[ExecuteInEditMode]
	private void Update()
	{
		if(someBool)
		{
			PlayVideo();
			someBool = false;
		}
	}

	
	public void PlayVideo()
	{
		UserTriggerPlay = true;
		TryPlayVideo();
	}

	void TryPlayVideo()
	{
		if (!UserTriggerPlay || !IsReady)
		{
			return;
		}
		
		if (offsetStartTime != 0)
		{
			mediaPlayer.Control.Seek(offsetStartTime * 1000);
		}
		else
		{
			mediaPlayer.Play();
			mediaPlayer.Events.RemoveListener(OnMediaPlayerEvents);
		}
	}
	protected void OnMediaPlayerEvents(MediaPlayer player, MediaPlayerEvent.EventType eventType, ErrorCode errorCode)
	{
		switch(eventType)
		{
			case MediaPlayerEvent.EventType.MetaDataReady:
				{
					IsReady = true;
					TryPlayVideo();
					break;
				}
			case MediaPlayerEvent.EventType.FinishedSeeking:
				{
					mediaPlayer.Play();
					mediaPlayer.Events.RemoveListener(OnMediaPlayerEvents);
					break;
				}
			default: break;
		}
	}

	public void StopVideo()
	{
		mediaPlayer.Events.RemoveListener(OnMediaPlayerEvents);
		mediaPlayer.Stop();
	}
}
