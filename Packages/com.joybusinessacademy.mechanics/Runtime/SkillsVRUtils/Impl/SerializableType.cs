using System;
using UnityEngine;

namespace SkillsVR.Serialization
{
	[Serializable]
	public class SerializableType : ISerializationCallbackReceiver
	{
		[SerializeField]
		[HideInInspector]
		private string name;

		[SerializeField]
		[HideInInspector]
		private string assembly;

		public Type type { get; private set; }

		public SerializableType() { }
		public SerializableType(Type targetType) { type = targetType; }


		public void OnAfterDeserialize()
		{
			if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(assembly))
			{
				type = null;
				return;
			}
			type = Type.GetType(string.Format("{0},{1}", name, assembly));
		}

		public void OnBeforeSerialize()
		{
			name = null == type ? null : type.FullName;
			assembly = null == type ? null : type.Assembly.ToString();
		}

		public static implicit operator Type(SerializableType serializableType)
		{
			return null == serializableType ? null : serializableType.type;
		}

		public static implicit operator SerializableType(Type type)
		{
			return new SerializableType(type);
		}

		public override string ToString()
		{
			return null == type ? "null" : type.Name;
		}
	}
}



