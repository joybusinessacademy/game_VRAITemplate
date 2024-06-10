using SkillsVR.Mechanic.Core;
using SkillsVR.Mechanic.Core.Impl;
using System.Collections.Generic;
using UnityEngine;
using VRMechanics.Mechanics.GestureDetection;

namespace SkillsVR.Mechanic.MechanicSystems.Gesture
{
	public class GestureDetectionSystem : AbstractMechanicSystemBehivour<GestureDetectionData>, IGestureSystem
    {
        public bool isRunning => null == templateDetector ? false : templateDetector.isRunning;

        public GestureTemplate gestureTemplate = new GestureTemplate();
        public RuntimeGestureBody runtimePose = new RuntimeGestureBody();

        public IGestureBody templateGesture => null == gestureTemplate ? null : gestureTemplate.gesture;
        public IGestureDetector templateDetector => null == gestureTemplate ? null : gestureTemplate.detector;

        public GameObject templateVisualizerObject;
        public GameObject runtimePosVisualizerObject;

        protected GestureVisualizer gestureVisualizer = new GestureVisualizer();

        public void OnStart()
        {
            if (null == templateDetector)
            {
                TriggerEvent(DetectionEvents.NotStart, false);
                return;
            }

            StartCoroutine(templateDetector.StartDetectCoroutine(templateGesture, runtimePose, OnDetectorEvents, -1, 0.2f));
        }

        public void OnStop()
        {
            templateDetector.StopDetectCoroutine();
        }

        protected void OnDetectorEvents(DetectionEvents eventType, bool detectionResult)
        {
            switch (eventType)
            {
                case DetectionEvents.BeforeDetect:
                    runtimePose.UpdateNormalizedGestureBody();
                    break;
            }
            TriggerEvent(eventType, detectionResult);
        }

        protected override void OnReceiveEvent(IMechanicSystemEvent systemEvent)
        {
            switch(systemEvent.eventKey)
            {
                case MechSysEvent.BeforeStart: BeforeStart(); break;
                case MechSysEvent.OnStart: OnStart(); break;
                case MechSysEvent.OnStop: OnStop(); break;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (null == gestureVisualizer)
            {
                gestureVisualizer = new GestureVisualizer();
            }
            gestureVisualizer.UpdateGestureToGameObject(runtimePose, runtimePosVisualizerObject);
            gestureVisualizer.UpdateGestureToGameObject(templateGesture, templateVisualizerObject);
        }

        protected override void Update()
        {
            base.Update();
            if (null != runtimePosVisualizerObject)
            {
                runtimePose.UpdateNormalizedGestureBody();
                gestureVisualizer.UpdateGestureToGameObject(runtimePose, runtimePosVisualizerObject);
            }
            gestureVisualizer.UpdateGestureToGameObject(templateGesture, templateVisualizerObject);
        }

        protected void BeforeStart()
        {
            LoadTemplate();
            SetupRuntimeBody();
            SetupVisualObjects();
        }

        protected void LoadTemplate()
        {
            if (null == mechanicData || null == mechanicData.gestureTemplateAsset)
            {
                throw new System.ArgumentNullException("Try start gesture detection with null template. Detector: " + this);
            }
            gestureTemplate.FromFileOverwrite(mechanicData.gestureTemplateAsset);
        }

        protected void SetupRuntimeBody()
        {
            if (null != mechanicData.runtimeGestureRef && null != mechanicData.runtimeGestureRef.poseDataList)
            {
                runtimePose.ClearRuntimeBodyParts();
                foreach (var item in mechanicData.runtimeGestureRef.poseDataList)
                {
                    if (null == item)
                    {
                        continue;
                    }
                    runtimePose.AddRuntimeBodyPart(item.type, item.transform);
                }
            }
        }

        protected void SetupVisualObjects()
        {
            if (null == mechanicData)
            {
                return;
            }
            templateVisualizerObject = mechanicData.gestureTemplateVisualObject;
            runtimePosVisualizerObject = mechanicData.runtimeGestureVisualObject;
        }

        public IEnumerable<GestureDetectStepData> GetAllDetectStepData()
        {
            if (null == templateDetector)
            {
                return null;
            }
            return templateDetector.GetAllDetectStepData();
        }

        public IGestureBody GetRuntimePose()
        {
            return runtimePose;
        }

        public IGestureBody GetTemplate()
        {
            return templateGesture;
        }
    }
}
