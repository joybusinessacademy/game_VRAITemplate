using System;
using UnityEngine;
using UnityEngine.Events;

namespace VRMechanics.Mechanics.GestureDetection
{
    [Serializable]
    [ExecuteInEditMode]
	public class GestureDetectionMechanic: AbstractMechanicSystem<GestureDetectionData>
    {
        public string id => null == templateGesture || string.IsNullOrWhiteSpace(templateGesture.GetId()) ? gameObject.name : templateGesture.GetId();
        public GestureTemplate gestureTemplate = new GestureTemplate();
        public RuntimeGestureBody runtimePose = new RuntimeGestureBody();
        
        [Serializable]
        public class BoolEvent : UnityEvent<bool> { }

        public UnityEvent onDetectionStart = new UnityEvent();
        public UnityEvent onDetectionStop = new UnityEvent();
        public UnityEvent onDetectSuccess = new UnityEvent();
        public BoolEvent onDetectResultChanged = new BoolEvent();
        public BoolEvent onDetect = new BoolEvent();

        public bool isRunning => null == templateDetector ? false : templateDetector.isRunning;

        public IGestureBody templateGesture => null == gestureTemplate ? null : gestureTemplate.gesture;
        public IGestureDetector templateDetector => null == gestureTemplate ? null : gestureTemplate.detector;


        public GameObject templateVisualizerObject;
        public GameObject runtimePosVisualizerObject;

        protected GestureVisualizer gestureVisualizer = new GestureVisualizer();

        public bool Detect()
        {
            runtimePose.UpdateNormalizedGestureBody();
            if (null == templateDetector)
            {
                return false;
            }
            return templateDetector.Detect(templateGesture, runtimePose);
        }
        public void StartDetect()
        {
            StartDetectEx(-1.0f);
        }

        public void StartDetectEx(float timeout, float detectionInterval = 0.2f)
        {
            if (null == templateDetector)
            {
                OnDetectorEvents(DetectionEvents.NotStart, false);
                return;
            }
            StartCoroutine(templateDetector.StartDetectCoroutine(templateGesture, runtimePose, OnDetectorEvents, timeout, detectionInterval));
        }

        public void StopDetect()
        {
            templateDetector?.StopDetectCoroutine();
        }

        public void LoadTemplateFromJsonAsseet(TextAsset jsonAsset)
        {
            gestureTemplate.FromFileOverwrite(jsonAsset);
        }

        public void AddRuntimePoseRef(string id, Transform transform)
        {
            runtimePose.AddRuntimeBodyPart(id, transform);
        }

        public void SetVisualObject(string id, GameObject visualObject)
        {
            if(string.IsNullOrWhiteSpace(id))
            {
                return;
            }
            switch(id)
            {
                case "Template": templateVisualizerObject = visualObject; return;
                case "Runtime": runtimePosVisualizerObject = visualObject; return;
                default: break;
            }
        }

        private void OnEnable()
        {
            if (null == gestureVisualizer)
            {
                gestureVisualizer = new GestureVisualizer();
            }
            gestureVisualizer.UpdateGestureToGameObject(runtimePose, runtimePosVisualizerObject);
            gestureVisualizer.UpdateGestureToGameObject(templateGesture, templateVisualizerObject);
        }

        protected void Update()
        {
            if (null != runtimePosVisualizerObject)
            {
                runtimePose.UpdateNormalizedGestureBody();
                gestureVisualizer.UpdateGestureToGameObject(runtimePose, runtimePosVisualizerObject);
            }
            gestureVisualizer.UpdateGestureToGameObject(templateGesture, templateVisualizerObject);
        }

        protected void OnDetectorEvents(DetectionEvents eventType, bool detectionResult)
        {
            switch (eventType)
            {
                case DetectionEvents.StartDetect:
                    onDetectionStart?.Invoke();
                    break;
                case DetectionEvents.StopDetect:
                    onDetectionStop?.Invoke();
                    break;
                case DetectionEvents.AlreadyStart:
                    break;
                case DetectionEvents.StateChanged:
                    onDetectResultChanged?.Invoke(detectionResult);
                    if (detectionResult)
                    {
                        onDetectSuccess?.Invoke();
                    }
                    break;
                case DetectionEvents.BeforeDetect:
                    runtimePose.UpdateNormalizedGestureBody();
                    break;
                case DetectionEvents.OnDetect:
                    onDetect?.Invoke(detectionResult);
                    break;
                default:
                    break;
            }

            switch (eventType)
            {
                case DetectionEvents.None:
                    break;
                case DetectionEvents.StartDetect:
                case DetectionEvents.StopDetect:
                case DetectionEvents.AlreadyStart:
                case DetectionEvents.NotStart:
                    Debug.Log(string.Join(" ", eventType, id));
                    break;
                case DetectionEvents.StateChanged:
                    Debug.Log(string.Join(" ", id, eventType, detectionResult));
                    break;
                case DetectionEvents.BeforeDetect:
                    break;
                case DetectionEvents.OnDetect:
                    break;
            }

            if (null != spawner)
            {
                SendMessageToSpawner(nameof(OnDetectorEvents), eventType, detectionResult);
            }
        }

        protected override object OnReceiveMechanicSystemMessage(string message, object[] args)
        {
            switch (message)
            {
                case nameof(Detect): return Detect();
                case nameof(StartDetect): StartDetect(); return null;
                case nameof(StartDetectEx): StartDetectEx((float)args[0], (float)args[1]); return null;
                case nameof(StopDetect): StopDetect(); return null;
                default: return base.OnReceiveMechanicSystemMessage(message, args);
            }
            
        }

        protected override void OnMechanicDataReady()
        {
            base.OnMechanicDataReady();
            
            if(mechanicData.gestureTemplateAsset == null)
			{
                Debug.LogError("Mechanic Gesture Template Asset is Null: Check Mechanic Spawner / Check Addressable Tags");
                return;
			}

            LoadTemplateFromJsonAsseet(mechanicData.gestureTemplateAsset);
            if (null != mechanicData.runtimeGestureRef && null != mechanicData.runtimeGestureRef.poseDataList)
            {
                runtimePose.ClearRuntimeBodyParts();
                foreach (var item in mechanicData.runtimeGestureRef.poseDataList)
                {
                    if (null == item)
                    {
                        continue;
                    }
                    AddRuntimePoseRef(item.type, item.transform);
                }
            }
            templateVisualizerObject = mechanicData.gestureTemplateVisualObject;
            runtimePosVisualizerObject = mechanicData.runtimeGestureVisualObject;
        }
    }
}