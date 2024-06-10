using NUnit.Compatibility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkillsVR.CCK.PackageManager.Ioc
{

    public class CCKBinding : IBinding
    {
        List<object> keys = new List<object>();

        CCKBinder parent;

        object value;
        object singleton;

        bool alreadyAdd;
        public enum ProvideMode
        {
            CreateNew,
            Instance,
            Singleton,
            Delegate,
        };

        ProvideMode mode;

        public CCKBinding(CCKBinder binder, params object[] initKeys)
        {
            parent = binder;
            if (null != initKeys)
            {
                keys.AddRange(initKeys);
            }
        }


        public IBinding To<TValue>()
        {
            CheckDuplicatedBindings();
            value = typeof(TValue);
            mode = ProvideMode.CreateNew;

            AddToBinder();
            return this;
        }

        public IBinding AsSingleton()
        {
            mode = ProvideMode.Singleton;
            return this;
        }

        public IBinding To<TValue>(Func<object> provideFunc)
        {
            CheckDuplicatedBindings();
            value = provideFunc;
            mode = ProvideMode.Delegate;
            AddToBinder();
            return this;
        }

        public IBinding To<TValue>(TValue instance)
        {
            CheckDuplicatedBindings();
            value = instance;
            mode = ProvideMode.Instance;
            AddToBinder();
            return this;
        }

        public IBinding Bind(params object[] extraKeys)
        {
            CheckDuplicatedBindings();
            if (null != extraKeys)
            {
                keys.AddRange(extraKeys);
            }
            return this;
        }

        protected void CheckDuplicatedBindings()
        {
            if (alreadyAdd)
            {
                throw new Exception($"This binding is already added to binder.\r\n{this}");
            }
        }
        protected void AddToBinder()
        {
            parent.Bindings.SetValue(this, keys.ToArray());
            alreadyAdd = true;
        }


        public override string ToString()
        {
            return $"{ string.Join("-", keys)} <- {mode} -> {(null == value ? "null" : value.GetType().Name + " " + value)}";
        }

        public T Provide<T>()
        {
            T v = default(T);
            TryResolve<T>(out v);
            return v;
        }

        public bool TryResolve<T>(out T outValue)
        {
            outValue = default(T);
            if (null == value)
            {
                return false;
            }

            object provideInstance = null;
            switch (mode)
            {
                case ProvideMode.CreateNew:
                    provideInstance = CreateNewFromValue<T>();
                    break;
                case ProvideMode.Instance:
                    provideInstance = value;
                    break;
                case ProvideMode.Singleton:
                    if (null == singleton)
                    {
                        singleton = CreateNewFromValue<T>();
                    }
                    provideInstance = singleton;
                    break;
                case ProvideMode.Delegate:
                    if (value is Func<object> callback)
                    {
                        provideInstance = callback?.Invoke();
                    }
                    break;
                default:
                    break;
            }

            if (null == provideInstance)
            {
                return false;
            }

            if (provideInstance is T typedInstance)
            {
                outValue = typedInstance;
                return true;
            }
            return false;
        }

        object CreateNewFromValue<T>()
        {
            if (!(value is Type))
            {
                return null;
            }
            var valueType = value as Type;
            if (typeof(Type) == valueType
                   || !valueType.IsClass
                   || valueType.IsAbstract
                   || valueType.IsGenericType
                   )
            {
                return null;
            }
            try
            {
                if (valueType == typeof(T) || typeof(T).IsAssignableFrom(valueType))
                {
                    return Activator.CreateInstance(valueType);
                }
            }
            catch(Exception e)
            {
                Debug.LogError($"Cannot create instance from type {valueType}: {e.Message}\r\n{e.StackTrace}");
            }
            
            return null;
        }
    }
}