using SkillsVR.CCK.PackageManager.AsyncOperation.Github;
using SkillsVR.CCK.PackageManager.Data;
using SkillsVR.CCK.PackageManager.Managers;
using SkillsVR.CCK.PackageManager.Resolvers;
using System.Collections;
using UnityEngine;

namespace SkillsVR.CCK.PackageManager.AsyncOperation.Registry
{
    public class DownloadJsonWithUrlAndResolverTypeAsync : CustomAsyncOperation<string>
    {
        public string Url { get; protected set; }
        public string AuthName { get; protected set; }
        public string DownloaderType { get; protected set; }

        public DownloadJsonWithUrlAndResolverTypeAsync(string url, string authName, string downloaderType)
        {
            Url = url;
            AuthName = authName;
            DownloaderType = downloaderType;
        }

        protected override IEnumerator OnProcessRoutine()
        {
            if (string.IsNullOrWhiteSpace(Url))
            {
                SetError("Registry url cannot be null or empty.");
                yield break;
            }
            if (string.IsNullOrWhiteSpace(DownloaderType))
            {
                SetError("Registry downloader type name cannot be null or empty.");
                yield break;
            }

            string authValue = CCKAuthUtility.GetAuthToken(AuthName);

            CustomAsyncOperation<string> downloader = null;

            switch(DownloaderType)
            {
                case RegistrySourceResolverKeys.GithubJson:
                    downloader = new DownloadGithubFileAsyncOperation(Url, authValue);
                    break;
                case RegistrySourceResolverKeys.LocalJson:
                    downloader = new LoadCachedJsonFromUrlAsync(Url);
                break;
            }

            if (null == downloader)
            {
                SetError("Cannt get registry downloader by type: " + DownloaderType);
                yield break;
            }

            yield return downloader;

            Result = downloader.Result;
            if (!UrlCacheUtility.IsAssetPath(Url))
            {
                yield return new SaveCachedJsonFromUrlAsync(Url, Result);

            }
        }
    }
}