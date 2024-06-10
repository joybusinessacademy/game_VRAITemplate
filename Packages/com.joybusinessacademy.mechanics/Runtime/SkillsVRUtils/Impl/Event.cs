using System.Collections.Generic;

namespace SkillsVR.Impl
{
	public class Event<SENDER_TYPE> : IEvent<SENDER_TYPE>
    {
        public SENDER_TYPE sender { get; protected set; }

        public object eventKey { get; protected set; }

        public object data { get; protected set; }

        public IEnumerable<object> args { get; protected set; }

        object IEvent.sender => this.sender;

        public IEnumerable<IEvent> innerEvents => innerEventList;

        protected List<IEvent> innerEventList = new List<IEvent>();

        public Event(SENDER_TYPE initSender, object eventType, object userData = null, params object[] userArgs)
        {
            sender = initSender;
            eventKey = eventType;
            data = userData;
            args = userArgs;
        }

        public void ResetSender(SENDER_TYPE newSender)
        {
            if (null == newSender)
            {
                return;
            }
            this.sender = newSender;
        }

        public void AddEvent(IEvent innerEvent)
        {
            if (null == innerEvent)
            {
                return;
            }
            if (innerEventList.Contains(innerEvent))
            {
                return;
            }
            innerEventList.Add(innerEvent);
        }
        public override string ToString()
        {
            return GetString("{e} ({et}) {st}\r\nData: {d} ({dt})\r\nSender: {s} ({st})\r\nArgs:{at}");
        }

        /// <summary>
        /// Get string with format.
        /// </summary>
        /// <param name="format">output string format, 
        /// {e} - event key, 
        /// {et} - event key type,
        /// {d} - data
        /// {dt} - data type
        /// {s} - sender
        /// {st} - sender type
        /// {a} - args list</param>
        /// {at} - typed args list
        /// </param>
        /// <returns></returns>
        public virtual string GetString(string format)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                return ToString();
            }
            return format
                .Replace("{e}", eventKey.GetObjectString())
                .Replace("{et}", eventKey.GetTypeName())
                .Replace("{d}", data.GetObjectString())
                .Replace("{dt}", data.GetTypeName())
                .Replace("{s}", sender.GetObjectString())
                .Replace("{st}", sender.GetTypeName())
                .Replace("{a}", args.GetObjectListString("{o}"))
                .Replace("{at}", args.GetObjectListString());
        }
        public T GetData<T>()
        {
            return GetData<T>(default(T));
        }

        public T GetData<T>(T defaultValue)
        {
            if (null == data)
            {
                return defaultValue;
            }
            if (data is T)
            {
                return (T)data;
            }
            else
            {
                return defaultValue;
            }
        }
    }
}
