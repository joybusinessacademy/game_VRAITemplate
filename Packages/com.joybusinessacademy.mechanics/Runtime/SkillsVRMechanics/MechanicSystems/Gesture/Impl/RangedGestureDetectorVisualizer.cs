using SkillsVR.Mechanic.Core;
using SkillsVR.Mechanic.Core.Impl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRMechanics.Mechanics.GestureDetection;
using VRMechanics.Mechanics.GestureDetection.StepDetectors;

namespace SkillsVR.Mechanic.MechanicSystems.Gesture
{
    public class RangedGestureDetectorVisualizer : AbstractMechanicSystemBehivour, IGestureVisualizer
    {
        public RangeAnimUI uiTemplate;

        protected Dictionary<RangeDetectionData, RangeAnimUI> dataToUIMapping = new Dictionary<RangeDetectionData, RangeAnimUI>();

        public SpawnerGestureDetection targetGesture { get; set; }

        protected override void OnReceiveEvent(IMechanicSystemEvent systemEvent)
        {
            switch (systemEvent.eventKey)
            {
                case MechSysEvent.OnStart: OnStart(); break;
                case MechSysEvent.OnStop: OnStop(); break;
            }
        }
        
        protected void OnStart()
        {
            StartCoroutine(WaitAndSetupTargetGesture());
        }

        protected IEnumerator WaitAndSetupTargetGesture()
        {
            CleanUp();
            while (null == targetGesture)
            {
                yield return null;
            }
            targetGesture?.AddListerner(OnReceiveGestureSystemEvents);
            UpdateResult();
        }

        protected void OnStop()
        {
            StopAllCoroutines();
            targetGesture?.RemoveListener(OnReceiveGestureSystemEvents);
            CleanUp();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            CleanUp();
        }

        protected void OnReceiveGestureSystemEvents(IMechanicSystemEvent eventArgs)
        {
            switch(eventArgs.eventKey)
            {
                case MechSysSpawnStateEvent.Ready:
                case MechSysEvent.OnStart:
                case DetectionEvents.OnDetect: UpdateResult(); break;
                case MechSysEvent.OnStop: StopMechanic(); break;
            }
        }

        protected void CleanUp()
        {
            foreach(var item in dataToUIMapping.Values)
            {
                if (null != item && null != item.gameObject)
                {
                    GameObject.Destroy(item.gameObject);
                }
            }
            dataToUIMapping.Clear();
        }

        protected void UpdateResult()
        {
            if (null == targetGesture || !targetGesture.active)
            {
                return;
            }
            var dataList = targetGesture.GetAllDetectStepData();
            if (null == dataList)
            {
                return;
            }

            foreach(var data in dataList)
            {
                var customData = data.GetCustomStepData<RangeDetectionData>();
                if (null == customData)
                {
                    continue;
                }
                var ui = GetOrCreateUI(customData);
                if (null == ui)
                {
                    continue;
                }
                ui.range = customData.range;
                ui.targetPos = customData.result.targetPositionWorldSpace;
                ui.transform.position = customData.result.rangeRootPositionWorldSpace;
            }
        }

        protected RangeAnimUI GetOrCreateUI(RangeDetectionData data)
        {
            if (null == data)
            {
                return null;
            }
            RangeAnimUI ui = null;
            if (dataToUIMapping.TryGetValue(data, out ui))
            {
                return ui;
            }
            ui = GameObject.Instantiate<RangeAnimUI>(uiTemplate, this.transform);
            dataToUIMapping.Add(data, ui);
            return ui;
        }
    }
}
