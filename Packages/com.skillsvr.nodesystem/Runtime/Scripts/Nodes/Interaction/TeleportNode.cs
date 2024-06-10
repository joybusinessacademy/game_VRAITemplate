using GraphProcessor;
using SkillsVR.Mechanic.Core;
using System;
using Props;
using UnityEngine;

namespace SkillsVRNodes.Scripts.Nodes
{
    [Serializable, NodeMenuItem("Learning/Teleport", typeof(SceneGraph)), NodeMenuItem("Learning/Teleport", typeof(SubGraph))]
    public class TeleportNode : ScriptableSpawnerNode<SpawnerTeleport, ITeleportSystem, TeleportData>
    {
	    public override Color color => NodeColours.Learning;
        public override string name => "Teleport";
		public override string tooltipURL => "https://skillsvr-documentation.vercel.app/nodes/interaction-node-breakdown#teleport-node";
		public override int Width => MEDIUM_WIDTH;

		public enum TeleportTypes
        {
            TeleportImmediately = 0,
            EnableTeleporterAndWait = 1,
            EnableTeleporterAndContinue = 2
        }

		protected override void MechanicListener(IMechanicSystemEvent mechanicSystemEvent)
		{
			switch (mechanicSystemEvent.eventKey)
			{
				case MechSysEvent.AfterFullStop:
					CompleteNode();
					break;
				case TeleportEvent.TeleportContinue:
					CompleteNode();
					break;
				case TeleportEvent.Teleported:

					Transform teleportPosition = PropManager.GetProp(spawnPosition).GetTransform();

					PlayerSpawnPosition.TeleportTo(teleportPosition, SystemInfo.deviceUniqueIdentifier);
					CompleteNode();
					break;
			}

			base.MechanicListener(mechanicSystemEvent);
		}
    }
}
