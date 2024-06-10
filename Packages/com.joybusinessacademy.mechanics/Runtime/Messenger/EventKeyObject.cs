using UnityEngine;

namespace SkillsVR.Messeneger
{
	[CreateAssetMenu(menuName = "Assets/EventKeyObject")]
	public class EventKeyObject : ScriptableObject
	{
		public string eventType => this.name;

		public string groupName = null;

		[TextArea]
		public string description;

#if UNITY_EDITOR
		public void DispatchUntyped()
		{
			GlobalMessenger.Instance?.Broadcast(new EventKey(name, groupName));
		}

		public void DispatchWithStringSignature()
		{
			GlobalMessenger.Instance?.Broadcast<string>(new EventKey(name, groupName), name);
		}
#endif

		public void Dispatch()
		{
			GlobalMessenger.Instance?.Broadcast<string>(name, name);
		}

		public static implicit operator string(EventKeyObject keyObject)
		{
			if (keyObject != null)
			{
				return keyObject.name;
			}
			return string.Empty;
		}

		public static implicit operator EventKey(EventKeyObject keyObject)
		{
			if (keyObject != null)
			{
				return keyObject.name;
			}
			return string.Empty;
		}
	}

}