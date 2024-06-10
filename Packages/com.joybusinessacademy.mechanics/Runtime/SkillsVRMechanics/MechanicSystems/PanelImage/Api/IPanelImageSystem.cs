using SkillsVR.Mechanic.Core;

namespace SkillsVR.Mechanic.MechanicSystems.PanelImage
{
	[RegisterMechanicEventParameterType(ImageStarted, typeof(int))]
	public enum PanelImageEvent
	{
		None,
		ImageStarted,
		ImageFinished,
	}

	[RegisterMechanicEvent(enumEventType = typeof(PanelImageEvent))]
	public interface IPanelImageSystem : IMechanicSystem<PanelImageData>
	{

	}
}