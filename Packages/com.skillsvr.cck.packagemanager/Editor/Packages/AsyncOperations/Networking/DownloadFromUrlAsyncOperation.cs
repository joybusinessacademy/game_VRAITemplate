using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace SkillsVR.CCK.PackageManager.AsyncOperation.Networking
{
    public class DownloadFromUrlAsyncOperation : CustomAsyncOperation<byte[]>, IDisposable
    {
        public string ResultString { get; protected set; }
        public string Url { get; protected set; }
        public int TimeoutInSec { get; protected set; }

        protected KeyValuePair<string, string>[] Headers { get; set; }

        public UnityWebRequest webRequest { get; protected set; }

        public DownloadFromUrlAsyncOperation(string url, int timeoutInSeconds, params KeyValuePair<string,string>[] headers)
        {
            Url = url;
            TimeoutInSec = timeoutInSeconds;
            Headers = headers;
        }

        protected override IEnumerator OnProcessRoutine()
        {
            webRequest = UnityWebRequest.Get(Url);
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
            if (!string.IsNullOrWhiteSpace(webRequest.error))
            {
                SetError(webRequest.error);
                yield break;
            }
            ProcessWebRequestData();
        }

        protected virtual void ProcessWebRequestData()
        {
            try
            {
                Result = webRequest.downloadHandler.data;
                ResultString = webRequest.downloadHandler.text;
                State = OperationState.Success;
            }
            catch (Exception e)
            {
                SetError(e.Message + "\r\n" + e.StackTrace);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            webRequest?.Dispose();
        }
        public override List<string> GetExtraInfoStrings()
        {
            var list = base.GetExtraInfoStrings();
            list.Add(Url.ToText("Url"));
            return list;
        }
    }
}