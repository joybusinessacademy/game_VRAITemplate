using SkillsVR.Mechanic.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillsVR.Mechanic.MechanicSystems.PanoImage
{
	public class SpawnerPanoImage : AbstractMechanicSpawner<IPanoImageSystem, PanoImageData>, IPanoImageSystem
	{
		public override string mechanicKey => "PanoImageSystem";

		protected override void OnReceiveEvent(IMechanicSystemEvent systemEvent)
		{

		}
	}
}
