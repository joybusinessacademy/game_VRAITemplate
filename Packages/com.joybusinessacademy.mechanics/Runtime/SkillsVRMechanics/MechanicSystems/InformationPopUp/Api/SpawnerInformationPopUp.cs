using SkillsVR.Mechanic.Core;

public class SpawnerInformationPopUp : AbstractMechanicSpawner<IInformationPopUpSystem, InfoPopUpDatas>, IInformationPopUpSystem
{
	public override string mechanicKey => "InformationPopUpSystem";

	protected override void OnReceiveEvent(IMechanicSystemEvent systemEvent)
	{

	}
}
