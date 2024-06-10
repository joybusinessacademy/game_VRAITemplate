using SkillsVR.Mechanic.Core;

namespace SkillsVR.Mechanic.MechanicSystems.AudioRecorder
{
	public class SpawnerAudioRecorder : AbstractMechanicSpawner<IAudioRecorderSystem, AudioRecorderData>, IAudioRecorderSystem
	{
		public override string mechanicKey => "AudioRecorderSystem";

		public void StartRecord()
		{
			LogIfNotReady(nameof(StartRecord));
			targetSystem?.StartRecord();
		}

		public void StopRecord()
		{
			LogIfNotReady(nameof(StopRecord));
			targetSystem?.StopRecord();
		}

		protected override void OnReceiveEvent(IMechanicSystemEvent systemEvent)
		{
		}


	}
}
