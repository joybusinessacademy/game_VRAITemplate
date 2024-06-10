using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    public class AnimatorSpeedTrackMixerBehaviour : PlayableBehaviour
    {
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            Animator bindingAnimator = playerData as Animator;
            if (null == bindingAnimator)
            {
                return;
            }

            int inputCount = playable.GetInputCount(); //get the number of all clips on this track


            float finalSpeed = 0.0f;
            bool enableSetSpeed = false;
            for (int i = 0; i < inputCount; i++)
            {
                var inputPlayble = (ScriptPlayable<AnimatorSetSpeedBehaviour>)playable.GetInput(i);
                var setSpeedbehaviour = inputPlayble.GetBehaviour();
                if (null == setSpeedbehaviour)
                {
                    continue;
                }
                
                float inputWeight = playable.GetInputWeight(i);
                bool active = inputWeight > 0.0001f;
                enableSetSpeed = active ? true : enableSetSpeed;
                finalSpeed += setSpeedbehaviour.speed * inputWeight;
            }

            if (enableSetSpeed)
            {
                bindingAnimator.speed = finalSpeed;
            }
        }
    }

}