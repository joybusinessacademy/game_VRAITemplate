using UnityEngine;
using SkillsVR.Mechanic.MechanicSystems.Gesture;
using VRMechanics.Mechanics.GestureDetection;
using SkillsVR.Mechanic.Core;
using System.Collections.Generic;
using VRMechanics.Mechanics.GestureDetection.StepDetectors;
using SkillsVR.UnityExtenstion;

namespace SkillsVR.Mechanic.MechanicSystems.DeepBreath
{
	public class SpawnerGestureControlledDeepBreath : SpawnerDeepBreath
    {
        public GestureDetectionData gestureTriggerData;
        public SpawnerGestureDetection gestureSystem;

        protected List<ITelemetry> telemetries = new List<ITelemetry>();
        protected TimeElpasedTelemetry firstLookGestureDetectionAreaTimeElpasedTelemetry;
        protected bool trackFirstLook = false;

        public override void Reset()
        {
            base.Reset();
            SetupGestureControl();
        }

        protected override void Awake()
        {
            SetupTelemetry();
            SetupGestureControl();
            base.Awake();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (null != gestureSystem)
            {
                gestureSystem.RemoveListener(OnGestureEvent);
            }
        }

        protected virtual void Update()
        {
            if (null == targetSystem)
            {
                return;
            }
            if (null != gestureSystem)
            {
                TrackFirstLook();
            }
            
            UpdateTelemetry();
        }


        protected void TrackFirstLook()
        {
            if (!trackFirstLook)
            {
                return;
            }
            if (null == gestureSystem || !gestureSystem.active)
            {
                
                return;
            }

            var runtimePose = gestureSystem.GetRuntimePose();
            if (null == runtimePose)
            {
                return;
            }

            var head = runtimePose.GetRawBodyPart(nameof(GestureBodyPartType.Head));
            if (null == head)
            {
                return;
            }

            var dataList = gestureSystem.GetAllDetectStepData();
            if (null == dataList)
            {
                return;
            }

            foreach (var data in dataList)
            {
                var customData = data.GetCustomStepData<RangeDetectionData>();
                if (null == customData)
                {
                    continue;
                }
                if (!customData.result.hasResult)
                {
                    continue;
                }
                Vector3 dir = (customData.result.rangeRootPositionWorldSpace - head.GetPosition()).normalized;
                float dotProd = Vector3.Dot(dir, head.GetRotation()* Vector3.forward);
                if (dotProd > 0.5) // range: -1 to 1. -1:  looking completely the opposite; 1: looking exactly; 0.5: around 45 degree
                {
                    firstLookGestureDetectionAreaTimeElpasedTelemetry.checkTimeElapsed = true;
                    trackFirstLook = false;
                    return;
                }
            }
        }

        protected void SetupGestureControl()
        {
            if (null == gestureSystem)
            {
                gestureSystem = this.gameObject.GetComponent<SpawnerGestureDetection>();
            }
            if (null == gestureSystem)
            {
                gestureSystem = this.gameObject.AddComponent<SpawnerGestureDetection>();
            }

            if (Application.isPlaying && null != gestureSystem)
            {
                // TODO: IOC Command Bind
                gestureSystem.mechanicData = gestureTriggerData;
                gestureSystem.AddListerner(OnGestureEvent);
            }
        }

        // TODO: IOC Command Bind
        protected void OnGestureEvent(IMechanicSystemEvent systemEvent)
        {
            switch(systemEvent.eventKey)
            {
                case DetectionEvents.StateChanged: ActiveBreath(systemEvent.GetData<bool>()); break;
                case MechSysSpawnStateEvent.Ready: gestureSystem.SetActive(active); break;
            }
        }

        // TODO: IOC Command Bind
        protected override void OnReceiveEvent(IMechanicSystemEvent systemEvent)
        {
            base.OnReceiveEvent(systemEvent);
            switch (systemEvent.eventKey)
            {
                case MechSysEvent.OnActiveStateChanged: OnActiveChanged(); break;
                case MechSysEvent.BeforeStart: BeforeStart(); break;
                case MechSysEvent.OnStop: OnStop(); break;
            }
        }

        void OnActiveChanged()
        {
            if (null != gestureSystem && gestureSystem.ready)
            {
                gestureSystem?.SetActive(active);
            }
        }

        protected void BeforeStart()
        {
            firstLookGestureDetectionAreaTimeElpasedTelemetry.checkTimeElapsed = false;
            firstLookGestureDetectionAreaTimeElpasedTelemetry.isCompleted = false;
            trackFirstLook = true;
            firstLookGestureDetectionAreaTimeElpasedTelemetry.startCheckingTime = true;
        }

        protected void OnStop()
        {
            trackFirstLook = false;
            if (!firstLookGestureDetectionAreaTimeElpasedTelemetry.isCompleted)
            {
                firstLookGestureDetectionAreaTimeElpasedTelemetry.checkTimeElapsed = true;
                firstLookGestureDetectionAreaTimeElpasedTelemetry.SendEvents();
            }
        }

        protected void SetupTelemetry()
        {
            firstLookGestureDetectionAreaTimeElpasedTelemetry = this.transform.GetOrAddComponent<TimeElpasedTelemetry>();
            telemetries.Add(firstLookGestureDetectionAreaTimeElpasedTelemetry);
        }

        protected void UpdateTelemetry()
        {
            foreach (var telemetry in telemetries)
            {
                if (!telemetry.isCompleted && telemetry.IsValidated())
                {
                    telemetry.id = GetTelemetryId();
                    telemetry.SendEvents();
                }
            }
        }

        protected string GetTelemetryId()
        {
            string sysId = (null == targetSystem || null == targetSystem.component || null == targetSystem.component.gameObject) ? "??" :
                targetSystem.component.gameObject.GetInstanceID().ToString();
            return "Deep Breath - First Look Detection Area Time: " + sysId;
        }
    }
}
