using System;
using UnityEngine;
using UnityEngine.Playables;

namespace CrazyMinnow.SALSA.Timeline
{
    [Serializable]
    public class SalsaControlBehavior : PlayableBehaviour
    {
        public AudioClip audioClip;
        public bool overrideDynamics = false;
        [Range(0.0f,1.0f)] public float advDynamicsBias = 0.5f;
        [Range(0.0f,1.0f)] public float globalDynamics = 1.0f;

        private float origAdvDynamicsBias = 0.5f;
        private float origGlobalDynamics = 1.0f;

        public Salsa trackBinding;
        public bool isTrackBindingCached = false;
        private bool settingsHaveBeenCaptured = false;
        public bool resetAtClipEnd = true;
        private Playable rootPlayable;
        public bool isDebug = false;


        // fields specific to PlayableAsset type.
        private float scrubbingThreshold = .05f; // amount of variance to detect a scrub change.


        // cache the rootPlayable for playback speed check.
        public override void OnGraphStart(Playable playable)
        {
            base.OnGraphStart(playable);
            rootPlayable = playable.GetGraph().GetRootPlayable(0);
        }

        public override void ProcessFrame(Playable playableHandle, FrameData info, object playerData)
        {
            if (!Application.isPlaying)
                return;

            var isPlaybackSpeedZero = Mathf.Approximately((float) rootPlayable.GetSpeed(), 0.0f);
            if (isPlaybackSpeedZero)
            {
                ClipStopped(); // effectively paused.
                return;
            }

            if (!isTrackBindingCached)
            {
                trackBinding = playerData as Salsa;
                if (trackBinding == null)
                    return;
                if (isDebug) Debug.Log("trackBinding cached");
                isTrackBindingCached = true;
            }

            ClipPlaying(playableHandle);
        }

        /// <summary>
        /// Callback from Playable to handle programmatically starting/resuming playable.
        /// </summary>
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (isDebug) Debug.Log("onBehaviorPlay");

            if (!Application.isPlaying)
                return;

            if (resetAtClipEnd && !settingsHaveBeenCaptured)
                CaptureOriginalSettings();

            ClipStarted(playable);
        }

        /// <summary>
        /// Callback from Playable to handle programmatically pausing/stopping playable.
        /// </summary>
        public override void OnBehaviourPause(Playable playableHandle, FrameData info)
        {
            if (isDebug) Debug.Log("onBehaviorPause");

            if (!Application.isPlaying)
                return;

            ClipStopped();


            if (resetAtClipEnd && settingsHaveBeenCaptured)
                ResetSettings();
        }

        /// <summary>
        /// One time process at the start of a new clip.
        /// </summary>
        private void ClipStarted(Playable playableHandle)
        {
            if (isDebug) Debug.Log("new clip started");

            trackBinding.audioSrc.Stop();
            trackBinding.audioSrc.clip = audioClip;
            if (audioClip == null)
                return; // no audio clip, nothing to do.

            PlayAudio(playableHandle);
        }

        /// <summary>
        /// Continual processing while a clip is playing.
        /// </summary>
        private void ClipPlaying(Playable playableHandle)
        {
            if (isDebug) Debug.Log("clip is playing");

            if (IsScrubbingActiveClip(playableHandle))
                trackBinding.audioSrc.time = (float) playableHandle.GetTime();

            if (!trackBinding.audioSrc.isPlaying)
                PlayAudio(playableHandle);
        }

        /// <summary>
        /// Called on clip programmatic pause/stop and at end-of-clip.
        /// </summary>
        private void ClipStopped()
        {
            if (isDebug) Debug.Log("clip stopped playing");

            PauseAudio();
        }

        private void CaptureOriginalSettings()
        {
            if (isDebug) Debug.Log("settings captured");

            origAdvDynamicsBias = trackBinding.advDynPrimaryBias;
            origGlobalDynamics = trackBinding.globalFrac;

            settingsHaveBeenCaptured = true;
        }

        private void ResetSettings()
        {
            if (!settingsHaveBeenCaptured)
                return;

            if (isDebug) Debug.Log("settings reset");

            // reset appropriate settings...
            trackBinding.advDynPrimaryBias = origAdvDynamicsBias;
            trackBinding.globalFrac = origGlobalDynamics;
        }

        private bool IsScrubbingActiveClip(Playable playableHandle)
        {
            var timeDiff = (float) playableHandle.GetTime() - trackBinding.audioSrc.time;
            timeDiff = timeDiff < 0f ? timeDiff * -1 : timeDiff;
            if (timeDiff > scrubbingThreshold)
                return true;

            return false;
        }

        private void PlayAudio(Playable playableHandle)
        {
            if (playableHandle.IsValid() && trackBinding != null && trackBinding.audioSrc.clip != null)
            {
                trackBinding.globalFrac = globalDynamics;
                trackBinding.advDynPrimaryBias = advDynamicsBias;
                trackBinding.audioSrc.time = (float) playableHandle.GetTime();
                trackBinding.audioSrc.Play();
            }
        }

        private void PauseAudio()
        {
            if (trackBinding != null && trackBinding.audioSrc != null)
                trackBinding.audioSrc.Pause();
        }
    }
}