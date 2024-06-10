using System;
using System.Collections.Generic;
namespace SkillsVR.CCK.PackageManager.AsyncOperation.Github
{
    public class Links
    {
        public string git;
        public string html;
        public string self;
    }

    public class GithubApiDownloadFileResp
    {
        public string type;
        public int size;
        public string name;
        public string path;
        public string content;
        public string sha;
        public string url;
        public string git_url;
        public string html_url;
        public string download_url;
        public Links _links;
    }
    public class Root
    {
        public string type;
        public int size;
        public string name;
        public string path;
        public string sha;
        public string url;
        public string git_url;
        public string html_url;
        public string download_url;
        public List<GithubApiDownloadFileResp> entries;
        public Links _links;
    }
}