using SkillsVR.Mechanic.Core;
using SkillsVR.OdinPlaceholder; 

namespace SkillsVR.Mechanic.MechanicSystems.AudioRecorder
{
	[RegisterMechanicEvent(enumEventType =typeof(AudioRecorderEvent))]
	public interface IAudioRecorderSystem : IMechanicSystem<AudioRecorderData>
	{
		void StartRecord();
		void StopRecord();
	}
}
