using SkillsVR.CCK.PackageManager.AsyncOperation.Networking;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SkillsVR.CCK.PackageManager.AsyncOperation.Github
{

    public class DownloadGithubFileAsyncOperation : DownloadTextFromUrlAsyncOperation
    {
        public GithubApiDownloadFileResp githubRespJson { get; protected set; }
        public string Token { get; protected set; }

        public DownloadGithubFileAsyncOperation(string url, string token, int timeoutInSec = 30): 
            base(url, timeoutInSec, GithubUtility.GenerateGithubApiHeaders(token))
        {
            Token = token;
        }
        
        protected override void ProcessWebRequestData()
        {
            try
            {
                base.ProcessWebRequestData();
                string txt = webRequest.downloadHandler.text;
                if (string.IsNullOrWhiteSpace(txt))
                {
                    SetError("No response received.");
                    return;
                }
                githubRespJson = JsonUtility.FromJson<GithubApiDownloadFileResp>(txt);
                var apiResp = githubRespJson;
                if (null == apiResp)
                {
                    Error = "Cannot parse response object from json: \r\n" + txt;
                    return;
                }
                byte[] data = Convert.FromBase64String(apiResp.content);
                if (null == data || 0 == data.Length)
                {
                    Error = "Only support decode string from base64 format.\r\nCurrent format = "
                        + (string.IsNullOrWhiteSpace(apiResp.type) ? "unknown format" : apiResp.type);
                    return;
                }
                Result = Encoding.UTF8.GetString(data);
                State = OperationState.Success;
            }
            catch (Exception e)
            {
                Debug.Log(e);
                SetError(e);
            }
        }

        public override List<string> GetExtraInfoStrings()
        {
            var list = base.GetExtraInfoStrings();
            list.Add(Token.ToText("Token"));
            list.Add(githubRespJson.ToText("Response"));
            return list;
        }
    }
}