using SkillsVR.Mechanic.Core;

namespace SkillsVR.Mechanic.MechanicSystems.PanelImage
{
	public class SpawnerPanelImage : AbstractMechanicSpawner<IPanelImageSystem, PanelImageData>, IPanelImageSystem
	{
		public override string mechanicKey => "PanelImageSystem";

		protected override void OnReceiveEvent(IMechanicSystemEvent systemEvent)
		{

		}
	}
}
