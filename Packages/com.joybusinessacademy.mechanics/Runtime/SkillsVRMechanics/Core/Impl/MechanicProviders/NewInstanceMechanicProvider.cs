using System;
using UnityEngine;

namespace SkillsVR.Mechanic.Core.Impl
{
    public class NewInstanceMechanicProvider : IMechanicProvider
	{
        public T ConvertToInterface<T>() where T : class, IMechanicProvider
        {
            return this as T;
        }

        public IMechanicSystem GetMechanic(Type type)
		{
			if (null == type || !typeof(Component).IsAssignableFrom(type))
			{
				return null;
			}

			if (!typeof(IMechanicSystem).IsAssignableFrom(type))
			{
				return null;
			}
			var go = new GameObject(type.Name);
			var sys = go.AddComponent(type) as IMechanicSystem;
			return sys;
		}

		public IMechanicSystem<T> GetMechanic<T>(Type type)
		{
			if (null == type || !typeof(Component).IsAssignableFrom(type))
			{
				return null;
			}
			if (!typeof(IMechanicSystem<T>).IsAssignableFrom(type))
			{
				return null;
			}
			var go = new GameObject(type.Name);
			var sys = go.AddComponent(type) as IMechanicSystem<T>;
			return sys;
		}
	}
}
