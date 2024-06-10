using UnityEngine;
using SkillsVR.OdinPlaceholder; 

namespace SkillsVR.Mechanic.MechanicSystems.AudioPlayback
{
	[RegisterMechanicEventParameterType(OnPlaybackStart, typeof(AudioClip))]
	[RegisterMechanicEventParameterType(OnPlaybackEnd, typeof(AudioClip))]
	[RegisterMechanicEventParameterType(OnAudioClipChanged, typeof(AudioClip))]
	[RegisterMechanicEventParameterType(OnPlaybackStateChanged, typeof(bool))]
	[RegisterMechanicEventParameterType(OnPlaybackTimeChanged, typeof(float))]
	public enum AudioPlaybackEvent
	{
		None,
		OnPlaybackStart,
		OnPlaybackEnd,
		OnAudioClipChanged,
		OnPlaybackStateChanged,
		OnPlaybackTimeChanged,
	}
}
