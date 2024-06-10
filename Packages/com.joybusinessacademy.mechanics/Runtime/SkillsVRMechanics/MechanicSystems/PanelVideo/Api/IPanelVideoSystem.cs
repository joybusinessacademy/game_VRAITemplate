using SkillsVR.Mechanic.Core;

namespace SkillsVR.Mechanic.MechanicSystems.PanelVideo
{
	[RegisterMechanicEventParameterType(VideoStarted, typeof(int))]
	public enum PanelVideoEvent
	{
		None,
		VideoStarted,
		VideoFinished,
	}

	[RegisterMechanicEvent(enumEventType = typeof(PanelVideoEvent))]
	public interface IPanelVideoSystem : IMechanicSystem<PanelVideoData>
	{

	}
}