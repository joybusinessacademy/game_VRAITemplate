namespace SkillsVR.CCK.PackageManager.Ioc
{
    public interface IBinder
    {
        IBinding Bind<TKey>();
        IBinding Bind(params object[] keys);

        TKey Provide<TKey>();
        TKey Provide<TKey>(params object[] keys);
        TValue ProvideAs<TValue>(params object[] keys);
    }
}