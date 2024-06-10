using System;

namespace SkillsVR.Mechanic.MechanicSystems.AudioRecorder
{
	public interface IAudioDataReceiver
	{
		void OnRecordStartSuccess(int samplesPerSec, int frequency, int channelCount);
		void OnException(Exception exception);
		void OnRecordEnd();
		void OnAudioDataRead(float[] buff, int sampleCount, int frequency, int channelCount);
	}
}
