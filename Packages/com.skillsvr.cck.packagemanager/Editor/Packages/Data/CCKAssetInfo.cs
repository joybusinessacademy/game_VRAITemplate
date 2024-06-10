using System;

namespace SkillsVR.CCK.PackageManager.Data
{
    [Serializable]
    public class CCKAssetInfo : CCKInfo
    {
        public string[] previews;
        public string category;
        public string packageName;
    }
}