using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace SkillsVRNodes.Audio
{
	public static class AudioRecorder
	{
		static int frequency = 44100;
		static AudioClip audioClipCache;

		private static string recordingDevice;

		public static string RecordingDevice
		{
			get => recordingDevice;
			set
			{
				if (!IsRecording)
				{
					recordingDevice = value;
				}
			}
		}
		
		public static bool IsRecording => Microphone.IsRecording(RecordingDevice);

		public static AudioClip Record(string folder = "", string fileName = "New Audio")
		{
			if (IsRecording)
			{
				return StopAndSaveRecording(folder, fileName);
			}
			
			audioClipCache = Microphone.Start(null, false, 20, frequency);
			return null;
		}
		
		public static List<string> GetDevices()
		{
			return Microphone.devices.ToList();
		}
		
		public static string DefaultDevice()
		{
			if (null == Microphone.devices || 0 == Microphone.devices.Length)
			{
				return "No Microphone Found";
			}
			return Microphone.devices[0];
		}

		static AudioClip StopAndSaveRecording(string folder = "", string fileName = "New Audio")
		{
			Microphone.End(RecordingDevice);

			audioClipCache = audioClipCache.TrimEnd();

			if (folder == "")
			{
				folder = EditorUtility.OpenFilePanel("Select Folder", "", "wav");
				if (folder == "")
				{
					return null;
				}
			}
			string file = folder + fileName + ".wav";
			
			Directory.CreateDirectory(folder);

			SavWav.Save(file, audioClipCache);

			AssetDatabase.Refresh();

			return AssetDatabase.LoadAssetAtPath<AudioClip>(file.Replace(Application.dataPath, "Assets"));
		}

		/// <summary>
		/// Removed all the empty space at the end of the audio clip
		/// </summary>
		/// <param name="audioClip">Audio Clip to trim</param>
		/// <returns>Trimmed Audio Clip</returns>
		private static AudioClip TrimEnd(this AudioClip audioClip)
		{
			float[] samples = new float[audioClip.samples * audioClip.channels];
			audioClip.GetData(samples, 0);

			int last = samples.ToList().FindLastIndex(t => t != 0);
			List<float> newSamples = samples.ToList().GetRange(0, last);


			audioClip = AudioClip.Create("temp audio file", newSamples.Count, 1, frequency, false);
			audioClip.SetData(newSamples.ToArray(), 0);

			return audioClip;
		}
	}
}