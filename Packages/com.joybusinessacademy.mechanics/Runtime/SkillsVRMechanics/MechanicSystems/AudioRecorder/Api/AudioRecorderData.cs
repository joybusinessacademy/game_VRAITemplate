using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SkillsVR.OdinPlaceholder;
using DialogExporter;
using SkillsVR.Mechanic.MechanicSystems.AudioRecorder.Impl;

namespace SkillsVR.Mechanic.MechanicSystems.AudioRecorder
{

	[Serializable]
	public class AudioRecorderData 
	{
		public string recorderName;

		public string clipName => string.IsNullOrWhiteSpace(recorderName) ? "recorderClip" : recorderName;

		[ReadOnly]
		public AudioClip audioClip;

		public float clipDuration;

		public bool loop;

		public bool exportLoopClip;

		[ReadOnly]
		public bool isRecording;

		[ReadOnly]
		public int currentSamplePos;

		[ShowInInspector, ReadOnly]
		public float currentPos => null == audioClip ? 0 : (float)currentSamplePos / (float)audioClip.samples * audioClip.length;

		public bool startRecordOnMechanicStart = false;
		public bool stopMechanicOnRecordStop = true;

		public string startRecordingInformationText = "Pull the trigger to begin recording your response";
		public string stopRecordingInformationText = "Pull the trigger to stop recording";

		public AudioInputType audioInputType = AudioInputType.TRIGGER;
		public AudioRecorderType audioRecorderType = AudioRecorderType.DEFAULT;

		public LocalisedString keyword = "Keyword";
		public LocalisedString informationForKeyword = "";
		public AudioClip keywordSoundBite;

	}
}
