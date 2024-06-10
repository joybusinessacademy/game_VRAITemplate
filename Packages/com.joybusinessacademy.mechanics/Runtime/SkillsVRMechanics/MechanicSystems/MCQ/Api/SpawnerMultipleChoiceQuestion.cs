using SkillsVR.Mechanic.Core;

public class SpawnerMultipleChoiceQuestion : AbstractMechanicSpawner<IMultipleChoiceQuestionSystem, MultipleChoiceQuestionScriptable>, IMultipleChoiceQuestionSystem
{
	public override string mechanicKey => "MultipleChoiceQuestionSystem";

	protected override void OnReceiveEvent(IMechanicSystemEvent systemEvent)
	{
	}
}
