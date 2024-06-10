using SkillsVR.CCK.PackageManager.Ioc.Data;
using System.Collections.Generic;
using UnityEngine;

namespace SkillsVR.CCK.PackageManager.Ioc
{
    public class CCKBinder : IBinder
    {
        public BindingMap Bindings { get; } = new BindingMap();

        public IBinding Bind<TKey>()
        {
            return new CCKBinding(this, typeof(TKey));
        }

        public IBinding Bind(params object[] keys)
        {
            return new CCKBinding(this, keys);
        }

        public TKey Provide<TKey>()
        {
            var binding = Bindings.GetValue<IBinding>(typeof(TKey));
            if (null == binding)
            {
                return default(TKey);
            }
            return binding.Provide<TKey>();
        }

        public TKey Provide<TKey>(params object[] extraKeys)
        {
            var keyList = new List<object>();
            keyList.Add(typeof(TKey));
            if (null != extraKeys)
            {
                keyList.AddRange(extraKeys);
            }
            var binding=  Bindings.GetValue<IBinding>(keyList.ToArray());
            if (null == binding)
            {
                return default(TKey);
            }
            return binding.Provide<TKey>();
        }

        public TValue ProvideAs<TValue>(params object[] keys)
        {
            var binding = Bindings.GetValue<IBinding>(keys);
            if (null == binding)
            {
                return default(TValue);
            }
            return binding.Provide<TValue>();
        }
    }
}