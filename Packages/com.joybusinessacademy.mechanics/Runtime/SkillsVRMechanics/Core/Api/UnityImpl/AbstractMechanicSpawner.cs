namespace SkillsVR.Mechanic.Core
{
	public class NO_DATA { }

	public abstract class AbstractMechanicSpawner<INTERFACE_TYPE> : AbstractMechanicSpawner<INTERFACE_TYPE, NO_DATA>
		where INTERFACE_TYPE : IMechanicSystem<NO_DATA>
	{
	}
}
