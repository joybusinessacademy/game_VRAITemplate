using System;

namespace SkillsVR.CCK.PackageManager.Data
{
    [Serializable]
    public class CCKRegistryInfo : CCKInfo
    {
        public CCKPackageInfo[] packages;

        public CCKRegistrySourceInfo Source { get; protected set; }
    }
}