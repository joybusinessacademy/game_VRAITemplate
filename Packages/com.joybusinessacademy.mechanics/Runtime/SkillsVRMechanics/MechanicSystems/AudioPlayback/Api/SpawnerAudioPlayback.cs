using SkillsVR.Mechanic.Core;
using UnityEngine;
using SkillsVR.OdinPlaceholder; 

namespace SkillsVR.Mechanic.MechanicSystems.AudioPlayback
{
	public class SpawnerAudioPlayback : AbstractMechanicSpawner<IAudioPlaybackSystem, AudioPlaybackData>, IAudioPlaybackSystem
	{
		public override string mechanicKey => "AudioPlaybackSystem_SystemOnly";

		public void ControlAudioPlayByMechanic(bool controlAudioPlayByMechanic)
		{
			LogIfNotReady(nameof(ControlAudioPlayByMechanic));
			targetSystem?.ControlAudioPlayByMechanic(controlAudioPlayByMechanic);
		}

		public void SetAudioClip(AudioClip clip)
		{
			LogIfNotReady(nameof(SetAudioClip));
			targetSystem?.SetAudioClip(clip);
		}

		public void SetAudioSource(AudioSource source)
		{
			LogIfNotReady(nameof(SetAudioSource));
			targetSystem?.SetAudioSource(source);
		}

		public void StartPlay()
		{
			LogIfNotReady(nameof(StartPlay));
			targetSystem?.StartPlay();
		}

		public void StopPlay()
		{
			LogIfNotReady(nameof(StopPlay));
			targetSystem?.StopPlay();
		}

		protected override void OnReceiveEvent(IMechanicSystemEvent systemEvent)
		{
		}
	}
}
