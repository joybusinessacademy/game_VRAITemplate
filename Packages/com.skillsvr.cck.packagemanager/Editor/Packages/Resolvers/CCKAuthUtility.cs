using SkillsVR.CCK.PackageManager.Settings;
using UnityEngine;

namespace SkillsVR.CCK.PackageManager.Resolvers
{
    public static class CCKAuthUtility
    {
        public static string GetAuthToken(string authName)
        {
            string token = null;
            TryGetAuthTokenString(authName, out token);
            return token;
        }

        public static bool TryGetAuthTokenString(string authName, out string token)
        {
            token = null;
            if (string.IsNullOrWhiteSpace(authName))
            {
                return false;
            }

            var setting = CCKPreferenceAuthentications.GetSettings();
            token = setting.GetAuthByName(authName);
            return !string.IsNullOrWhiteSpace(token);
        }
    }
}