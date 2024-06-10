using SkillsVR.Mechanic.Core;
using SkillsVR.Mechanic.MechanicSystems.DeepBreath.Impl;
using System;
using UnityEngine;

namespace SkillsVR.Mechanic.MechanicSystems.DeepBreath
{
    public class SpawnerDeepBreath : AbstractMechanicSpawner<IDeepBreathSystem, DeepBreathData> , IDeepBreathSystem
    {
        public override string mechanicKey => "DeepBreathSystem_SystemOnly";

        public int fullBreathCount => null == targetSystem ? 0 : targetSystem.fullBreathCount;

        public void ActiveBreath(bool active)
        {
            LogIfNotReady(nameof(ActiveBreath));
            targetSystem?.ActiveBreath(active);
        }

        public void StartCheckBreath(float duration = 6, float timeout = -1, bool autoHideAfterSuccess = true, bool autoBreathOut = true)
        {
            LogIfNotReady(nameof(StartCheckBreath));
            targetSystem?.StartCheckBreath(duration, timeout, autoHideAfterSuccess, autoBreathOut);
        }

        public void StopCheckBreath()
        {
            LogIfNotReady(nameof(StopCheckBreath));
            targetSystem?.StopCheckBreath();
        }

        public override void SetActive(bool isActive)
        {
            if (isActive)
            {
                StartMechanic();
            }
            else
            {
               StopMechanic();
            }
        }

        protected override void OnReceiveEvent(IMechanicSystemEvent systemEvent)
        {
        }
    }
}
