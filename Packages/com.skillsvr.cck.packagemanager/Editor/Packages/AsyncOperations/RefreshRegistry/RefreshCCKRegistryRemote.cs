using SkillsVR.CCK.PackageManager.Data;
using SkillsVR.CCK.PackageManager.Registry;
using System.Collections;
using UnityEngine;

namespace SkillsVR.CCK.PackageManager.AsyncOperation.Registry
{
    public class RefreshCCKRegistryRemote : CustomAsyncOperation
    {
        public CCKRegistry Registry { get; protected set; }

        public RefreshCCKRegistryRemote(CCKRegistry registry)
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

            var url = Registry.Source.url;
            var auth = Registry.Source.auth;
            var type = Registry.Source.type;
            var op = new DownloadJsonWithUrlAndResolverTypeAsync(url, auth, type);
            yield return op;
            Registry.LoadFromJson(op.Result);
        }
    }
}