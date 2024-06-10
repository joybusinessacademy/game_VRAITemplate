using UnityEngine;

namespace SkillsVRNodes
{
	[RequireComponent(typeof(AudioSource))]
	public class SceneAudio : SceneElement
	{
		private AudioSource audioSource;
        public AudioSource AudioSource { get => audioSource; }

        public override void Reset()
        {
			base.Reset();

			if(audioSource ==null)
				audioSource = GetComponent<AudioSource>();

			audioSource.spatialBlend = 0.8f;
			audioSource.maxDistance = 3f;
		}

        private void Awake()
		{
			audioSource = GetComponent<AudioSource>();
		}

		public void PlayAudio(AudioClip audioClip)
		{
			audioSource.clip = audioClip;
			audioSource.Play();
		}
	}
}