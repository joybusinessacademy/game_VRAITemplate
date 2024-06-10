using SkillsVR.CCK.PackageManager.AsyncOperation.Github;
using SkillsVR.CCK.PackageManager.Data;
using SkillsVR.CCK.PackageManager.Resolvers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkillsVR.CCK.PackageManager.AsyncOperation.CCKAssetPreview
{
    public class DownloadGithubAssetPreviewAsyncOperation : CustomAsyncOperation<Texture2D>, IDisposable
    {
        public int PreviewIndex { get; protected set; }
        public string PreviewPath { get; protected set; }
        public CCKAssetInfo AssetInfo { get; protected set; }

        protected DownloadGithubImageAsyncOperation imageDownloadOp;

        public DownloadGithubAssetPreviewAsyncOperation(CCKAssetInfo assetInfo, int previewIndex)
        {
            AssetInfo = assetInfo;
            PreviewIndex = previewIndex;
            PreviewPath = AssetInfo.GetPreviewAt(PreviewIndex);


            if (null == assetInfo)
            {
                SetError("Asset cannot be null.");
                return;
            }

            if (string.IsNullOrWhiteSpace(PreviewPath))
            {
                SetError($"No preview path found with index {previewIndex}\r\nAsset: {assetInfo.name}\r\nPackage: {assetInfo.packageName}");
                return;
            }

            string pkgUrl = assetInfo?.Parent?.url ?? string.Empty;
            string user = null;
            string repo = null;
            string path = null;
            string branch = null;

            if (!GithubUtility.TryGetInfoFromGithubUrl(pkgUrl, out user, out repo, out path, out branch))
            {
                SetError($"Cannot parse github info from url {pkgUrl}");
                return;
            }

            if (string.IsNullOrWhiteSpace(branch))
            {
                branch = assetInfo.GetInhertValueString(Flags.GitBranch);
            }

            var url = GithubUtility.GenerateGithubContentsUrl(user, repo, branch, path, PreviewPath);

            string authName = assetInfo.GetInhertValueString(Flags.PreviewAuth);
            if (string.IsNullOrWhiteSpace(authName))
            {
                authName = assetInfo.GetInhertValueString(Flags.PkgAuth);
            }

            var token = CCKAuthUtility.GetAuthToken(authName);
            if (string.IsNullOrWhiteSpace(token))
            {
                SetError("No token found by auth name " + authName);
                return;
            }

            imageDownloadOp = new DownloadGithubImageAsyncOperation(url, token);
        }
        protected override IEnumerator OnProcessRoutine()
        {
            yield return imageDownloadOp;
            State = imageDownloadOp.State;
            Error = imageDownloadOp.Error;
            Result = imageDownloadOp.Result;
            IsComplete = true;
        }

        public override void Dispose()
        {
            base.Dispose();
            imageDownloadOp?.Dispose();
        }

        public override List<string> GetExtraInfoStrings()
        {
            var list = base.GetExtraInfoStrings();
            list.Add(AssetInfo.ToText("Asset Info"));
            list.Add(PreviewIndex.ToText("Preview Index"));
            list.Add(PreviewPath.ToText("Preview Path"));
            list.AddRange(imageDownloadOp.ToExtraInfoText("Download Image Operation"));
            return list;
        }
    }
}