using System;

namespace SkillsVR.CCK.PackageManager.Data
{
    [Serializable]
    public class CCKInfo
    {
        public string displayName;
        public string name;
        public string version;
        public string url;
        public string description;
        public string type;
        public string[] flags;
        public string unity;
        public string unityRelease;
        public CCKAuthorInfo author = new CCKAuthorInfo();

        public CCKInfo Parent { get; set; }
    }
}