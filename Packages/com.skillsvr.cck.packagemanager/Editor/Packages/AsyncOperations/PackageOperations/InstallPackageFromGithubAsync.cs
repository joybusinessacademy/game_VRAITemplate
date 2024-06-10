using SkillsVR.CCK.PackageManager.AsyncOperation.Github;
using SkillsVR.CCK.PackageManager.Data;
using SkillsVR.CCK.PackageManager.Settings;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace SkillsVR.CCK.PackageManager.AsyncOperation.PackageOperations
{
    public class InstallPackageFromGithubAsync: CustomAsyncOperation<AddRequest>
    {
        public CCKPackageInfo PackageInfo { get; protected set; }

        public InstallPackageFromGithubAsync(CCKPackageInfo package)
        {
            PackageInfo = package;
        }


        // 1. Get auth token from preference authentications by the name of PkgAuth.
        // 2. Insert token into package url
        // 3. If raw url has branch info (end with #branchName), use the url branch.
        // 4. If raw url not has branch info, check and try add package branch info from GitBranch flag.
        protected override IEnumerator OnProcessRoutine()
        {
            if (null == PackageInfo)
            {
                SetError("Package to installed is null.");
                yield break;
            }

            var authName = PackageInfo.GetInhertValueString(Flags.PkgAuth);
            if (string.IsNullOrWhiteSpace(authName))
            {
                SetError("Package auth is required.");
                yield break;
            }
            var authValue = CCKPreferenceAuthentications.GetSettings().GetAuthByName(AuthKeys.GithubAuthKey);

            if (string.IsNullOrWhiteSpace(authValue))
            {
                SetError("No auth found by name " + authName + ". \r\n" +
                    "Please add auth token or password by follow steps:\r\n" +
                    "  1. Open Preferences settings from \"Edit -> Preferences...\";\r\n" +
                    "  2. Find the CCK/Authentications;\r\n" +
                    "  3. In the \"Custom Auth\" list, click the \"+\" button at below;\r\n" +
                    $"  4. In the \"Name\" field, add \"{authName}\"\r\n" +
                    "  5. In the \"Auth\" field , add your token or password.\r\n" +
                    "  6. Save your auth by press ctrl/command + s, then close the preferences window.\r\n" +
                    "  7. Retry install.");
                yield break;
            }

            var url = PackageInfo.url;
            if (string.IsNullOrWhiteSpace(url))
            {
                SetError("Github url is required. The url format should be like:\r\n" +
                    "git+https://github.com/owner/repo.git");
                yield break;
            }

            if (!url.StartsWith("git+https://github.com/"))
            {
                SetError("Github url should be formatted as: " +
                    "git+https://github.com/owner/repo.git");
                yield break;
            }

            string githubAuthUrl = url.Replace("git+https://github.com/", $"git+https://x-oauth-basic:{authValue}@github.com/");

            string existingBranch = "";
            GithubUtility.TryGetInfoFromGithubUrl(githubAuthUrl, out _, out _, out _, out existingBranch);

            if (string.IsNullOrWhiteSpace(existingBranch))
            {
                string settinggBranch = PackageInfo.GetInhertValueString(Flags.GitBranch);
                if (!string.IsNullOrWhiteSpace(settinggBranch))
                {
                    githubAuthUrl += "#" + settinggBranch;
                }
            }

            var op = new QueuedClientRequest<AddRequest>(() => Client.Add(githubAuthUrl));
            yield return op;

            TrySetErrorFrom(op);
            Result = op.Result;
        }

        protected override void SetError(string errorMsg)
        {
            errorMsg = "Install Package from Github Fail: " + errorMsg;
            base.SetError(errorMsg);
        }

        public override List<string> GetExtraInfoStrings()
        {
            var list = base.GetExtraInfoStrings();
            list.Add(PackageInfo.ToText("Package"));
            return list;
        }
    }
}