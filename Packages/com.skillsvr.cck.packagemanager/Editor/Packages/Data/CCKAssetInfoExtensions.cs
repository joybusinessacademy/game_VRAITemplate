using System.Linq;

namespace SkillsVR.CCK.PackageManager.Data
{
    public static class CCKAssetInfoExtensions
    {
        public static CCKPackageInfo GetPackageInfo(this CCKAssetInfo info)
        {
            return info.Parent as CCKPackageInfo;
        }
        public static string GetPreviewAt(this CCKAssetInfo info, int index)
        {
            if (null == info
                || null == info.previews)
            {
                return string.Empty;
            }

            var value = info.previews.ElementAtOrDefault(index);
            if (string.IsNullOrWhiteSpace(value))
            {
                value = string.Empty;
            }
            return value;
        }
    }
}