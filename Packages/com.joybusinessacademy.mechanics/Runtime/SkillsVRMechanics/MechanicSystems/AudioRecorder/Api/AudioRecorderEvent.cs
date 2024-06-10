using SkillsVR.OdinPlaceholder; 
using UnityEngine;

namespace SkillsVR.Mechanic.MechanicSystems.AudioRecorder
{
	[RegisterMechanicEventParameterType(OnRecordStart, typeof(AudioClip))]
	[RegisterMechanicEventParameterType(OnRecordEnd, typeof(AudioClip))]
	[RegisterMechanicEventParameterType(OnAudioClipChanged, typeof(AudioClip))]
	[RegisterMechanicEventParameterType(OnRecordStateChanged, typeof(bool))]
	[RegisterMechanicEventParameterType(OnMaxRecordTimeChanged, typeof(float))]
	[RegisterMechanicEventParameterType(OnCurrentRecordTimeChanged, typeof(float))]
	[RegisterMechanicEventParameterType(OnRemainRecordTimeChanged, typeof(float))]
	[RegisterMechanicEventParameterType(OnFixDurationRecrodingStateChanged, typeof(bool))]
	public enum AudioRecorderEvent
	{
		None,
		OnRecordStart,
		OnRecordEnd,
		OnAudioClipChanged,
		OnRecordStateChanged,
		OnMaxRecordTimeChanged,
		OnCurrentRecordTimeChanged,
		OnRemainRecordTimeChanged,
		OnFixDurationRecrodingStateChanged,
		OnReceiveDataBuff,
	}
}
