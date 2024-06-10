using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace SkillsVR.CCK.PackageManager.AsyncOperation.CCKAssetPreview
{
    public class SetCachedPreviewAsyncOperation : CustomAsyncOperation
    {
        public string CacheFilePath { get; protected set; }
        public Texture2D TextureToSave { get; protected set; }

        public byte[] Data { get; protected set; }

        protected Task task;
        public SetCachedPreviewAsyncOperation(string path, Texture2D texture2D)
        {
            CacheFilePath = path;
            TextureToSave = texture2D;
        }
        protected override IEnumerator OnProcessRoutine()
        {
            if (string.IsNullOrWhiteSpace(CacheFilePath))
            {
                SetError("No cache file path found");
                yield break ;
            }

            if (null == TextureToSave)
            {
                SetError("Texture cannot be null.");
                yield break;
            }

            Data = TextureToSave.EncodeToPNG();

            string dir = Path.GetDirectoryName(CacheFilePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            try
            {
                task = File.WriteAllBytesAsync(CacheFilePath, Data);
            }
            catch (Exception e)
            {
                SetError(e.Message + "\r\n" + e.StackTrace);
                yield break;
            }

            while (null != task && !task.IsCompleted && !task.IsCanceled)
            {
                yield return null;
            }

            if (task.IsCanceled)
            {
                SetError("Operation cancelled.");
                yield break;
            }
            if (null != task.Exception)
            {
                SetError(task.Exception.Message + "\r\n" + task.Exception.StackTrace);
                yield break;
            }

            if (!task.IsCompletedSuccessfully)
            {
                SetError("Operation unsuccessful.");
                yield break;
            }
        }

        public override List<string> GetExtraInfoStrings()
        {
            var list = base.GetExtraInfoStrings();
            list.Add(TextureToSave.ToText("Texture"));
            list.Add(CacheFilePath.ToText("Cache File Path"));
            return list;
        }
    }
}