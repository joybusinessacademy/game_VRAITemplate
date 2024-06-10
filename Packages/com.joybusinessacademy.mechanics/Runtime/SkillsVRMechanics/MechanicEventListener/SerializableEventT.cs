using System;
using UnityEngine;
using UnityEngine.Events;
using SkillsVR.Mechanic.Core;

namespace SkillsVR.Mechanic.Events
{

	public interface SerializableEventT 
	{
		object eventId { get; }
		Type argumentType { get; }
	}

	[Serializable]
	public abstract class SerializableEventT<T> : SerializableEventT
	{
		public abstract void Invoke(T data);

		public object eventId => key;
		public Type argumentType => typeof(T);

		[SerializeReference] public object key;

		public void SetId(object id)
		{
			key = id;
		}
	}
}



