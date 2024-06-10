using SkillsVR.CCK.PackageManager.AsyncOperation.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillsVR.CCK.PackageManager.AsyncOperation.Github
{
    public class DownloadGithubImageAsyncOperation : CustomAsyncOperation<Texture2D>
    {
        public DownloadGithubFileAsyncOperation downloadFileInfoOp { get; protected set; }
        public DownloadTextureFromUrlAsyncOperation downloadImageOp { get; protected set; }

        public string Url { get; protected set; }
        public string Token { get; protected set; }
        public int Timeout { get; protected set; }

        public string AssetUrl { get; protected set; }
        public DownloadGithubImageAsyncOperation(string url, string token, int timeoutInSec = 30)
        {
            Url = url;
            Token = token;
            Timeout = timeoutInSec;
        }

        protected override IEnumerator OnProcessRoutine()
        {
            downloadFileInfoOp = new DownloadGithubFileAsyncOperation(Url, Token, Timeout);
            yield return downloadFileInfoOp;
            AssetUrl = downloadFileInfoOp.githubRespJson.download_url;
            downloadImageOp = new DownloadTextureFromUrlAsyncOperation(AssetUrl);
            yield return downloadImageOp;
            Result = downloadImageOp.Result;
            
        }

        public override void Dispose()
        {
            base.Dispose();
            downloadFileInfoOp?.Dispose();
            downloadImageOp?.Dispose();
        }

        public override List<string> GetExtraInfoStrings()
        {
            var list = base.GetExtraInfoStrings();
            list.Add(Url.ToText("Url"));
            list.Add(Token.ToText("Token"));
            list.Add(AssetUrl.ToText("Asset Url"));
            list.Add("Download File Operation:");
            list.AddRange(downloadFileInfoOp.ToExtraInfoText("Download File Op"));
            list.Add("Download Image Operation");
            list.AddRange(downloadImageOp.ToExtraInfoText("Download Image Op"));
            return list;
        }
    }
}