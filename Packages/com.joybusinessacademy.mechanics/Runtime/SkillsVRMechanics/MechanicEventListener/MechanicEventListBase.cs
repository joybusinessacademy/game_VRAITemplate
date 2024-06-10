using System;
using System.Collections.Generic;
using System.Linq;
using SkillsVR.Mechanic.Core;

namespace SkillsVR.Mechanic.Events
{
	public interface IMechanicEventListBase
	{
		Type ArguType { get; }
		void Add(object key);
		bool Contains(object key);
		int Count { get; }

		void Invoke(IMechanicSystemEvent sysEvent);
	}

	[Serializable]
	public abstract class MechanicEventListBase<TA, T> : IMechanicEventListBase where T : SerializableEventT<TA>, new()
	{
		public Type ArguType => typeof(TA);

		public List<T> list = new List<T>();

		public int Count => null == list ? 0 : list.Count;

		public T GetEvent(object key)
		{
			if (null == key)
			{
				return default(T);
			}
			if (key is Enum)
			{
				return list.Where(x => null != x && x.eventId.GetType() == key.GetType() && (int)x.eventId == (int)key).FirstOrDefault();
			}
			return list.Find(x => null != x && x.eventId == key);
		}
		public void Add(object key)
		{
			if (null == key)
			{
				return;
			}
			if (null != GetEvent(key))
			{
				return;
			}
			T t = new T();
			t.SetId(key);
			list.Add(t);
		}

		public bool Contains(object key)
		{
			return null != GetEvent(key);
		}

		public void Invoke(IMechanicSystemEvent sysEvent)
		{
			if (null == sysEvent || null == sysEvent.eventKey)
			{
				return;
			}
			var eventItem = GetEvent(sysEvent.eventKey);
			if (null == eventItem)
			{
				return;
			}

			if (typeof(IMechanicSystemEvent) == typeof(TA))
			{
				eventItem.Invoke((TA)sysEvent);
			}
			else if (null == sysEvent.data)
			{
				eventItem.Invoke(default(TA));
			}
			else if (sysEvent.data.GetType() == typeof(TA) || typeof(TA).IsAssignableFrom(sysEvent.data.GetType()))
			{
				eventItem.Invoke((TA)sysEvent.data);
			}
		}
	}
}



