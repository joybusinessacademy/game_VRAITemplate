using System;
using System.Collections.Generic;
using UnityEngine;
using SkillsVR.OdinPlaceholder; 
using UnityEngine.Events;
using System.Linq;

namespace SkillsVR.Mechanic.MechanicSystems.AudioRecorder.Impl
{
	internal class DefaultAudioRecorder : MonoBehaviour, IAudioRecorder
	{
		public const int DEFAULT_RECORDING_FREQUENCY = 44100;

		[ShowInInspector, ReadOnly]
		protected string deviceName { get; set; } = null;

		[ShowInInspector, ReadOnly]
		protected bool isRecording => Microphone.IsRecording(deviceName);

		[ShowInInspector, ReadOnly]
		protected AudioClip audioDataCache;

		[ShowInInspector, ReadOnly]
		private int micPrevPos;

		[ShowInInspector, ReadOnly]
		protected HashSet<IAudioDataReceiver> managedDataReceivers = new HashSet<IAudioDataReceiver>();

		protected HashSet<IAudioDataReceiver> receiversToBeRemoved = new HashSet<IAudioDataReceiver>();
		protected HashSet<IAudioDataReceiver> receiversToBeAdded = new HashSet<IAudioDataReceiver>();
		protected bool isProcessingReceiverData;

		public void StartRecord(IAudioDataReceiver receiver)
		{
			if (null == receiver)
			{
				return;
			}
			if (isProcessingReceiverData)
			{
				if (!receiversToBeAdded.Contains(receiver))
				{
					receiversToBeAdded.Add(receiver);
				}
				return;
			}
			if (managedDataReceivers.Contains(receiver))
			{
				return;
			}
			managedDataReceivers.Add(receiver);
			try
			{
				StartMicrophone();
			}
			catch (Exception exc)
			{
				receiver.OnException(exc);
				return;
			}
			receiver.OnRecordStartSuccess(audioDataCache.samples, audioDataCache.frequency, audioDataCache.channels);

		}

		public void StopRecord(IAudioDataReceiver receiver)
		{
			if (null == receiver)
			{
				return;
			}
			if (isProcessingReceiverData)
			{
				if (!receiversToBeRemoved.Contains(receiver))
				{
					receiversToBeRemoved.Add(receiver);
				}
				return;
			}
			if (!managedDataReceivers.Contains(receiver))
			{
				return;
			}
			if (!managedDataReceivers.Remove(receiver))
			{
				return;
			}
			if (managedDataReceivers.Count == 0)
			{
				StopMicrophone();
			}
			receiver.OnRecordEnd();

		}

		public void StopAll()
		{
			foreach (var receiver in managedDataReceivers)
			{
				receiver.OnRecordEnd();
			}
			managedDataReceivers.Clear();
			StopMicrophone();
		}

		protected void OnDestroy()
		{
			StopAll();
		}

		protected void Update()
		{
			if (null == audioDataCache || !isRecording)
			{
				return;
			}
			float[] data = null;
			int micPos = Microphone.GetPosition(deviceName);
			int samplesToRead = 0;
			if (ReadDiffDataFromClip(out data, out samplesToRead, audioDataCache, micPrevPos, micPos) && null != data)
			{
				isProcessingReceiverData = true;
				foreach (var receiver in managedDataReceivers)
				{
					receiver.OnAudioDataRead(data, samplesToRead, audioDataCache.frequency, audioDataCache.channels);
				}
				isProcessingReceiverData = false;
			}

			foreach (var item in receiversToBeAdded)
			{
				StartRecord(item);
			}
			foreach (var item in receiversToBeRemoved)
			{
				StopRecord(item);
			}
			receiversToBeAdded.Clear();
			receiversToBeRemoved.Clear();
			micPrevPos = micPos;
		}

		[Button]
		protected void StartMicrophone()
		{
			if (isRecording && null != audioDataCache)
			{
				return;
			}

			if (null == Microphone.devices || 0 == Microphone.devices.Length)
			{
				throw new Exception("Start Microphone Fail: No microphone device found.");
			}

			int maxFreq = 0;
			int minFreq = 0;
			Microphone.GetDeviceCaps(deviceName, out minFreq, out maxFreq);
			int recordFreq = 0 == maxFreq ? DEFAULT_RECORDING_FREQUENCY : Mathf.Min(DEFAULT_RECORDING_FREQUENCY, maxFreq);
			audioDataCache = Microphone.Start(deviceName, true, 1, recordFreq);
			micPrevPos = 0;
#if UNITY_EDITOR
			Debug.Log(string.Join(" ", "Start Micphone", nameof(DefaultAudioRecorder), "\r\nAudio Clip",
				"Freq:", audioDataCache.frequency,
				"Duration:", audioDataCache.length,
				"channels", audioDataCache.channels));
#endif
			if (null == audioDataCache)
			{
				throw new Exception("Microphone Not Start by Unknown Reason.");
			}
		}

		[Button]
		protected void StopMicrophone()
		{
			if (isRecording)
			{
				Microphone.End(deviceName);
#if UNITY_EDITOR
				Debug.Log(string.Join(" ", "End Micphone", nameof(DefaultAudioRecorder)));
#endif
			}
		}

		protected bool ReadDiffDataFromClip(out float[] data, out int samplesToRead, AudioClip audioClip, int startSamplePos, int endSamplePos)
		{
			data = null;
			samplesToRead = 0;
			if (null == audioClip)
			{
				return false;
			}
			if (startSamplePos < 0 || endSamplePos < 0)
			{
				return false;
			}
			samplesToRead = startSamplePos > endSamplePos ? audioDataCache.samples - startSamplePos + endSamplePos : endSamplePos - startSamplePos;

			int bufferLength = audioDataCache.channels * samplesToRead;
			if (0 == bufferLength)
			{
				return false;
			}

			data = new float[bufferLength];
			bool success = audioDataCache.GetData(data, startSamplePos);
			return success;
		}

	}
}