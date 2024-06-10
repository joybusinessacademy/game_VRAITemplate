using SkillsVR.Mechanic.Core;
using System.Collections.Generic;
using UnityEngine;
using VRMechanics.Mechanics.GestureDetection;

namespace SkillsVR.Mechanic.MechanicSystems.Gesture
{
    public class SpawnerGestureDetection : AbstractMechanicSpawner<IGestureSystem, GestureDetectionData>, IGestureSystem
    {
        public override string mechanicKey => "GestureDetectionMechanic";

        protected override void OnReceiveEvent(IMechanicSystemEvent systemEvent)
        {
            if (typeof(MechSysSpawnStateEvent) == systemEvent.eventKey.GetType())
            {
                Debug.Log(systemEvent);
            }
            switch (systemEvent.eventKey)
            {
                case DetectionEvents.BeforeDetect: break;
                case DetectionEvents.OnDetect: break;
                case DetectionEvents.StateChanged:
                case DetectionEvents.StartDetect:
                case DetectionEvents.StopDetect:
                    Debug.Log(systemEvent); break;
                default:
                    break;
            }
        }

        public override void SetActive(bool isActive)
        {
            base.SetActive(isActive);
        }
        public override void StartMechanic()
        {
            if (null != mechanicData && null == mechanicData.gestureTemplateAsset)
            {
                LoadAsset<TextAsset>(mechanicData.gestureTemplateAssetKey, (result) => {
                    if (null != result)
                    {
                        mechanicData.gestureTemplateAsset = result;
                    }
                    base.StartMechanic();
                });
                return;
            }
            else
            {
                base.StartMechanic();
            }
        }

        public IEnumerable<GestureDetectStepData> GetAllDetectStepData()
        {
            LogIfNotReady(nameof(GetAllDetectStepData));
            return null == targetSystem ? null : targetSystem?.GetAllDetectStepData();
        }

        public IGestureBody GetRuntimePose()
        {
            return null == targetSystem ? null : targetSystem.GetRuntimePose();
        }

        public IGestureBody GetTemplate()
        {
            return null == targetSystem ? null : targetSystem.GetTemplate();
        }
    }
}
