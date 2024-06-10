using System.Collections.Generic;

namespace SkillsVR.Mechanic.Core
{
	public interface IDictionaryManager<TKey, TValue> : IDictionary<TKey, TValue>
    {
        T Get<T>(TKey key) where T : TValue;
        bool TryGet<T>(TKey key, out TValue value) where T : TValue;
    }
}
