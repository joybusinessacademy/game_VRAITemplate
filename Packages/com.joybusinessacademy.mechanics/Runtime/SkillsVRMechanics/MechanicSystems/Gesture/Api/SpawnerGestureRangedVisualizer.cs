using SkillsVR.Mechanic.Core;
using UnityEngine;

namespace SkillsVR.Mechanic.MechanicSystems.Gesture
{
	public class SpawnerGestureRangedVisualizer : Core.AbstractMechanicSpawner<IGestureVisualizer>, IGestureVisualizer
    {
        public override string mechanicKey => "GestureVisualizerRanged";

        public SpawnerGestureDetection targetGesture
        {
            get => targetGestureSystem;
            set => targetGestureSystem = value;
        }

        [SerializeField]
        protected SpawnerGestureDetection targetGestureSystem;


        protected override void OnReceiveEvent(IMechanicSystemEvent systemEvent)
        {
            switch(systemEvent.eventKey)
            {
                case MechSysSpawnStateEvent.Ready: OnReady(); break;
            }
        }

        protected void OnReady()
        {
            if (null == targetSystem)
            {
                return;
            }
            targetSystem.targetGesture = this.targetGesture;
            targetSystem.StartMechanic();
        }
    }
}
