namespace VRMechanics.Mechanics.GestureDetection.StepDetectors
{
	public abstract class AbstractGestureStepDetector<T> : IGestureDetectStep where T : CustomGestureDetectStepData, new()
    {
        protected abstract bool OnDetect(T detectorData, IGestureBody template, IGestureBody runtimePose, bool defaultResult);

        public CustomGestureDetectStepData CreateCustomData()
        {
            return new T();
        }

        public bool Detect(GestureDetectStepData data, IGestureBody template, IGestureBody runtimePose)
        {
            if (null == data || data.detectorName != this.GetType().Name)
            {
                return true;
            }
            if (null == template
                || 0 == template.BodyPartsCount()
                || null == runtimePose
                || 0 == runtimePose.BodyPartsCount())
            {
                return data.defaultResult;
            }

            var detectorData = data.GetCustomStepData<T>();
            if (null == detectorData)
            {
                return data.defaultResult;
            }
            
            return OnDetect(detectorData, template, runtimePose, data.defaultResult);
        }

        public bool ExecDetectFromData(GestureDetectStepData data, IGestureBody one, IGestureBody other)
        {
            return DetectStepManager.Detect(data, one, other);
        }
    }
}