using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace UnityEngine.Timeline
{
    // The runtime instance of a the TextTrack. It is responsible for blending and setting the final data
    // on the Text binding
    public class LegacyTextTrackMixerBehaviour : PlayableBehaviour
    {
        Color m_DefaultColor;
        string m_DefaultText;

        Text m_TrackBinding;

        // Called every frame that the timeline is evaluated. ProcessFrame is invoked after its' inputs.
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            SetDefaults(playerData as Text);
            if (m_TrackBinding == null)
                return;

            int inputCount = playable.GetInputCount();

            Color blendedColor = Color.clear;
            float totalWeight = 0f;
            float greatestWeight = 0f;
            string text = m_DefaultText;

            for (int i = 0; i < inputCount; i++)
            {
                float inputWeight = playable.GetInputWeight(i);
                ScriptPlayable<LegacyTextPlayableBehaviour> inputPlayable = (ScriptPlayable<LegacyTextPlayableBehaviour>)playable.GetInput(i);
                LegacyTextPlayableBehaviour input = inputPlayable.GetBehaviour();

                blendedColor += input.color * inputWeight;
                totalWeight += inputWeight;

                // use the text with the highest weight
                if (inputWeight > greatestWeight)
                {
                    text = input.text;
                    greatestWeight = inputWeight;
                }
            }

            // blend to the default values
            m_TrackBinding.color = Color.Lerp(m_DefaultColor, blendedColor, totalWeight);
            m_TrackBinding.text = text;
        }

        // Invoked when the playable graph is destroyed, typically when PlayableDirector.Stop is called or the timeline
        // is complete.
        public override void OnPlayableDestroy(Playable playable)
        {
            RestoreDefaults();
        }

        void SetDefaults(Text text)
        {
            if (text == m_TrackBinding)
                return;

            RestoreDefaults();

            m_TrackBinding = text;
            if (m_TrackBinding != null)
            {
                m_DefaultColor = m_TrackBinding.color;
                m_DefaultText = m_TrackBinding.text;
            }
        }

        void RestoreDefaults()
        {
            if (m_TrackBinding == null)
                return;

            m_TrackBinding.color = m_DefaultColor;
            m_TrackBinding.text = m_DefaultText;
        }
    }
}
