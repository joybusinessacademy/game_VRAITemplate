using System.Collections.Generic;
using System.IO;

namespace SkillsVR.CCK.PackageManager.AsyncOperation.Github
{
    public static class GithubUtility
    {
        public static KeyValuePair<string, string>[] GenerateGithubApiHeaders(string token)
        {
            return new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("Authorization", $"Bearer {token}"),
                new KeyValuePair<string, string>("Accept", "application/vnd.github+json"),
                new KeyValuePair<string, string>("X-GitHub-Api-Version", "2022-11-28")
            };
        }
        public static string GenerateGithubContentsUrl(string user, string repo, string branch, params string[] pathSections)
        {
            var url = Path.Combine("https://api.github.com/repos", user, repo, "contents");
            if (null != pathSections)
            {
                foreach(var path in pathSections)
                {
                    if (string.IsNullOrWhiteSpace(path))
                    {
                        continue;
                    }
                    url = Path.Combine(url, path);
                }
            }
            if (!string.IsNullOrWhiteSpace(branch))
            {
                url += $"?ref={branch}";
            }

            url = url.Replace("\\", "/");
            return url;
        }

        public static bool TryGetInfoFromGithubUrl(string url, out string user, out string repo, out string path, out string branch)
        {
            user = string.Empty;
            repo = string.Empty;
            path = string.Empty;
            branch = string.Empty;

            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            url = url.Replace("\\", "/");

            string githubHost = "github.com/";
            var index = url.IndexOf(githubHost);
            if (index < 0)
            {
                return false;
            }

            url = url.Substring(index + githubHost.Length);
            var userIndex = url.IndexOf('/');
            user = url.Substring(0, userIndex);
            url = url.Substring(userIndex + 1);

            var repoIndex = url.IndexOf(".git");
            repo = url.Substring(0, repoIndex);
            url = url.Substring(repoIndex + 4);

            var branchIndex = url.LastIndexOf('#');
            if (branchIndex >= 0)
            {
                branch = url.Substring(branchIndex + 1);
                url = url.Substring(0, branchIndex);
            }
            
            var pathIndex = url.IndexOf("path=");
            if (pathIndex >= 0)
            {
                path = url.Substring(pathIndex + 5);
                path = path.TrimStart('/');
            }
            return true;
        }
    }
}