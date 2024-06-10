using CrazyMinnow.SALSA;
using UnityEngine;
using UnityEngine.Playables;

namespace Scripts.Timeline.LookAt
{
	public class LookAtMixerPlayer : PlayableBehaviour
	{
		// for setting up the initial "default Position" of the look target
		bool lookTargetSet;
		private Vector3 currentTargetLook;

		// NOTE: This function is called at runtime and edit time.  Keep that in mind when setting the values of properties.
		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
			Eyes trackBinding = playerData as Eyes;
			if (!trackBinding)
			{
				return;
			}
			
			if (lookTargetSet == false)
			{
				currentTargetLook = trackBinding.lookTarget.position;
				lookTargetSet = true;
			}
			
			Vector3 finalPosition = new();

			int inputCount = playable.GetInputCount();

			float baseWeight = 1f;
			for (int i = 0; i < inputCount; i++)
			{
				float inputWeight = playable.GetInputWeight(i);
				baseWeight -= inputWeight;
				ScriptPlayable<LookAtPlayable> inputPlayable = (ScriptPlayable<LookAtPlayable>)playable.GetInput(i);
				LookAtPlayable input = inputPlayable.GetBehaviour();

				Transform lookAtTransform = input.GetLookAtPoint();
				if (lookAtTransform == null)
				{
					continue;
				}

				// Use the above variables to process each frame of this playable.
				Vector3 offsetPos = lookAtTransform.position;
				finalPosition += offsetPos * inputWeight;
			}
			
			// for changing the current default look at point
			if (baseWeight > 0)
			{
				finalPosition += currentTargetLook * baseWeight;
			}
			else
			{
				currentTargetLook = finalPosition;
			}

			//assign the result to the bound object
			trackBinding.lookTarget.position = finalPosition;
		}
	}
}