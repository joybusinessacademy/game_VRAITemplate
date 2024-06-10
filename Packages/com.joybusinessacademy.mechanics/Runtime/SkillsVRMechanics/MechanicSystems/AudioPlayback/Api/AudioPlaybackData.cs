using System;
using UnityEngine;
using SkillsVR.OdinPlaceholder; 

namespace SkillsVR.Mechanic.MechanicSystems.AudioPlayback
{
	[Serializable]
	public class AudioPlaybackData
	{
		public AudioClip audioClip;
		public AudioSource audioSource;

		[ReadOnly]
		public float playbackTime;

		public bool startPlaybackOnMechanicStart = true;
		public bool stopMechanicOnPlaybackStop = true;
	}
}
