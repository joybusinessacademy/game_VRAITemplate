using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using DialogExporter;
using GraphProcessor;
using SkillsVR.Mechanic.Core;
using SkillsVR.VideoPackage;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.XR.Interaction.Toolkit;

namespace SkillsVRNodes.Scripts.Nodes
{
	
	[Serializable, NodeMenuItem("Flow/360 Video", typeof(SceneGraph)), NodeMenuItem("Flow/360 Video", typeof(SubGraph))]
	public class PanoramaVideoNode : ExecutableNode
	{
#if PANORAMA_VIDEO
		public enum PlayMode
		{
			Section,
			LoopSection,
			SectionThanLoopLastSeconds,
		}
		public override string name => "360 Video";
		public override string icon => "Play";
		public override string layoutStyle => "PanoramaNode";
		public override string tooltipURL => "https://skillsvr-documentation.vercel.app/nodes/learner-experience-node-types#360-video-node";
		public override Color color => NodeColours.Other;
		public override int Width => MEDIUM_WIDTH;

		public string AssociatedCustomClip;

		public string videoClipLocation;

		[NonSerialized]
		public PanoramaVideoSystem panoramaVideoSystem;
		[NonSerialized]
		public GameObject videoGameobject;

		public VideoPlayerItem videoPlayerItem;

		public VideoTransitionType transitionType = VideoTransitionType.FadeToBlack;
		public float fadeDuration = 0.5f;
		public VideoImageType videoImageType = VideoImageType.ThreeSixtyImage;
		public VideoLayoutType videoLayoutType = VideoLayoutType.None;

		public PlayMode playMode;

		[Obsolete("loopVideo is obsoleted. Use playMode = PlayMode.Loop instead.")]
		public bool loopVideo;
		public float videoStartTime;
		public float videoCutoffTime;

		public float loopSectionStartTime;
		public float loopSectionEndTime;

		[Obsolete("loopSectionVideo is obsoleted. Use playMode = PlayMode.PlayAndLoopSection instead.")]
		public bool loopSectionVideo;
		[Obsolete("loopSectionDuration is obsoleted. Use loopSectionStartTime and loopSectionEndTime instead.")]
		public float loopSectionDuration;

		public int virtualNorth = 0;
		public float volumeSetting = 1;
		public float volumeBooster = 1;

		public int videoWidth = 1920;
		public int videoHeight = 1080;

		public float videoClipLengthCache;

		public int ver;

		public AudioClip customTrackAudio;

		public Dictionary<string, string> mechanicDropdownItems = new Dictionary<string, string>
		{
			{ "Multiple Choice Question", "MultipleChoiceQuestionNode" },
			{ "RankSort Order", "RankOrderNode" },
			{ "Popup", "PopupNode" }
		};

		protected override void OnStart()
		{
			GenerateItems();
			base.OnStart();
		}

		private void GenerateItems()
		{
			FindOrCreateVideoSystem();

			ProcessDataUpdate(panoramaVideoSystem.DataVersion);

			panoramaVideoSystem.Reset();
			panoramaVideoSystem.SetupVideoPlayerItem(videoPlayerItem);

			panoramaVideoSystem.currentVideoPlayer.RegisterOneTimeCallback(VideoPlayerEventType.Error, OnVideoError);
			panoramaVideoSystem.currentVideoPlayer.RegisterOneTimeCallback(VideoPlayerEventType.ReadyToPlay, OnVideoReadyToPlay);
			// Load this node video asset
			panoramaVideoSystem.SetVideoLocation(videoClipLocation);

			// Complete if load fail
			if (Mathf.Approximately(videoStartTime, videoCutoffTime) || !panoramaVideoSystem.CanPlay)
			{
				CompleteVideoSystem();
				return;
			}

			panoramaVideoSystem.PreformVideoEnterTransitions();
			// Wait for video loading done.
			
		}

		private void OnVideoError(IAdvancedVideoPlayer videoPlayer, VideoPlayerEventType eventKey, string error)
		{
			panoramaVideoSystem?.RestorePrevSkyBoxMaterial();
            CompleteNode();
        }

		private void OnVideoReadyToPlay(IAdvancedVideoPlayer videoPlayer, VideoPlayerEventType eventKey, string error)
		{
            if (!panoramaVideoSystem.CanPlay)
            {
                CompleteVideoSystem();
                return;
            }

            RegisterMechanicEventsToVideo();
            
			panoramaVideoSystem.onFirstTimeReachPlayEndPoint += CompleteVideoSystem;
            panoramaVideoSystem.ApplySkyBoxMaterial();
            // Setup and play.
            bool successStart = panoramaVideoSystem
                .SetVirtualNorth(virtualNorth)
                .SetupAudioVolume(volumeSetting, volumeBooster)
                .SetTransitionType(transitionType)
                .SetTransitionDuration(fadeDuration)
                .SetPlayMode(playMode.ToVideoPlayMode())
                .SetPlaySection(videoStartTime, videoCutoffTime)
                .SetImageType(videoImageType, videoLayoutType)
                .SetCustomLoopSection(loopSectionStartTime, loopSectionEndTime)
                .SetCustomAudioClip(customTrackAudio)
				.SetRenderTextureSize(videoWidth, videoHeight)
				.PlayWithConfig();

			

            if (!successStart)
            {
                CompleteVideoSystem();
            }
        }

		private void RegisterMechanicEventsToVideo()
		{
			int index = 0;
			foreach (var item in panoramaMechanicEventDatas)
			{
				int localCacheIndex = index;
				panoramaVideoSystem.AddOneOffTimeEvent(item.mechanicEventTime, () => { OnMechanicEventOutput(localCacheIndex); });
				++index;
			}
		}
		private void FindOrCreateVideoSystem()
		{
			if (panoramaVideoSystem == null)
			{
				panoramaVideoSystem = GameObject.FindObjectOfType<PanoramaVideoSystem>();

				if (panoramaVideoSystem == null)
				{
					if (videoGameobject == null)
						videoGameobject = new GameObject("VideoPlayer - Runtime");

					panoramaVideoSystem = videoGameobject.AddComponent<PanoramaVideoSystem>();

				}
				else
				{
					videoGameobject = panoramaVideoSystem.gameObject;
					videoGameobject.name = "VideoPlayer - Runtime (Existed from Editor)";
				}
			}
		}

		private void CompleteVideoSystem()
		{
			if (null != panoramaVideoSystem)
			{
                panoramaVideoSystem.onFirstTimeReachPlayEndPoint -= CompleteVideoSystem;
                if (null != panoramaVideoSystem.currentVideoPlayer)
				{
                    panoramaVideoSystem.currentVideoPlayer.UnregisterOneTimeCallback(OnVideoReadyToPlay);
                    panoramaVideoSystem.currentVideoPlayer.UnregisterOneTimeCallback(OnVideoError);
                }
				panoramaVideoSystem.RestorePrevSkyBoxMaterial();
            }
			CompleteNode();
		}

		private void OnMechanicEventOutput(int output)
		{
			selectedAnswer = output;
			RunSelectedOutput();
		}

		protected override void Destroy()
		{
			if (Application.isPlaying)
				return;

			panoramaVideoSystem = GameObject.FindObjectOfType<PanoramaVideoSystem>();

			if (panoramaVideoSystem != null)
				GameObject.DestroyImmediate(panoramaVideoSystem.gameObject);

			base.Destroy();
		}

		private int selectedAnswer;

		public void RunSelectedOutput()
		{
			
			if (this.nodeActive)
				RunLink("Output" + (selectedAnswer + 1), false);
		}

		[Output(name = "Spawn Mechanic:")]
		public ConditionalLink Output1 = new();
		[Output(name = "Spawn Mechanic:")]
		public ConditionalLink Output2 = new();
		[Output(name = "Spawn Mechanic:")]
		public ConditionalLink Output3 = new();
		[Output(name = "Spawn Mechanic:")]
		public ConditionalLink Output4 = new();
		[Output(name = "Spawn Mechanic:")]
		public ConditionalLink Output5 = new();
		[Output(name = "Spawn Mechanic:")]
		public ConditionalLink Output6 = new();
		[Output(name = "Spawn Mechanic:")]
		public ConditionalLink Output7 = new();
		[Output(name = "Spawn Mechanic:")]
		public ConditionalLink Output8 = new();
		[Output(name = "Spawn Mechanic:")]
		public ConditionalLink Output9 = new();
		[Output(name = "Spawn Mechanic:")]
		public ConditionalLink Output10 = new();

		public string GetOutputPortNameByIndex(int index)
		{
			return "Output" + (index + 1);
		}

		[Serializable]
		public class PanoramaMechanicEventData
		{
			public float mechanicEventTime;
		}

		public List<PanoramaMechanicEventData> panoramaMechanicEventDatas = new();

		public bool ProcessDataUpdate(int targetVersion)
		{
			if (targetVersion <= ver)
			{
				return false;
			}

			switch(targetVersion)
			{
				case 1:
					if (ver >= targetVersion)
					{
						break;
					}
					playMode = loopSectionVideo ? PlayMode.SectionThanLoopLastSeconds : (loopVideo ? PlayMode.LoopSection : PlayMode.Section);
					loopSectionStartTime = videoCutoffTime - loopSectionDuration;
					loopSectionEndTime = videoCutoffTime;
					ver = targetVersion;
					return true;
				default: break;
			}
			return false;
		}
	}

	public static class PlayModeExtensions
	{
		public static SkillsVR.VideoPackage.PlayMode ToVideoPlayMode(this PanoramaVideoNode.PlayMode playMode)
		{
			switch (playMode)
			{
				case PanoramaVideoNode.PlayMode.Section: return SkillsVR.VideoPackage.PlayMode.ABSection;
				case PanoramaVideoNode.PlayMode.LoopSection:
					return SkillsVR.VideoPackage.PlayMode.ABSectionLoop;
				case PanoramaVideoNode.PlayMode.SectionThanLoopLastSeconds:
					return SkillsVR.VideoPackage.PlayMode.ABSectionThenCustomLoopSection;
				default:
					return SkillsVR.VideoPackage.PlayMode.Loop;
			}
		}
#endif
	}
}