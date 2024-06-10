using UnityEngine;

namespace Props.PropInterfaces
{
	public interface IPropAudioSource : IBaseProp
	{
		public AudioSource GetAudioSource();

		public void PlayAudio(AudioClip audioClip)
		{
			GetAudioSource().clip = audioClip;
			GetAudioSource().Play();
		}
	}
}