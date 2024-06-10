using System;

namespace SkillsVR.CCK.PackageManager.Data
{
    [Serializable]
    public class CCKAuthorInfo
    {
        public string name;
        public string email;
        public string url;
    }

    public static class CCKAuthInfoExtensions
    {
        public static bool IsEmpty(this CCKAuthorInfo author)
        {
            if (null == author)
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(author.name)
                && string.IsNullOrWhiteSpace(author.url)
                && string.IsNullOrWhiteSpace(author.email))
            {
                return true;
            }

            return false;
        }
    }
}