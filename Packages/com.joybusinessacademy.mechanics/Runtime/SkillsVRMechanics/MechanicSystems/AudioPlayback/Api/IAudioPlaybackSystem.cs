using SkillsVR.Mechanic.Core;
using UnityEngine;
using SkillsVR.OdinPlaceholder; 

namespace SkillsVR.Mechanic.MechanicSystems.AudioPlayback
{
	[RegisterMechanicEvent(enumEventType = typeof(AudioPlaybackEvent))]
	public interface IAudioPlaybackSystem : IMechanicSystem<AudioPlaybackData>
	{
		void StartPlay();
		void StopPlay();

		void SetAudioClip(AudioClip clip);
		void SetAudioSource(AudioSource source);
		void ControlAudioPlayByMechanic(bool controlAudioPlayByMechanic);
	}
}
