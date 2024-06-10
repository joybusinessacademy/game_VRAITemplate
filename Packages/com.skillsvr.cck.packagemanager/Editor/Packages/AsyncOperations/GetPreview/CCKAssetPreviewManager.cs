using SkillsVR.CCK.PackageManager.AsyncOperation.Networking;
using SkillsVR.CCK.PackageManager.Data;
using System.IO;
using UnityEngine;

namespace SkillsVR.CCK.PackageManager.AsyncOperation.CCKAssetPreview
{
    public class CCKAssetPreviewManager
    {
        public string GetPreivewCacheFilePath(CCKAssetInfo assetInfo, int index)
        {
            string previewPath = assetInfo.GetPreviewAt(index);
            if (string.IsNullOrWhiteSpace(previewPath))
            {
                return null;
            }
            string pkg = assetInfo.Parent.name;
            string file = Path.Combine(pkg, previewPath).Replace("\\", ".").Replace("/", ".");

            var projPath = Application.dataPath.Replace("Assets", "");
            string registry = assetInfo.Parent.Parent.name;
            string fullPath = Path.Combine(projPath, "Library/SkillsVR CCK/Cache/AssetStore/Previews", registry, file);
            fullPath += ".png";
            return fullPath;
        }

        public CustomAsyncOperation<Texture2D> GetCachedPreviewAsync(string path)
        {
            return new GetCachedPreviewAsyncOperation(path);
        }

        public CustomAsyncOperation SetCachedPreviewAsync(string path, Texture2D texture2D)
        {
            return new SetCachedPreviewAsyncOperation(path, texture2D);
        }

        public CustomAsyncOperation<Texture2D> DownloadAssetPreviewAsync(CCKAssetInfo assetInfo, int index = 0)
        {
            string resolverName = assetInfo.GetInhertValueString(Flags.PreviewResolver).ToLower();
            switch(resolverName)
            {
                case "github": return new DownloadGithubAssetPreviewAsyncOperation(assetInfo, index);
                default:  return new DownloadTextureFromUrlAsyncOperation(assetInfo.GetPreviewAt(index));
            }
        }
    }
}