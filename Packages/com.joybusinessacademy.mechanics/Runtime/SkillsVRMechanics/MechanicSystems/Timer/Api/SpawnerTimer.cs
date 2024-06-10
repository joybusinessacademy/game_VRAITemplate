using SkillsVR.Mechanic.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SpawnerTimer : AbstractMechanicSpawner<ITimerSystem, TimerData>, ITimerSystem
{
	public override string mechanicKey => "TimerSystem";

	protected override void OnReceiveEvent(IMechanicSystemEvent systemEvent)
	{
	}
}