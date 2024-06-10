using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

namespace SkillsVR.CCK.PackageManager.Ioc.Providers
{
    public class CreateFromTypeProvider : IProvider
    {
        public Type TargetType { get; protected set; }

        public CreateFromTypeProvider(Type type)
        {
            TargetType = type;
        }

        public virtual T Provide<T>()
        {
            T value = default(T);
            TryProvide<T>(out value);
            return value;
        }

        public virtual bool TryProvide<T>(out T value)
        {
            value = default(T);
            if (null == TargetType)
            {
                Debug.LogError("Cannot provide instance from null type.");
                return false;
            }

            if (!TargetType.IsClass
                || TargetType == typeof(Type)
                || TargetType.IsAbstract 
                || TargetType.IsGenericType
                )
            {
                Debug.LogError($"Cannot provide instance from type {TargetType} which is abstract, or generic, or not class, or is type of Type.");
                return false;
            }

            var outputType = typeof(T);
            if (TargetType != outputType
                && !outputType.IsAssignableFrom(TargetType))
            {
                Debug.LogError($"Cannot provide {TargetType} to {outputType}.");
                return false;
            }

            var defaultConstructor = TargetType.GetConstructor(null);
            if (null == defaultConstructor)
            {
                Debug.LogError($"Cannot provide {TargetType}: no default constructor.");
                return false;
            }

            var inst = defaultConstructor.Invoke(null);
            if (null == inst)
            {
                Debug.LogError($"Cannot provide {TargetType}: Create instance fail.");
                return false;
            }

            if (inst is T )
            {
                value = (T)inst;
            }

            return false;
        }
    }
}