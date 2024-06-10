using SkillsVR.CCK.PackageManager.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SkillsVR.CCK.PackageManager.Settings
{

    [Serializable]
    public class CCKPreferenceAuthentications : CustomSettings
    {
        protected static CCKPreferenceAuthentications SettingProviderInstance { get; set; }
        public override string Name { get; protected set; } = "Authentications";
        public override string Path { get; protected set; } = "Preferences/CCK/Authentications";
        public override SettingsScope Scopes { get; protected set; } = SettingsScope.User;
        public override IEnumerable<string> Keywords { get; protected set; } = new HashSet<string>();
        public override string SaveLocation { get; protected set; } = "CCK.Authentications.PreferenceSettings";


        [Serializable]
        public class AuthInfo
        {
            public string name;
            public string auth;
        }

        public List<AuthInfo> CustomAuth = new List<AuthInfo>();

        public static CCKPreferenceAuthentications GetSettings()
        {
            if (null == SettingProviderInstance)
            {
                SettingProviderInstance = ScriptableObject.CreateInstance<CCKPreferenceAuthentications>();
            }
            SettingProviderInstance.Reload();
            return SettingProviderInstance;
        }

        public string FindAuth(Predicate<AuthInfo> predicate)
        {
            if (null == predicate)
            {
                return string.Empty;
            }
            return CustomAuth.FirstOrDefault(x => predicate.Invoke(x))?.auth ?? string.Empty;
        }
        public string GetAuthByName(string authName)
        {
            var match = CustomAuth?.FirstOrDefault(x => x.name == authName);
            if(null == match)
            {
                return null;
            }
            return match.auth;
        }

        [SettingsProvider]
        private static SettingsProvider CreateSettingsProvider()
        {
            return GetSettings().CreateSettingsProvider();
        }

        private void OnEnable()
        {
            Reload();
        }

        public override void Reload()
        {
            this.TryLoadFromEditorPrefs(SaveLocation);
            TryAddDefaultToken();
        }

        private void TryAddDefaultToken()
        {
            if (null == CustomAuth)
            {
                CustomAuth = new List<AuthInfo>();
            }
            if (0 < CustomAuth.Count)
            {
                return;
            }

            var tokenInfo = new AuthInfo();
            tokenInfo.name = AuthKeys.GithubAuthKey;
            tokenInfo.auth = "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";
            CustomAuth.Add(tokenInfo);
        }

        public override bool Save()
        {
            return this.TrySaveAsEditorPrefs(this.SaveLocation);
        }
    }
}