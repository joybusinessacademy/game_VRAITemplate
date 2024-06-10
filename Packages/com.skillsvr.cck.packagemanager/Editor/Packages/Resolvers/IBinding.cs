using System;

namespace SkillsVR.CCK.PackageManager.Ioc
{
    public interface IBinding
    {
        IBinding Bind(params object[] extraKeys);
        IBinding To<TValue>();
        IBinding To<TValue>(Func<object> provideFunc);
        IBinding To<TValue>(TValue instance);
        IBinding AsSingleton();

        T Provide<T>();
    }

    public interface IProvider
    {
        T Provide<T>();
        bool TryProvide<T>(out T value);
    }
}