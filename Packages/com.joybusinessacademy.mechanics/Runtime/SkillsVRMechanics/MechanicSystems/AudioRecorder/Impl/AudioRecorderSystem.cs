using SkillsVR.Mechanic.Core.Impl;
using SkillsVR.Mechanic.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SkillsVR.Mechanic.MechanicSystems.AudioRecorder;
using UnityEditor;
using SkillsVR.Mechanic.Audio.Visualizator;
using SkillsVR.Mechanic.MechanicSystems.AudioPlayback;

namespace SkillsVR.Mechanic.MechanicSystems.AudioRecorder.Impl
{
	public enum AudioRecorderType
	{
		DEFAULT,
		RECORDWITHPLAYBACK,
		RECORDWITHPLAYBACKANDINFO
	}

	public enum AudioInputType
	{
		TRIGGER,
		BUTTON
	}
	

	public class AudioRecorderSystem : AbstractMechanicSystemBehivour<AudioRecorderData>, IAudioRecorderSystem, IAudioDataReceiver
	{
		//public TextMeshProUGUI startRecordingTMP;
		//public TextMeshProUGUI stopRecordingTMP;

		//Visual
		public GameObject mechanicVisual;

		//Before Recording
		public GameObject beforeRecordingVisual;
		public Button startRecordingButton;

		//During Recording
		public Button stopRecordingButton;
		public GameObject duringRecordingVisual;

		//After Recording
		public Button reRecordButton;
		public Button confirmRecordButton;
		public Button replayRecording;
		public Button stopReplayingRecording;
		public GameObject afterRecordingVisual;
		public GameObject stopRePlayingVisual;

		//Sound Bite Information
		public InformationPopUpSystems informationPop;

		public List<AudioSpectrumExtractor> visualizers = new List<AudioSpectrumExtractor> ();

		private static IAudioRecorder _instance;
		internal static IAudioRecorder audioRecorder
		{
			get
			{
				if (null == _instance)
				{
					if (!Application.isPlaying)
					{
						return null;
					}
					GameObject instanceObject = new GameObject(nameof(DefaultAudioRecorder));
					_instance = instanceObject.AddComponent<DefaultAudioRecorder>();
					if (Application.isPlaying)
						GameObject.DontDestroyOnLoad(instanceObject);
				}
				return _instance;
			}
		}

		public override AudioRecorderData mechanicData
		{
			get
			{
				if (null == base.mechanicData)
				{
					base.mechanicData = new AudioRecorderData() { clipDuration = 5.0f };
				}
				return base.mechanicData;
			}
			set => base.mechanicData = value;
		}

		private bool _isRecording;
		protected bool isRecording
		{
			get => _isRecording;
			set
			{
				if (value == _isRecording && value == mechanicData.isRecording)
				{
					return;
				}
				_isRecording = value;
				mechanicData.isRecording = value;
				TriggerEvent(AudioRecorderEvent.OnRecordStateChanged, value);
			}
		}

		private AudioClip _audioClip;
		protected AudioClip audioClip
		{
			get => _audioClip;
			set
			{
				if (value == _audioClip && value == mechanicData.audioClip)
				{
					return;
				}
				_audioClip = value;
				mechanicData.audioClip = value;
				TriggerEvent(AudioRecorderEvent.OnAudioClipChanged, value);
			}
		}

		private int _currentSamplePos;
		protected int currentSamplePos
		{
			get => _currentSamplePos;
			set
			{
				_currentSamplePos = value;
				mechanicData.currentSamplePos = value;
			}
		}

		protected float clipDuration
		{
			get => mechanicData.clipDuration;
			set => mechanicData.clipDuration = value;
		}

		protected bool loop
		{
			get => mechanicData.loop;
			set => mechanicData.loop = value;
		}

		protected float maxTime => loop ? -1.0f : clipDuration;

		protected float recordingTime => null == mechanicData.audioClip ? 0.0f :
			(0 == mechanicData.audioClip.frequency ? 0.0f : currentSamplePos / (float)mechanicData.audioClip.frequency);
		protected float remainTime => loop ? -1.0f : maxTime - recordingTime;

		internal List<ITelemetry> telemetries = new List<ITelemetry>();
		internal AudioTelemetry audioTelemetry = new AudioTelemetry();

		public void SetAutoRecordWithMechanicActive(bool autoRecord)
		{
			mechanicData.startRecordOnMechanicStart = autoRecord;
			mechanicData.stopMechanicOnRecordStop = autoRecord;
		}

		public void SetLoop(bool isLoop)
		{
			mechanicData.loop = isLoop;
		}

		#region  IAudioRecorderSystem
		public void StartRecord()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			if (isRecording)
			{
				return;
			}
			audioClip = null;
			isRecording = true;
			clipDuration = Mathf.Max(clipDuration, 3.0f);
			currentSamplePos = 0;
			TriggerEvent(AudioRecorderEvent.OnMaxRecordTimeChanged, maxTime);
			TriggerEvent(AudioRecorderEvent.OnRemainRecordTimeChanged, remainTime);
			TriggerEvent(AudioRecorderEvent.OnCurrentRecordTimeChanged, recordingTime);
			TriggerEvent(AudioRecorderEvent.OnFixDurationRecrodingStateChanged, !loop);

			if (mechanicData.loop && mechanicData.exportLoopClip)
			{
				outputLoopBuff = new List<float>();
			}
			audioRecorder.StartRecord(this);

			SetStartRecordVisual();
		}

		public void StopRecord()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			if (!isRecording)
			{
				return;
			}
			isRecording = false;
			TriggerEvent(AudioRecorderEvent.OnMaxRecordTimeChanged, maxTime);
			TriggerEvent(AudioRecorderEvent.OnRemainRecordTimeChanged, remainTime);
			TriggerEvent(AudioRecorderEvent.OnCurrentRecordTimeChanged, recordingTime);
			audioRecorder.StopRecord(this);
			if (mechanicData.loop && mechanicData.exportLoopClip)
			{
				outputLoopBuff = new List<float>();
			}
			if (mechanicData.stopMechanicOnRecordStop)
				StopMechanic();

			SetStopRecordVisual();
		}

		public void OnStartRecordingPressed()
		{
			StartRecord();
		}

		public void SetStartRecordVisual()
		{
			beforeRecordingVisual.SetActive(false);
			afterRecordingVisual.SetActive(false);
			duringRecordingVisual.SetActive(true);
		}

		public void SetStopRecordVisual()
		{
			duringRecordingVisual.SetActive(false);
		}

		public void OnDuringRecordingPressed()
		{
			if(mechanicData.audioRecorderType == AudioRecorderType.DEFAULT || mechanicData.audioInputType == AudioInputType.TRIGGER)
			{
				StopRecord();
				StopMechanic();
			}
			else
			{
				StopRecord();
				afterRecordingVisual.SetActive(true);
			}
		}

		public void OnReplayRecordingPressed()
		{
			replayRecording.gameObject.SetActive(false);
			stopReplayingRecording.gameObject.SetActive(true);
			stopRePlayingVisual.gameObject.SetActive(true);
		}

		public void OnStopReplayRecordingPressed()
		{
			replayRecording.gameObject.SetActive(true);
			stopReplayingRecording.gameObject.SetActive(false);
			stopRePlayingVisual.gameObject.SetActive(false);
		}

		public void OnConfirmRecordPressed()
		{
			StopMechanic();
		}

		public void OnReRecordPressed()
		{
			StartRecord();
		}

		public void SetAudioListeners()
		{
			startRecordingButton.onClick.AddListener(OnStartRecordingPressed);
			stopRecordingButton.onClick.AddListener(OnDuringRecordingPressed);
			reRecordButton.onClick.AddListener(OnReRecordPressed);
			confirmRecordButton.onClick.AddListener(OnConfirmRecordPressed);
			replayRecording.onClick.AddListener(OnReplayRecordingPressed);
			stopReplayingRecording.onClick.AddListener(OnStopReplayRecordingPressed);
		}

		public void RemoveAudioListener()
		{
			startRecordingButton.onClick.RemoveListener(OnStartRecordingPressed);
			stopRecordingButton.onClick.RemoveListener(OnDuringRecordingPressed);
			reRecordButton.onClick.RemoveListener(OnReRecordPressed);
			confirmRecordButton.onClick.RemoveListener(OnConfirmRecordPressed);
			replayRecording.onClick.RemoveListener(OnReplayRecordingPressed);
			stopReplayingRecording.onClick.RemoveListener(OnStopReplayRecordingPressed);
		}

		#endregion IAudioRecorderSystem

		#region  AbstractMechanicSystemBehivour
		protected override void OnReceiveEvent(IMechanicSystemEvent systemEvent)
		{
			switch (systemEvent.eventKey)
			{
				case MechSysEvent.OnStart: OnMechanicStart(); break;
				case MechSysEvent.OnStop: OnMechanicStop(); break;
			}
		}

		protected override void Start()
		{
			base.Start();
			string mechID = "Information Slide: " + this.gameObject.GetInstanceID().ToString();
			audioTelemetry.id = mechID;
			telemetries.Add(audioTelemetry);
		}

		protected override void Update()
		{
			base.Update();
			foreach (var telemetry in telemetries)
			{
				if (!telemetry.isCompleted && telemetry.IsValidated())
					telemetry.SendEvents();
			}
		}

		public override void SetMechanicData()
		{
			base.SetMechanicData();

			mechanicData = base.mechanicData as AudioRecorderData;

			if (mechanicData == null)
			{
				Debug.LogError("Missing Mechanic Data");
				return;
			}

			//if (startRecordingTMP != null)
			//	startRecordingTMP.text = mechanicData.startRecordingInformationText;

			//if(stopRecordingTMP != null)
			//	stopRecordingTMP.text = mechanicData.stopRecordingInformationText;
			mechanicData.startRecordOnMechanicStart = false;

			if(mechanicData.audioRecorderType == AudioRecorderType.RECORDWITHPLAYBACKANDINFO && null != informationPop)
			{
				informationPop.mechanicData.information = mechanicData.keyword;
				informationPop.mechanicData.multiMediaInformation = mechanicData.informationForKeyword;
				informationPop.mechanicData.soundBiteClip = mechanicData.keywordSoundBite;

				informationPop.mechanicData.timeUntilDisappear = 99999;

				informationPop.SetMechanicData();

				informationPop.gameObject.SetActive(true);
			}
				
			if(mechanicData.audioInputType == AudioInputType.BUTTON)
				SetAudioListeners();

			beforeRecordingVisual.SetActive(true);
			duringRecordingVisual.SetActive(false);
			afterRecordingVisual.SetActive(false);

		}

		protected void OnMechanicStart()
		{
			if (mechanicData.startRecordOnMechanicStart)
			{
				StartRecord();
			}
		}

		protected void OnMechanicStop()
		{
			if (mechanicData.audioInputType == AudioInputType.BUTTON)
				RemoveAudioListener();
		}

		public override void SetVisualState(bool show)
		{
			base.SetVisualState(show);

			mechanicVisual?.SetActive(show);
		}

		#endregion  AbstractMechanicSystemBehivour

		#region IAudioDataReceiver
		public void OnAudioDataRead(float[] buff, int sampleCount, int frequency, int channelCount)
		{
			if (loop)
			{
				OnAudioDataReadLoop(buff, sampleCount, frequency, channelCount);
			}
			else
			{
				OnAudioDataReadSingle(buff, sampleCount, frequency, channelCount);
				TriggerEvent(AudioRecorderEvent.OnRemainRecordTimeChanged, remainTime);

            }
            TriggerEvent(AudioRecorderEvent.OnReceiveDataBuff, buff);
            TriggerEvent(AudioRecorderEvent.OnCurrentRecordTimeChanged, recordingTime);

			visualizers.ForEach(x => x?.ProcessDataBuff(buff)); 
		}

        private List<float> outputLoopBuff = new List<float>();

		protected void OnAudioDataReadLoop(float[] buff, int sampleCount, int frequency, int channelCount)
		{
			if (mechanicData.exportLoopClip)
			{
				outputLoopBuff.AddRange(buff);
			}
			audioClip.SetData(buff, currentSamplePos);
			currentSamplePos += sampleCount;
			currentSamplePos %= audioClip.samples;
		}

		protected void OnAudioDataReadSingle(float[] buff, int sampleCount, int frequency, int channelCount)
		{
			if (sampleCount + currentSamplePos <= audioClip.samples)
			{
				audioClip.SetData(buff, currentSamplePos);
				currentSamplePos += sampleCount;
			}
			else
			{
				int overflowCount = sampleCount + currentSamplePos - audioClip.samples;
				if (currentSamplePos < audioClip.samples)
				{
					float[] startData = new float[overflowCount * channelCount];
					audioClip.GetData(startData, 0);
					audioClip.SetData(buff, currentSamplePos);
					audioClip.SetData(startData, 0);
				}
				currentSamplePos = audioClip.samples;
				StopRecord();
			}
		}

		void ExportClip()
		{
			if (!mechanicData.loop || !mechanicData.exportLoopClip)
			{
				return;
			}

			if (null == audioClip || this.outputLoopBuff.Count == 0)
			{
				return;
			}
			var export = AudioClip.Create(audioClip.name, this.outputLoopBuff.Count, audioClip.channels, audioClip.frequency, false);
			export.SetData(outputLoopBuff.ToArray(), 0);
			audioClip = export;
			outputLoopBuff.Clear();
		}

		public void OnRecordEnd()
		{
			isRecording = false;
			ExportClip();
			TrimEndSpaceForNonlLoopClip();
			audioTelemetry.audioClipData = audioClip;
			TriggerEvent(AudioRecorderEvent.OnRecordEnd, audioClip);
			if (mechanicData.stopMechanicOnRecordStop)
				StopMechanic();
		}

		public void OnException(Exception exception)
		{
			Debug.LogException(exception);
			ExportClip();
			TriggerEvent(MechSysEvent.OnException, exception);
			TriggerEvent(AudioRecorderEvent.OnRecordEnd, audioClip);
			audioTelemetry.audioClipData = audioClip;
			if (mechanicData.stopMechanicOnRecordStop)
				StopMechanic();
		}

		public void OnRecordStartSuccess(int samplesPreSec, int frequency, int channelCount)
		{
			audioClip = AudioClip.Create(mechanicData.clipName, Mathf.RoundToInt(samplesPreSec * clipDuration), channelCount, frequency, false);
			currentSamplePos = 0;
			TriggerEvent(AudioRecorderEvent.OnRecordStart, audioClip);
		}
		#endregion IAudioDataReceiver

		public void TrimEndSpaceForNonlLoopClip()
		{
			if (null != audioClip && !loop && currentSamplePos < audioClip.samples)
			{
				var trimEndClip = AudioClip.Create(audioClip.name, currentSamplePos, audioClip.channels, audioClip.frequency, false);
				float[] data = new float[currentSamplePos * audioClip.channels];
				audioClip.GetData(data, 0);
				trimEndClip.SetData(data, 0);
				audioClip = trimEndClip;
			}
		}


#if UNITY_EDITOR
		protected void Replay()
		{
			if (null == audioClip)
			{
				return;
			}
			var audioSource = GetComponent<AudioSource>();
			if (null == audioSource)
			{
				audioSource = this.gameObject.AddComponent<AudioSource>();
			}
			audioSource.Stop();
			audioSource.clip = audioClip;
			audioSource.Play();
		}

		protected void StopReplay()
		{
			var audioSource = GetComponent<AudioSource>();
			audioSource?.Stop();
		}
#endif
	}
}
