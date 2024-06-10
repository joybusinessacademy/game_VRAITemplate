using SkillsVR.Mechanic.Core;

namespace SkillsVR.Mechanic.MechanicSystems.PanelVideo
{
	public class SpawnerPanelVideo : AbstractMechanicSpawner<IPanelVideoSystem, PanelVideoData>, IPanelVideoSystem
	{
		public override string mechanicKey => "PanelVideoSystem";

		protected override void OnReceiveEvent(IMechanicSystemEvent systemEvent)
		{

		}
	}
}
