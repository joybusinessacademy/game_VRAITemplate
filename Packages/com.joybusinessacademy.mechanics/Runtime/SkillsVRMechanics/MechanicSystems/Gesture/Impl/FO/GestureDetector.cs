using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace VRMechanics.Mechanics.GestureDetection
{
    [Serializable]
    public class GestureDetector : IGestureDetector
    {
        
        public List<GestureDetectStepData> detectionStepDatas = new List<GestureDetectStepData>();

        public bool isRunning { get; protected set; }

        public bool Detect(IGestureBody one, IGestureBody other)
        {
            foreach (var detectStep in detectionStepDatas)
            {
                if (!DetectStepManager.Detect(detectStep, one, other))
                {
                    return false;
                }
            }
            return true;
        }

        public IEnumerable<GestureDetectStepData> GetAllDetectStepData()
        {
            return detectionStepDatas;
        }

        public IEnumerator StartDetectCoroutine(IGestureBody one, IGestureBody other, Action<DetectionEvents, bool> eventCallback, float timeout = -1.0f, float detectionInterval = 0.2f)
        {
            if (isRunning)
            {
                eventCallback?.Invoke(DetectionEvents.AlreadyStart, false);
                yield break;
            }
            isRunning = true;

            eventCallback?.Invoke(DetectionEvents.BeforeDetect, false);
            bool lastDetectResult = Detect(one, other);
            eventCallback?.Invoke(DetectionEvents.StartDetect, lastDetectResult);
            eventCallback?.Invoke(DetectionEvents.OnDetect, lastDetectResult);
            eventCallback?.Invoke(DetectionEvents.StateChanged, lastDetectResult);
            float timer = 0.0f;
            float startTime = Time.realtimeSinceStartup;
            while (isRunning)
            {
                startTime = Time.realtimeSinceStartup;
                if (detectionInterval > 0.0f)
                {
                    yield return new WaitForSeconds(detectionInterval);
                }
                else
                {
                    yield return null;
                }
                timer += Time.realtimeSinceStartup - startTime;

                if (0.0f < timeout && timer > timeout)
                {
                    break;
                }

                eventCallback?.Invoke(DetectionEvents.BeforeDetect, false);
                bool state = Detect(one, other);
                eventCallback?.Invoke(DetectionEvents.OnDetect, state);
                if (lastDetectResult != state)
                {
                    eventCallback?.Invoke(DetectionEvents.StateChanged, state);
                }
                lastDetectResult = state;
            }

            eventCallback?.Invoke(DetectionEvents.StopDetect, lastDetectResult);
        }

        public void StopDetectCoroutine()
        {
            isRunning = false;
        }
    }
}