using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Linq;
using SkillsVR.TimelineTool.Bindings;

namespace SkillsVR.TimelineTool.AnimatorTimeline
{


    [Serializable]
    public class TimelineDirectorBehaviour : StateMachineBehaviour
    {
        public TimelineAsset timelineAsset;
        public DirectorWrapMode loopMode = DirectorWrapMode.None;

        [SerializeReference]
        [ClassPicker(typeof(AnimatorBindingValueProvider), typeof(PlayableDirectorProvider), includeOriginFieldType = false)]
        public IUnityObjectProvider directorProvider;

        public List<SerializableTrackBinding> trackBindings = new List<SerializableTrackBinding>();


        public void CopyFrom(TimelineDirectorBehaviour source)
        {
            if (null == source)
            {
                return;
            }
            this.timelineAsset = source.timelineAsset;
            this.loopMode = source.loopMode;
            this.directorProvider = null == source.directorProvider ? null : source.directorProvider.Clone() as IUnityObjectProvider;
            trackBindings.Clear();
            foreach(var item in source.trackBindings)
            {
                if (null == item)
                {
                    continue;
                }
                trackBindings.Add(item.Clone() as SerializableTrackBinding);
            }
        }

        Animator thisAnimator;
        float prevSpeed;
        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            thisAnimator = animator;
            PlayableDirector director = null;
            try
            {
                director = GetPlayableDirector(animator);
                ThrowIf(null == director, "Cannot get playable director from provider.");
                TryBindToDirector(director, animator);
                PlayDirector(director);
                PauseState(animator);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                SkipCurrentState(director);
            }
        }

        void PauseState(Animator animator)
        {
            prevSpeed = animator.speed;
            animator.speed = 0.0f;
        }

        void PlayDirector(PlayableDirector director)
        {
            director.stopped += OnDirectorStopped;
            director.Play(timelineAsset, loopMode);
        }


        void LogErrorIf(bool condition, string error)
        {
            if (!condition)
            {
                return;
            }

            Debug.LogError(string.Join("\r\n",
                error,
                string.Join(" ", "Timeline Asset", null == timelineAsset ? "<null>" : timelineAsset.name),
                this.ToString()
                )); ;
        }

        private void SkipCurrentState(PlayableDirector director)
        {
            // skip current anima state;
            Debug.LogError("Skip" + thisAnimator.speed);
            if (null != director)
            {
                director.stopped -= OnDirectorStopped;
            }
        }
        private void OnDirectorStopped(PlayableDirector director)
        {
            if (null != director)
            {
                director.stopped -= OnDirectorStopped;
            }
            thisAnimator.speed = prevSpeed;
        }


        void TryBindToDirector(PlayableDirector director, Animator animator)
        {
            director.playableAsset = timelineAsset;
            ThrowIf(null == director.playableAsset, "No timeline to play");
            var bindings = trackBindings.Where(binding => 
                null != binding 
                    && null != binding.trackAsset
                    && binding.timelineAsset == director.playableAsset);

            foreach(var binding in bindings)
            {
                if (null == binding.valueProvider)
                {
                    Debug.LogError(string.Join("", "Provider cannot be null.", 
                        "\r\nTimeline:", timelineAsset.name,
                        "\r\nTrack:", binding.trackAsset.name));
                    continue;
                }
                var value = binding.valueProvider.GetUnityObject(animator);
                if (null == value)
                {
                    Debug.LogError(string.Join("", "Provider Value is null.",
                        "\r\nTimeline:", timelineAsset.name,
                        "\r\nTrack:", binding.trackAsset.name));
                    continue;
                }

                director.SetGenericBinding(binding.trackAsset, value);
            }
        }

        PlayableDirector GetPlayableDirector(Animator animator)
        {
            if (null == directorProvider)
            {
                return null;
            }
            return directorProvider.GetUnityObject(animator) as PlayableDirector;
        }

        void ThrowIf(bool condition, string error)
        {
            if (!condition)
            {
                return;
            }

            throw new Exception(string.Join("\r\n",
                error,
                string.Join(" ", "Timeline Asset", null == timelineAsset ? "<null>" : timelineAsset.name),
                this.ToString()
                )); ;
        }

        public void AttachTrackBindings()
        {
            if (null == timelineAsset)
            {
                return;
            }
            foreach (var track in timelineAsset.GetOutputTracks())
            {
                var bindingType = track.outputs.FirstOrDefault().outputTargetType;
                if (null == bindingType)
                {
                    continue;
                }

                var exist = trackBindings.Where(b => b.timelineAsset == timelineAsset && b.trackAsset == track).FirstOrDefault();
                if (null != exist)
                {
                    continue;
                }

                SerializableTrackBinding binding = new SerializableTrackBinding();
                binding.timelineAsset = timelineAsset;
                binding.trackAsset = track;
                binding.bindType = bindingType;

                trackBindings.Add(binding);
            }
        }

        public void ClearInactiveBindings()
        {
            if (null == timelineAsset)
            {
                trackBindings.Clear();
                return;
            }

            trackBindings.RemoveAll(b => null == b || null == b.timelineAsset || b.timelineAsset != timelineAsset);
        }

        public void OnValidate()
        {
            AttachTrackBindings();
        }

        /*
        

        

        UnityEngine.Object GetObjectFromBind(SerializableAnimatorTrackBindingAgent bindingData, Animator animator)
        {
            return null;
            //return animator.GetBehaviours<AnimatorBindingBehaviour>().Get(bindingData.animatorBindingKey, bindingData.outputType, animator);
        }

        
        


        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            thisAnimator.speed = originalAnimatorSpeed;
        }

        // OnStateMove is called right after Animator.OnAnimatorMove()
        //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that processes and affects root motion
        //}

        // OnStateIK is called right after Animator.OnAnimatorIK()
        //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that sets up animation IK (inverse kinematics)
        //}

        public override string ToString()
        {
            string txt = "";
            if (null != txt)
            {
                txt += info.ToString("\r\n");
            }


            if (Application.isPlaying)
            {
                txt += string.Join("\r\n",
                    string.Join(" ", "Animator:", thisAnimator.name),
                    string.Join(" ", "Game Object:", thisAnimator.gameObject.GetGameObjectPath()),
                    string.Join(" ", "Scene:", null == thisAnimator.gameObject.scene ? "<null>" : thisAnimator.gameObject.scene.name));
            }

            return txt;
        }
        */
    }
}