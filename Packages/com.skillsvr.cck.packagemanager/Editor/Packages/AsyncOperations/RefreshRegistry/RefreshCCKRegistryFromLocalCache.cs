using SkillsVR.CCK.PackageManager.Data;
using SkillsVR.CCK.PackageManager.Registry;
using System.Collections;
using UnityEngine;

namespace SkillsVR.CCK.PackageManager.AsyncOperation.Registry
{

    public class RefreshCCKRegistryFromLocalCache : CustomAsyncOperation
    {
        public CCKRegistry Registry { get; protected set; }

        public RefreshCCKRegistryFromLocalCache(CCKRegistry registry)
        {
            Registry = registry;
        }

        protected override IEnumerator OnProcessRoutine()
        {
            if (null == Registry)
            {
                SetError("Registry cannot be null.");
                yield break;
            }

            if (null == Registry.Source
                || string.IsNullOrWhiteSpace(Registry.Source.url))
            {
                SetError("Registry url cannot be null or empty.");
                yield break;
            }

            var op = new LoadCachedJsonFromUrlAsync(Registry.Source.url);
            yield return op;
            Registry.LoadFromJson(op.Result);
        }
    }
}