using SkillsVR.CCK.PackageManager.Managers;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SkillsVR.CCK.PackageManager.AsyncOperation.Registry
{
    // Load json string by url
    // If url is asset or package file path (start with Assets/ or Packages/), load the text asset;
    // Otherwise convert url to cache file name and read text.
    public class LoadCachedJsonFromUrlAsync: CustomAsyncOperation<string>
    {
        public string Url { get; protected set; }
        public string CacheFilePath { get; protected set; }
        public bool IsAssetOrPackage { get; protected set; }

        public LoadCachedJsonFromUrlAsync(string url)
        {
            Url = url;
        }

        protected override IEnumerator OnProcessRoutine()
        {
            if (string.IsNullOrWhiteSpace(Url))
            {
                SetError("Url cannot be null or empty.");
                yield break;
            }
            CacheFilePath = UrlCacheUtility.UrlToCacheFilePath(Url);

            if (string.IsNullOrWhiteSpace(CacheFilePath))
            {
                SetError("Cannot get cache file path from url.");
                yield break;
            }

            IsAssetOrPackage = UrlCacheUtility.IsAssetPath(CacheFilePath);

            if (IsAssetOrPackage)
            {
                Result = LoadFromAsset(CacheFilePath);
            }
            else
            {
                Result = LoadFile(CacheFilePath);
                yield break;
            }
        }

        public override List<string> GetExtraInfoStrings()
        {
            var list = base.GetExtraInfoStrings();
            list.Add(Url.ToText("Url"));
            list.Add(CacheFilePath.ToText("Cache File Path"));
            list.Add(IsAssetOrPackage.ToText("Is Asset Or Package"));
            return list;
        }
        
        protected string LoadFromAsset(string assetPath)
        {
            var txtAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            if (null == txtAsset)
            {
                SetError("Cached file asset not found.");
                return string.Empty;
            }
            return txtAsset.text;
        }

        protected string LoadFile(string fileFullPath)
        {

            if (!File.Exists(fileFullPath))
            {
                SetError("Cached file not found.");
                return string.Empty;
            }
            return File.ReadAllText(fileFullPath);
        }
    }
}