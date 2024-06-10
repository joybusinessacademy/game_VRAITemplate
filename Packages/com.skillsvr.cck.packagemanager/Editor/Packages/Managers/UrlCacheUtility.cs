using UnityEngine;

namespace SkillsVR.CCK.PackageManager.Managers
{
    public class UrlCacheUtility
    {
        /// <summary>
        /// Convert url to cache file path.
        /// If url is asset or package asset path (start with Assets/ or Packages/), no changes.
        /// Otherwise will be "Library/SkillsVR CCK/Cache/PackageRegistries/url_replace_dot_to_underline.json".
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string UrlToCacheFilePath(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return string.Empty;
            }
            if (IsAssetPath(url))
            {
                return url;
            }
            else
            {
                return UrlToLibraryCacheFilePath(url);
            }
        }

        public static bool IsAssetPath(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }
            url = url.Replace("\\", "/");
            if (url.StartsWith("Assets/")
                 || url.StartsWith("Packages/"))
            {
                return true;
            }
            return false;
        }

        protected static string UrlToLibraryCacheFilePath(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return string.Empty;
            }
            url = url.Replace("\\", "/");
            string path = Application.dataPath.Replace("/Assets", "/Library/") + "SkillsVR CCK/Cache/PackageRegistries/";
            int index = url.IndexOf("//");
            if (index >= 0)
            {
                url = url.Substring(index + 2);
            }
            index = url.IndexOf('?');
            if (index > 0)
            {
                url = url.Substring(0, index);
            }
            url = url.Replace("/", "_");
            if (!url.EndsWith(".json"))
            {
                url += ".json";
            }
            string filePath = path + url;
            return filePath;
        }
    }
}