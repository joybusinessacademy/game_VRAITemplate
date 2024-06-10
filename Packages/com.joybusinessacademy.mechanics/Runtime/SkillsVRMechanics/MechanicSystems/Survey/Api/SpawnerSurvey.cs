using SkillsVR.Mechanic.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SpawnerSurvey : AbstractMechanicSpawner<ISurveySystem, SurveyData>, ISurveySystem
{
	public override string mechanicKey => "SurveySystem";

	protected override void OnReceiveEvent(IMechanicSystemEvent systemEvent)
	{
	}
}
