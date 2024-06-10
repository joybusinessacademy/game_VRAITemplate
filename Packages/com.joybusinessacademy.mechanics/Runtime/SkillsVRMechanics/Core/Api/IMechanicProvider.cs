using SkillsVR.Mechanic.Core.Impl;
using System;
using UnityEngine;

namespace SkillsVR.Mechanic.Core
{
	public interface IMechanicProvider
    {
        IMechanicSystem GetMechanic(Type type);
        IMechanicSystem<T> GetMechanic<T>(Type type);

        T ConvertToInterface<T>() where T : class, IMechanicProvider;
    }


    public static class MechanicProvider
    {
        private static IMechanicProvider instance;
        public static IMechanicProvider Current
        {
            get
            {
                return instance;
            }
            set
            {
                if (null == instance)
                {
                    instance = value;
                }
                else
                {
                    Debug.LogError("MechanicProvider.Current already set to " + instance.GetType().Name);
                }
            }
        }
    }
}
