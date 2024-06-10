using System;
using UnityEngine.UIElements;

namespace SkillsVR.CCK.PackageManager.UI.Events
{
    public interface ICloneableUIEvent
    {
        EventBase Clone();
    }
    
    public abstract class AbstractUIEvent<EVENT_TYPE> : EventBase<EVENT_TYPE>, ICloneableUIEvent where EVENT_TYPE : EventBase<EVENT_TYPE>, new()
    {
        public EventBase Clone()
        {
            return GetPooled();
        }
    }

    public abstract class AbstractUIEvent<EVENT_TYPE, ARG1> : EventBase<EVENT_TYPE>, ICloneableUIEvent where EVENT_TYPE : AbstractUIEvent<EVENT_TYPE, ARG1>, new()
    {
        protected ARG1 arg1 { get; private set; }

        public static EVENT_TYPE GetPooled(ARG1 arg1)
        {
            var evt = EventBase<EVENT_TYPE>.GetPooled();
            evt.arg1 = arg1;
            return evt;
        }
        public AbstractUIEvent()
        {
            LocalInit();
        }
        protected override void Init()
        {
            base.Init();
            LocalInit();
        }

        private void LocalInit()
        {
            this.bubbles = true;
            this.tricklesDown = true;
        }

        public EventBase Clone()
        {
            return GetPooled(this.arg1);
        }
    }

    public abstract class AbstractUIEvent<EVENT_TYPE, ARG1, ARG2> : EventBase<EVENT_TYPE>, ICloneableUIEvent where EVENT_TYPE : AbstractUIEvent<EVENT_TYPE, ARG1, ARG2>, new()
    {
        protected ARG1 arg1 { get; private set; }
        protected ARG2 arg2 { get; private set; }

        public static EVENT_TYPE GetPooled(ARG1 arg1, ARG2 arg2)
        {
            var evt = EventBase<EVENT_TYPE>.GetPooled();
            evt.arg1 = arg1;
            evt.arg2 = arg2;
            return evt;
        }

        public AbstractUIEvent()
        {
            LocalInit();
        }

        protected override void Init()
        {
            base.Init();
            LocalInit();
        }
        private void LocalInit()
        {
            this.bubbles = true;
            this.tricklesDown = true;
        }

        public EventBase Clone()
        {
            return GetPooled(this.arg1, this.arg2);
        }
    }
}