using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace SkillsVR.CCK.PackageManager.AsyncOperation.Networking
{
    public class DownloadTextureFromUrlAsyncOperation : CustomAsyncOperation<Texture2D>, IDisposable
    {
        public string Url { get; protected set; }
        public int TimeoutInSec { get; protected set; }

        protected KeyValuePair<string, string>[] Headers { get; set; }

        protected UnityWebRequest webRequest;

        public DownloadTextureFromUrlAsyncOperation(string url, int timeoutInSeconds, params KeyValuePair<string, string>[] headers)
        {
            Url = url;
            TimeoutInSec = timeoutInSeconds;
            Headers = headers;
        }

        public DownloadTextureFromUrlAsyncOperation(string url, params KeyValuePair<string, string>[] headers)
        {
            Url = url;
            Headers = headers;
            TimeoutInSec = -1;
            
        }
        
        protected override IEnumerator OnProcessRoutine()
        {
            if (string.IsNullOrWhiteSpace(Url))
            {
                SetError("Url cannot be null or empty.");
                yield break;
            }
            webRequest = UnityWebRequestTexture.GetTexture(Url);
            webRequest.timeout = TimeoutInSec > 0 ? TimeoutInSec : webRequest.timeout;
            if (null != Headers)
            {
                foreach (var header in Headers)
                {
                    webRequest.SetRequestHeader(header.Key, header.Value);
                }
            }
            webRequest.SendWebRequest();

            yield return webRequest;

            Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);
            Result = texture;
            if (null == texture)
            {
                SetError("No texture downloaded from url.");
                yield break;
            }
        }

        public override void Dispose()
        {
            if (null == webRequest)
            {
                return;
            }
            base.Dispose();
            if (null != webRequest)
            {
                webRequest.Dispose();

                webRequest = null;
            }
        }
        public override List<string> GetExtraInfoStrings()
        {
            var list = base.GetExtraInfoStrings();
            list.Add(Url.ToText("Url"));
            return list;
        }

        protected override void SetError(string errorMsg)
        {
            Debug.Log("set error texture");
            base.SetError(errorMsg);
        }
    }
}