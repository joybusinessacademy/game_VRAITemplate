using SkillsVR.TimelineTool.Bindings;
using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace SkillsVR.TimelineTool.AnimatorTimeline
{
    [Serializable]
    public class SerializableTrackBinding : ICloneable
    {
        public TimelineAsset timelineAsset;
        public TrackAsset trackAsset;
        public SerializableType bindType;

        public Type outputType => null == bindType ? null : bindType;

        [SerializeReference]
        [ClassPicker(typeof(AnimatorBindingValueProvider), typeof(IGameObjectProvider), typeof(IComponentProvider), includeOriginFieldType = false)]
        public IUnityObjectProvider valueProvider;

        public object Clone()
        {
            var clone = new SerializableTrackBinding();
            clone.timelineAsset = this.timelineAsset;
            clone.trackAsset = this.trackAsset;
            clone.bindType = this.bindType;
            clone.valueProvider = null == valueProvider ? null : valueProvider.Clone() as IUnityObjectProvider;
            return clone;

        }
    }
}