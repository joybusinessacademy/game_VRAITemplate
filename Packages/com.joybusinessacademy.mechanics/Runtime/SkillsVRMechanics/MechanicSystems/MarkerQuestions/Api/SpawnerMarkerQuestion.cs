using SkillsVR.Mechanic.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerMarkerQuestion : AbstractMechanicSpawner<IMarkerQuestionSystem, MarkerQuestionsData>, IMarkerQuestionSystem
{
	public override string mechanicKey => "MarkerQuestionSystem";


	protected override void OnReceiveEvent(IMechanicSystemEvent systemEvent)
	{
		
	}
}
