using System;

namespace SkillsVR.Mechanic.Core
{
    public interface IPooledMechanicProvider : IMechanicProvider
    {
        void PreparePoolObjects(Type type, int count);
        void PreparePoolObjects<T>(Type type, int count);

        void SetReusePoolObject(bool enable);

        void AddOneTimeAllObjectsReadyListener(Action callback);
        void RemoveAllObjectsReadyListener(Action callback);
    }
}