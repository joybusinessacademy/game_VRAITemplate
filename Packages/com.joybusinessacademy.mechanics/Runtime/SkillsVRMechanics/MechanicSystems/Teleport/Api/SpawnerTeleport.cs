using SkillsVR.Mechanic.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerTeleport : AbstractMechanicSpawner<ITeleportSystem, TeleportData>, ITeleportSystem
{
	public override string mechanicKey => "TeleportSystem";

	protected override void OnReceiveEvent(IMechanicSystemEvent systemEvent)
	{
	}
}
