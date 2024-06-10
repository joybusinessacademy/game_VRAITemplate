namespace SkillsVR.Mechanic.MechanicSystems.AudioRecorder
{
	public interface IAudioRecorder
	{
		void StartRecord(IAudioDataReceiver receiver);
		void StopRecord(IAudioDataReceiver receiver);
	}
}