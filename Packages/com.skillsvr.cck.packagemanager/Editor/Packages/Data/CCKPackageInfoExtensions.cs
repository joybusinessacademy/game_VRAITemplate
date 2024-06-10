using SkillsVR.CCK.PackageManager.Managers;
using UnityEditor.PackageManager;

namespace SkillsVR.CCK.PackageManager.Data
{
    public static class CCKPackageInfoExtensions
    {
        public static PackageInfo GetUnityPackageInfo(this CCKPackageInfo package)
        {
            if (null == package || string.IsNullOrWhiteSpace(package.name))
            {
                return null;
            }
            return LocalPackageManager.Current.Get(package.name);
        }
        public static bool IsInstalled(this CCKPackageInfo package)
        {
            if (null == package || string.IsNullOrWhiteSpace(package.name))
            {
                return false;
            }
            return LocalPackageManager.Current.IsInstalled(package.name);
        }

        public static string GetInstalledVersion(this CCKPackageInfo package)
        {
            if (null == package || string.IsNullOrWhiteSpace(package.name))
            {
                return string.Empty;
            }
            return LocalPackageManager.Current.GetVersion(package.name);
        }

        public static bool IsEmbed(this CCKPackageInfo package)
        {
            if (null == package || string.IsNullOrWhiteSpace(package.name))
            {
                return false;
            }
            return LocalPackageManager.Current.IsEmbed(package.name);
        }

        public static CCKPackageOperationState GetOperationState(this CCKPackageInfo package)
        {
            return CCKPackageOperationQueue.MainQueue.GetPackageState(package.GetFullId());
        }

        public static void RequestInstall(this CCKPackageInfo package)
        {
            CCKPackageOperationQueue.MainQueue.Install(package.GetFullId());
        }

        public static void RequestUninstall(this CCKPackageInfo package)
        {
            CCKPackageOperationQueue.MainQueue.Uninstall(package.GetFullId());
        }

        public static string GetFullId(this CCKPackageInfo package)
        {
            if (null == package)
            {
                return "";
            }

            string id = "";
            if (null != package.Parent && !string.IsNullOrWhiteSpace(package.Parent.name))
            {
                id += package.name + ".";
            }
            id += string.IsNullOrWhiteSpace(package.name) ? "null" : package.name;
                
            return id;
        }
        public static bool IsValid(this CCKPackageInfo package)
        {
            if (null == package)
            {
                return false;
            }

            return !string.IsNullOrWhiteSpace(package.url) 
                && !string.IsNullOrWhiteSpace(package.name) 
                && !string.IsNullOrWhiteSpace(package.version);
        }
    }

}