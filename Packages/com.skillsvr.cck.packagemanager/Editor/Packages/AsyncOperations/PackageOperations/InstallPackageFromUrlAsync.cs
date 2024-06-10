using SkillsVR.CCK.PackageManager.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace SkillsVR.CCK.PackageManager.AsyncOperation.PackageOperations
{
    public class InstallPackageFromUrlAsync : CustomAsyncOperation<AddRequest>
    {
        public CCKPackageInfo PackageInfo { get; protected set; }

        public InstallPackageFromUrlAsync(CCKPackageInfo package)
        {
            PackageInfo = package;
        }
        protected override IEnumerator OnProcessRoutine()
        {
            if (null == PackageInfo)
            {
                SetError("Package to installed is null.");
                yield break;
            }

            string url = PackageInfo.url;
            if (string.IsNullOrWhiteSpace(url))
            {
                SetError("Url cannot be null or empty.");
                yield break;
            }
            var op = new QueuedClientRequest<AddRequest>(() => Client.Add(url));
            yield return op;

            TrySetErrorFrom(op);
            Result = op.Result;
        }

        protected override void SetError(string errorMsg)
        {
            errorMsg = "Install Package from Url Fail: " + errorMsg;
            base.SetError(errorMsg);
        }

        public override List<string> GetExtraInfoStrings()
        {
            var list =  base.GetExtraInfoStrings();
            list.Add(PackageInfo.ToText("Package"));
            return list;
        }
    }
}