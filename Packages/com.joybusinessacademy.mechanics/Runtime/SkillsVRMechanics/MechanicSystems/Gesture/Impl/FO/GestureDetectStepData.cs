using System;
using UnityEngine;

namespace VRMechanics.Mechanics.GestureDetection
{
    [Serializable]
    public class CustomGestureDetectStepData {}

    [Serializable]
    public sealed class GestureDetectStepData
    {
        [TypeNameValueDropdown(baseType = typeof(IGestureDetectStep), onValueChangedCallback = nameof(OnDetectorChanged))]
        public string detectorName;

        public bool defaultResult = true;

        [SerializeReference]
        public CustomGestureDetectStepData detectorData;

        public T GetCustomStepData<T>() where T : CustomGestureDetectStepData
        {
            return (null == detectorData || !(detectorData is T)) ? null : (T)detectorData;
        }

        private void OnDetectorChanged()
        {
            var detector = DetectStepManager.GetDetector(detectorName);
            detectorData = null == detector ? null : detector.CreateCustomData();
        }

        public GestureDetectStepData()
        {
            OnDetectorChanged();
        }
    }
}