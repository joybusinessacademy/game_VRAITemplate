using System.Collections;
using UnityEditor.PackageManager;
using UnityEngine;

namespace SkillsVR.CCK.PackageManager.AsyncOperation.PackageOperations
{
    public class EmbedPackageAsync : CustomAsyncOperation
    {
        public string PackageName { get; protected set; }
        public EmbedPackageAsync(string packageName)
        {
            PackageName = packageName;
        }

        protected override IEnumerator OnProcessRoutine()
        {
            if (string.IsNullOrWhiteSpace(PackageName))
            {
                SetError("Cannot embed package from null or empty package name.");
                yield break;
            }
           
            yield return new QueuedClientRequest(() => Client.Embed(PackageName));
            Debug.Log("Embed " + PackageName);
        }
    }
}