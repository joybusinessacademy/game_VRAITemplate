using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace SkillsVR.CCK.PackageManager.AsyncOperation.CCKAssetPreview
{
    public class GetCachedPreviewAsyncOperation : CustomAsyncOperation<Texture2D>
    {
        public string CacheFilePath { get; protected set; }

        protected Task<byte[]> task;

        public GetCachedPreviewAsyncOperation(string path)
        {
            CacheFilePath = path;
            MoveNext();
        }

        protected override IEnumerator OnProcessRoutine()
        {
            if (string.IsNullOrWhiteSpace(CacheFilePath))
            {
                SetError("No cache file path found");
                yield break;
            }

            string dir = Path.GetDirectoryName(CacheFilePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            try
            {
                task = File.ReadAllBytesAsync(CacheFilePath);
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

            Texture2D img = new Texture2D(2, 2);
            img.LoadImage(task.Result);
            Result = img;
        }

        public override List<string> GetExtraInfoStrings()
        {
            var list = base.GetExtraInfoStrings();
            list.Add(CacheFilePath.ToText("File Path"));
            return list;
        }
    }
}