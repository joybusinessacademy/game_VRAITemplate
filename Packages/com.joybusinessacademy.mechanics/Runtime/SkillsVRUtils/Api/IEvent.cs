using System.Collections.Generic;

namespace SkillsVR
{
	public interface IEvent : IGetFormatString
    {
        object eventKey { get; }
        object sender { get; }
        object data { get; }
        IEnumerable<object> args { get; }
        IEnumerable<IEvent> innerEvents { get; }
        void AddEvent(IEvent innerEvent);

        T GetData<T>();
        T GetData<T>(T defaultValue);
    }

    public interface IEvent<SENDER_TYPE> : IEvent
    {
        new SENDER_TYPE sender { get; }

        void ResetSender(SENDER_TYPE newSender);
    }
}
