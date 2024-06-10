using SkillsVR.Mechanic.Core;

namespace SkillsVR.Mechanic.MechanicSystems.PanoImage
{
	[RegisterMechanicEventParameterType(ImageStarted, typeof(int))]
	public enum PanoImageEvent
	{
		None,
		ImageStarted,
		ImageFinished,
	}

	[RegisterMechanicEvent(enumEventType = typeof(PanoImageEvent))]
	public interface IPanoImageSystem : IMechanicSystem<PanoImageData>
	{

	}
}