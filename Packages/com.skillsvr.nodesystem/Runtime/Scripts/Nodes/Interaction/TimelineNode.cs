using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using Props;
using Props.PropInterfaces;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

namespace SkillsVRNodes.Scripts.Nodes
{
	[Serializable, NodeMenuItem("Character & Props/Timeline", typeof(SceneGraph)), NodeMenuItem("Character & Props/Timeline", typeof(SubGraph))]

	public class TimelineNode : ExecutableNode
	{
		public PropGUID<IPropTimeline> director;
		public TimelineAsset timeline;

		public override string name => "Timeline";
		public override string icon => "Play";
		public override string layoutStyle => "TimelineNode";
		public override string tooltipURL => "https://skillsvr-documentation.vercel.app/nodes/interaction-node-breakdown#timeline-node";
		public override int Width => MEDIUM_WIDTH;

		public override Color color => NodeColours.CharactersAndProps;

		private PlayableDirector sceneDirector;

		private event Action onOneTimeTrackFinishUnMuteTrack;

		private List<MonoBehaviour> coroutineElements = new List<MonoBehaviour>();
		//internal string target;
		
		protected override void OnStart()
		{
            if (director?.GetProp()?. GetDirector?. playableAsset == null)
			{
				Debug.Assert(director.GetProp().GetDirector.playableAsset != null, "Empty timeline, Is this intended?");
				CompleteNode();
				return;
			}

			sceneDirector = director.GetProp().GetDirector;

			if (sceneDirector == null)
			{
				Debug.LogError("No timeline player found by name " + director);
				CompleteNode();
				return;
			}
			
			CheckForSalsaAudio(sceneDirector);

			sceneDirector.playableAsset = director.GetProp().GetDirector.playableAsset;
            UpdateAnimationTracksToSceneOffset();
            sceneDirector.Play(director.GetProp().GetDirector.playableAsset);

			sceneDirector.stopped += OnDirectorStopped;
		}
        public void UpdateAnimationTracksToSceneOffset()
        {
            if (null == director.GetProp().GetDirector.playableAsset)
            {
                return;
            }
            foreach (TrackAsset track in timeline.GetOutputTracks())
            {
                if (!(track is AnimationTrack))
                {
                    continue;
                }
                var animTrack = track as AnimationTrack;
                animTrack.trackOffset = TrackOffset.ApplySceneOffsets;
            }
        }

        protected void CheckForSalsaAudio(PlayableDirector director)
		{


            foreach (TrackAsset track in timeline.GetOutputTracks())
			{
				Object obj = director.GetGenericBinding(track);
				if (obj as AudioSource != null)
				{
					TryMuteAudioTracksForSalsa(track, obj as AudioSource);
				}
			}
		}

		protected void TryMuteAudioTracksForSalsa(TrackAsset track, AudioSource audioSource)
		{
			// Special Handle of salsa audio
			if (null == track || !(track is AudioTrack audioTrack) || null == audioSource)
			{
				return;
			}

			// Handle dialog track by mute and manually play clips.
			// To make sure voice bind to SALSA, and do not play multiple voice at one time.
			// The voice playing under the rule: first in first play. If one voice is playing, all other voice well be cancelled.
			bool isUserMuted = audioTrack.mutedInHierarchy;
			if (isUserMuted)
			{
				return;
			}
			audioTrack.muted = true;
			onOneTimeTrackFinishUnMuteTrack += () => { audioTrack.muted = false; };
			//bindingSceneElement.GetPropComponent().StartCoroutine(AlignAudioSourceToTimeline(audioTrack.GetClips().ToList(), audioSource));
			PropComponent directorComponent = director.GetProp().GetPropComponent();
			
			directorComponent.StartCoroutine(AlignAudioSourceToTimeline(audioTrack.GetClips().ToList(), audioSource));
			coroutineElements.Add(directorComponent);
		}

		
		
		private IEnumerator AlignAudioSourceToTimeline(List<TimelineClip> clips, AudioSource audioSource)
		{
			if (null == clips || null == audioSource || 0 == clips.Count)
			{
				yield break;
			}
			TimelineClip lastClip = null;
			// Wait timeline start
			while (null != sceneDirector && PlayState.Playing != sceneDirector.state)
			{
				yield return new WaitForEndOfFrame();
			}

			// Auto stop on timeline finished, or the coroutine target object (targetObject) is disabled
			while (null != sceneDirector && PlayState.Playing == sceneDirector.state)
			{
				double currentTime = sceneDirector.time;
					
				TimelineClip current = clips.Find(clip => clip.start <= currentTime && clip.end > currentTime);
				if (current != null)
				{
					if (lastClip != current)
					{
						AudioClip audioClip = (current.asset as AudioPlayableAsset)?.clip;
						// The voice playing under the rule: first in first play.
						// If one voice is playing, all other voice well be cancelled.
						if (null != audioClip && null != audioSource && !audioSource.isPlaying)
						{
							audioSource.clip = audioClip;
							audioSource.Play();
						}
					}
					lastClip = current;
				}
				yield return null;
			}
			// Dont put anything here. This part may cut off if the coroutine target object disabled before timeline finish.
			// Use OnDirectorStopped instead.
		}

		private void StopAndCleanAllElementCoroutines()
		{
			CompleteNode();

			foreach (MonoBehaviour element in coroutineElements.Where(element => null != element))
			{
				element.StopCoroutine(nameof(AlignAudioSourceToTimeline));
			}
			coroutineElements.Clear();
		}

		void OnDirectorStopped(PlayableDirector director)
		{
			StopAndCleanAllElementCoroutines();
			CompleteNode();
		}

		protected override void OnComplete()
		{
			onOneTimeTrackFinishUnMuteTrack?.Invoke();
			onOneTimeTrackFinishUnMuteTrack = null;
			base.OnComplete();

			if (sceneDirector != null)
			{
				sceneDirector.stopped -= OnDirectorStopped;
			}
		}
	}
}