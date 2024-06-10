using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;

namespace CrazyMinnow.SALSA.Timeline
{
    [Serializable]
    public class EyesControlBehavior : PlayableBehaviour
    {
        public string clipName = "Eyes";
        public bool manualBlink = false;
        public bool blinkCustomTiming;
        public float blinkON = .1f;
        public float blinkHOLD = .1f;
        public float blinkOFF = .1f;

        public bool enableHead = true;
        public bool enableEyes = true;
        public bool enableBlink = true;
        public bool enableTrack = true;
        [FormerlySerializedAs("filterEnableHead")] public bool excludeEnableHead = false;
        [FormerlySerializedAs("filterEnableEyes")] public bool excludeEnableEyes = false;
        [FormerlySerializedAs("filterEnableBlink")] public bool excludeEnableBlink = false;
        [FormerlySerializedAs("filterEnableTrack")] public bool excludeEnableTrack = false;

        public Transform lookTarget;
        [FormerlySerializedAs("filterLookTarget")] public bool excludeLookTarget = false;

        public bool useAffinity = false;
        [FormerlySerializedAs("filterUseAffinity")] public bool excludeUseAffinity = false;
        public float percentage = 0.75f;
        public float timerMin = 2.0f;
        public float timerMax = 5.0f;

        public bool setHeadRandom = true;
        public bool setEyesRandom = true;
        public bool setBlinkRandom = true;
        [FormerlySerializedAs("filterSetHeadRandom")] public bool excludeSetHeadRandom = false;
        [FormerlySerializedAs("filterSetEyesRandom")] public bool excludeSetEyesRandom = false;
        [FormerlySerializedAs("filterSetBlinkRandom")] public bool excludeSetBlinkRandom = false;

        // reset vars
        private bool origEnableHead;
        private bool origEnableEyes;
        private bool origEnableBlink;
        private bool origEnableTrack;
        private Transform origLookTarget;
        private bool origUseAffinity;
        private float origPercentage;
        private float origTimerMin;
        private float origTimerMax;
        private bool origHeadRand;
        private bool origEyesRand;
        private bool origBlinkRand;

        public Eyes trackBinding;
        public bool isTrackBindingCached = false;
        private bool settingsHaveBeenCaptured = false;
        public bool resetAtClipEnd = false;
        private Playable rootPlayable;
        public bool isDebug = false;


        // cache the rootPlayable for playback speed check.
        public override void OnGraphStart(Playable playable)
        {
            base.OnGraphStart(playable);
            rootPlayable = playable.GetGraph().GetRootPlayable(0);
        }

        // NOTE: runtime and edit time callback.
        public override void ProcessFrame(Playable playableHandle, FrameData info, object playerData)
        {
            var playbackSpeedIsZero =
                Mathf.Approximately((float) rootPlayable.GetSpeed(), 0.0f);
            if (playbackSpeedIsZero)
            {
                ClipStopped(); // effectively paused.
                return;
            }

            if (!isTrackBindingCached)
            {
                trackBinding = playerData as Eyes;
                if (trackBinding == null)
                    return;
                if (isDebug) Debug.Log("trackBinding cached");
                isTrackBindingCached = true;
            }

            if (Application.isPlaying)
                ClipPlaying(playableHandle);
        }

        /// <summary>
        /// Callback from Playable to handle programmatically starting/resuming playable.
        /// </summary>
        public override void OnBehaviourPlay(Playable playableHandle, FrameData info)
        {
            if (isDebug) Debug.Log("onBehaviorPlay");

            if (!Application.isPlaying)
                return;

            if (resetAtClipEnd && !settingsHaveBeenCaptured)
                CaptureOriginalSettings();

            ClipStarted(playableHandle);
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

            // Blink control
            if (manualBlink)
            {
                if (blinkCustomTiming)
                {
                    trackBinding.NewBlink(
                        blinkON,
                        blinkHOLD,
                        blinkOFF);
                }
                else
                    trackBinding.NewBlink();
            }

            // Enable control
            if (!excludeEnableHead && enableHead != trackBinding.headEnabled)
                trackBinding.EnableHead(enableHead);
            if (!excludeEnableEyes && enableEyes != trackBinding.eyeEnabled)
                trackBinding.EnableEye(enableEyes);
            if (!excludeEnableBlink && enableBlink != trackBinding.blinkEnabled)
                trackBinding.EnableEyelidBlink(enableBlink);
            if (!excludeEnableTrack && enableTrack != trackBinding.trackEnabled)
                trackBinding.EnableEyelidTrack(enableTrack);

            // Set Target (Look)
            if (!excludeLookTarget)
                trackBinding.lookTarget = lookTarget;

            // Use Affinity
            if (!excludeUseAffinity && useAffinity != trackBinding.useAffinity)
            {
                trackBinding.affinityPercentage = percentage;
                trackBinding.affinityTimerRange = new Vector2(timerMin, timerMax);
                trackBinding.useAffinity = useAffinity;
            }

            // Set Random
            if (!excludeSetHeadRandom)
                trackBinding.headRandom = setHeadRandom;
            if (!excludeSetEyesRandom)
                trackBinding.eyeRandom = setEyesRandom;
            if (!excludeSetBlinkRandom)
                trackBinding.blinkRandom = setBlinkRandom;
        }

        /// <summary>
        /// Continual processing while a clip is playing.
        /// </summary>
        private void ClipPlaying(Playable playableHandle)
        {
            if (isDebug) Debug.Log("clip is playing");

        }

        /// <summary>
        /// Called on clip programmatic pause/stop and at end-of-clip.
        /// </summary>
        private void ClipStopped()
        {
            if (isDebug) Debug.Log("clip stopped playing");

        }

        private void CaptureOriginalSettings()
        {
            if (isDebug) Debug.Log("settings captured");

            var eyes = trackBinding;
            origEnableHead = eyes.headEnabled;
            origEnableEyes = eyes.eyeEnabled;
            origEnableBlink = eyes.blinkEnabled;
            origEnableTrack = eyes.trackEnabled;
            origLookTarget = eyes.lookTarget;
            origUseAffinity = eyes.useAffinity;
            origPercentage = eyes.affinityPercentage;
            origTimerMin = eyes.affinityTimerRange.x;
            origTimerMax = eyes.affinityTimerRange.y;
            origHeadRand = eyes.headRandom;
            origEyesRand = eyes.eyeRandom;
            origBlinkRand = eyes.blinkRandom;

            settingsHaveBeenCaptured = true;
        }

        private void ResetSettings()
        {
            if (!settingsHaveBeenCaptured)
                return;

            if (isDebug) Debug.Log("settings reset");

            // reset appropriate settings...
            var eyes = trackBinding;
            if (!excludeEnableHead) eyes.headEnabled = origEnableHead;
            if (!excludeEnableEyes) eyes.eyeEnabled = origEnableEyes;
            if (!excludeEnableBlink) eyes.blinkEnabled = origEnableBlink;
            if (!excludeEnableTrack) eyes.trackEnabled = origEnableTrack;
            if (!excludeLookTarget) eyes.lookTarget = origLookTarget;
            if (!excludeUseAffinity) eyes.useAffinity = origUseAffinity;
            eyes.affinityPercentage = origPercentage;
            eyes.affinityTimerRange = new Vector2(origTimerMin, origTimerMax);
            if (!excludeSetHeadRandom) eyes.headRandom = origHeadRand;
            if (!excludeSetEyesRandom) eyes.eyeRandom = origEyesRand;
            if (!excludeSetBlinkRandom) eyes.blinkRandom = origBlinkRand;

            settingsHaveBeenCaptured = false;
        }
   }
}