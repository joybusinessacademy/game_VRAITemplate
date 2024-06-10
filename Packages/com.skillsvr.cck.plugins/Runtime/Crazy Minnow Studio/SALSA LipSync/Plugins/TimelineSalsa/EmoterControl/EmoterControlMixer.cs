using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Random = UnityEngine.Random;

namespace CrazyMinnow.SALSA.Timeline
{
    public class EmoterControlMixer : PlayableBehaviour
    {
        private float timecheck;
        private List<EmoterControlBehavior> behaviors = new List<EmoterControlBehavior>();
        private const float NO_DURATION = 0.0f;

        // NOTE: This function is called at runtime and edit time.  Keep that in mind when setting the values of properties.
        public override void ProcessFrame(Playable playableHandle, FrameData info, object playerData)
        {
            if (!Application.isPlaying)
                return;

            int count = playableHandle.GetInputCount(); // how many clips on the track.
            // let's loop through the clips on the track to find the one currently under the playhead.
            for (int i = 0; i < count; i++)
            {
                Playable inputHandle = playableHandle.GetInput(i);

                if (inputHandle.IsValid() && inputHandle.GetPlayState() == PlayState.Playing)
                {
                    EmoterControlBehavior inputData = ((ScriptPlayable<EmoterControlBehavior>) inputHandle).GetBehaviour();
                    if (inputData != null)
                    {
                        if (inputData.handler == ExpressionComponent.ExpressionHandler.RoundTrip)
                        {
                            return; // no further action necessary...
                        }

                        // keep track of one-way behaviors with dynamics enabled...
                        if (inputData.isAnimatingOn && inputData.dynamics && !behaviors.Contains(inputData))
                        {
                            behaviors.Add(inputData);
                        }

                        // remove one-way behaviors that have an off trigger...
                        if (!inputData.isAnimatingOn)
                        {
                            for (int j = 0; j < behaviors.Count; j++)
                            {
                                if (inputData.emoteName == behaviors[j].emoteName)
                                    behaviors.Remove(behaviors[j]);
                            }
                        }
                    }
                    break; // since we've found and processed the correct clip...bail the loop.
                }
            }

            // animate behaviors that are playing one-way with dynamics...
            for (int i = 0; i < behaviors.Count; i++)
            {
                if (behaviors[i].trackBinding == null)
                {
                    behaviors.Remove(behaviors[i]);
                    continue;
                }

                bool computeAndApplyDynamics = behaviors[i].dynamics && Time.time - timecheck > behaviors[i].dynamicsTiming;
                if (computeAndApplyDynamics)
                {
                    timecheck = Time.time;
                    float dynamicsMin = 0.0f + (1.0f - behaviors[i].dynamicsAmount);
                    float frac = Random.Range(dynamicsMin, 1.0f);
                    behaviors[i].trackBinding.ManualEmote(behaviors[i].emoteName, behaviors[i].handler, NO_DURATION, behaviors[i].isAnimatingOn, frac);
                }
            }
        }
    }
}