using System;
using System.Collections.Generic;

namespace SkillsVR.CCK.PackageManager.Data
{
    [Serializable]
    public class CCKPackageManifest
    {
        public List<CCKPackageInfo> packages = new List<CCKPackageInfo>();
    }
}