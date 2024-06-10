using System;

namespace SkillsVR.CCK.PackageManager.Data
{
    /// <summary>
    /// Json file that contains all individual cck assets in the package.
    /// Located at same level with package.json file.
    /// </summary>
    [Serializable]
    public class CCKPackageInfo : CCKInfo
    {
        public CCKAssetInfo[] assets;

        public override string ToString()
        {
            return string.Join("\r\n",
                $"{nameof(name)}: {(string.IsNullOrWhiteSpace(name) ? "null" : name)}",
                $"{nameof(displayName)}: {(string.IsNullOrWhiteSpace(displayName) ? "null" : displayName)}",
                $"{nameof(version)}: {(string.IsNullOrWhiteSpace(version) ? "null" : version)}",
                $"{nameof(url)}: {(string.IsNullOrWhiteSpace(url) ? "null" : url)}",
                $"{nameof(flags)}: {(null == flags ? "null" : string.Join(", ", flags))}"
                );
        }
    }

}