using SkillsVR.CCK.PackageManager.Managers;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SkillsVR.CCK.PackageManager.AsyncOperation.Registry
{
    public class SaveCachedJsonFromUrlAsync : CustomAsyncOperation
    {
        public string Url { get; protected set; }
        public string CacheFilePath { get; protected set; }
        public bool IsAssetOrPackage { get; protected set; }
        public string Json { get; protected set; }

        public SaveCachedJsonFromUrlAsync(string url, string json)
        {
            Url = url;
            Json = json;
        }

        protected override IEnumerator OnProcessRoutine()
        {
            if (string.IsNullOrWhiteSpace(Json))
            {
                SetError("Json content cannot be null or empty.");
                yield break; 
            }
            
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
                SetError("Cannot save cache file as an asset or package asset.");
                yield break;
            }
            else
            {
                string dir = Path.GetDirectoryName(CacheFilePath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                File.WriteAllText(CacheFilePath, Json);
               // Debug.Log("save cache json to " + CacheFilePath);
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
        
        protected bool SaveToAsset(string assetPath, string json)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            var newTxt = new TextAsset(json);
            var existingTextAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            if (null == existingTextAsset)
            {
                AssetDatabase.CreateAsset(existingTextAsset, assetPath);
            }
            else
            {
                EditorUtility.CopySerialized(newTxt, existingTextAsset);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return true;
        }
    }
}