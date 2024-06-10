using SkillsVR.Mechanic.Core;
using SkillsVR.Mechanic.Core.Impl;
using UnityEngine;
using SkillsVR.OdinPlaceholder; 

namespace SkillsVR.Mechanic.MechanicSystems.AudioPlayback
{
	public class AudioPlaybackSystem : AbstractMechanicSystemBehivour<AudioPlaybackData>, IAudioPlaybackSystem
	{
		private bool wasPlaying;
		public override AudioPlaybackData mechanicData
		{
			get
			{
				if (null == base.mechanicData)
				{
					base.mechanicData = new AudioPlaybackData();
				}
				return base.mechanicData;
			}
			set => base.mechanicData = value;
		}

		public void SetAudioClip(AudioClip targetClip)
		{
			mechanicData.audioClip = targetClip;
		}

		public void SetAudioSource(AudioSource source)
		{
			mechanicData.audioSource = source;
		}

		public void StartPlay()
		{
			mechanicData.playbackTime = 0.0f;
			
			
			if (null == mechanicData.audioSource)
			{
				mechanicData.audioSource = this.gameObject.AddComponent<AudioSource>();
			}
			
			var audioSource = mechanicData.audioSource;
			audioSource.Stop();
			audioSource.loop = false;
			audioSource.clip = mechanicData.audioClip;
			if (null != mechanicData.audioClip)
			{
				audioSource.Play();
			}
			
			TriggerEvent(AudioPlaybackEvent.OnPlaybackStart, mechanicData.audioClip);
			TriggerEvent(AudioPlaybackEvent.OnPlaybackStateChanged, true);
			TriggerEvent(AudioPlaybackEvent.OnPlaybackTimeChanged, audioSource.time);

			if (null == mechanicData.audioClip)
			{
				OnStopPlay();
			}
		}

		public void StopPlay()
		{
			bool isPlaying = null == mechanicData.audioSource ? false : mechanicData.audioSource.isPlaying;
			mechanicData.audioSource?.Stop();
			if (isPlaying)
			{
				OnStopPlay();
			}
		}

		public void ControlAudioPlayByMechanic(bool controlAudioPlayByMechanic)
		{
			mechanicData.startPlaybackOnMechanicStart = controlAudioPlayByMechanic;
			mechanicData.stopMechanicOnPlaybackStop = controlAudioPlayByMechanic;
		}

		protected void OnStopPlay()
		{
			TriggerEvent(AudioPlaybackEvent.OnPlaybackEnd, mechanicData.audioClip);
			TriggerEvent(AudioPlaybackEvent.OnPlaybackStateChanged, false);
			UpdatePlaybackTime();
			if (mechanicData.stopMechanicOnPlaybackStop)
			{
				StopMechanic();
			}
		}

		protected override void OnReceiveEvent(IMechanicSystemEvent systemEvent)
		{
			switch (systemEvent.eventKey)
			{
				case MechSysEvent.OnStart: OnMechanicStart(); break;
				case MechSysEvent.OnStop: OnMechanicStop(); break;
			}
		}

		protected void OnMechanicStart()
		{
			if (mechanicData.startPlaybackOnMechanicStart)
			{
				StartPlay();
			}
		}

		protected void OnMechanicStop()
		{
			StopPlay();
		}

		protected override void Update()
		{
			base.Update();
			if (null != mechanicData.audioSource && mechanicData.audioSource.isPlaying)
			{
				UpdatePlaybackTime();
				wasPlaying = true;
			}
			if (wasPlaying && null != mechanicData.audioSource && !mechanicData.audioSource.isPlaying)
			{
				wasPlaying = false;
				OnStopPlay();
			}
		}

		protected void UpdatePlaybackTime()
		{
			mechanicData.playbackTime = null == mechanicData.audioSource ? 0.0f : mechanicData.audioSource.time;
			TriggerEvent(AudioPlaybackEvent.OnPlaybackTimeChanged, mechanicData.playbackTime);
		}
	}
}
