using System;
using UnityEngine;
using UnityEngine.Playables;
using Random = UnityEngine.Random;

namespace CrazyMinnow.SALSA.Timeline
{
    [Serializable]
    public class EmoterControlBehavior : PlayableBehaviour
    {
        // We only need to define a single field for this Behavior
        public string emoteName = "";
        public bool isAnimatingOn = true;
        public ExpressionComponent.ExpressionHandler handler = ExpressionComponent.ExpressionHandler.RoundTrip;
        public bool dynamics = false;
        [Range(0.0f, 1.0f)] public float dynamicsAmount = 0.0f;
        [Range(0.0f, 1.0f)] public float dynamicsTiming = .2f;

        public Emoter trackBinding;
        public bool isTrackBindingCached = false;
        private bool settingsHaveBeenCaptured = false;
        public bool resetAtClipEnd = true;
        private Playable rootPlayable;
        public bool isDebug = false;


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
                trackBinding = playerData as Emoter;
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

            // perform actions for clip starting...
            var playDuration = (float) playableHandle.GetDuration();

            trackBinding.ManualEmote(emoteName, handler, playDuration, isAnimatingOn);
        }

        /// <summary>
        /// Continual processing while a clip is playing.
        /// </summary>
        private void ClipPlaying(Playable playableHandle)
        {
            if (isDebug) Debug.Log("clip is playing");

            // perform actions for clip playing...

        }

        /// <summary>
        /// Called on clip programmatic pause/stop and at end-of-clip.
        /// </summary>
        private void ClipStopped()
        {
            if (isDebug) Debug.Log("clip stopped playing");

            // perform actions when clip stops...

        }

        private void CaptureOriginalSettings()
        {
            if (isDebug) Debug.Log("settings captured");

            // store appropriate settings...

            settingsHaveBeenCaptured = true;
        }

        private void ResetSettings()
        {
            if (!settingsHaveBeenCaptured)
                return;

            if (isDebug) Debug.Log("settings reset");

            // reset appropriate settings...
        }
    }
}