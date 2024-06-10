using System;
using System.Collections.Generic;

namespace SkillsVR.Mechanic.Core.Impl
{
    public class ChainedMechanicProvider : IChainedMechanicProvider
    {
        public List<IMechanicProvider> providers { get; private set; } = new List<IMechanicProvider>();

        public T ConvertToInterface<T>() where T : class, IMechanicProvider
        {
            T item = this as T;
            if (null != item)
            {
                return item;
            }
            foreach (var p in providers)
            {
                item = (T)p;
                if (null != item)
                {
                    return item;
                }
            }
            return default(T);
        }

        public IMechanicSystem GetMechanic(Type type)
        {
            foreach (var provider in providers)
            {
                if (null == provider)
                {
                    continue;
                }
                var item = provider.GetMechanic(type);
                if (null != item)
                {
                    return item;
                }
            }
            return null;
        }

        public IMechanicSystem<T> GetMechanic<T>(Type type)
        {
            foreach (var provider in providers)
            {
                if (null == provider)
                {
                    continue;
                }
                var item = provider.GetMechanic<T>(type);
                if (null != item)
                {
                    return item;
                }
            }
            return null;
        }
    }
}
