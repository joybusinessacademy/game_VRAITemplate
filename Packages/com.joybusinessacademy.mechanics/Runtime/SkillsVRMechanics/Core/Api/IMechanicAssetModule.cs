using System;
using System.Collections.Generic;

namespace SkillsVR.Mechanic.Core
{
	public interface IMechanicAssetModule
    {
        void GetAsset<T>(string key, Action<IResult<T>> callback, params object[] optionalArgs)
            where T : UnityEngine.Object;

        void GetMechanic<T>(string key, string name,bool networkStatus, Action<IMechanicSystemResult> callback, params object[] optionalArgs)
            where T : IMechanicSystem;

        List<string> GetKeys(Predicate<string> predicate);
        void AddKeys(IEnumerable<string> keys);
    }
}
