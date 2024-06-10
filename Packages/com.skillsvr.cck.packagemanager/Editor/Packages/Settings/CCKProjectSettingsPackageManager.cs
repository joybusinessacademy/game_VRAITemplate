using SkillsVR.CCK.PackageManager.Data;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SkillsVR.CCK.PackageManager.Settings
{
    [Serializable]
    public class CCKProjectSettingsPackageManager : CustomSettings
    {
        protected static CCKProjectSettingsPackageManager SettingProviderInstance { get; set; }

        public override string Name { get; protected set; } = "Package Manager";
        public override string Path { get; protected set; } = "Project/CCK/Package Manager";
        public override SettingsScope Scopes { get; protected set; } = SettingsScope.Project;
        public override IEnumerable<string> Keywords { get; protected set; } = new HashSet<string>();
        public override string SaveLocation { get; protected set; } = "ProjectSettings/CCKPackageManagerSettings.asset";


        
        public List<CCKRegistrySourceInfo> registries = new List<CCKRegistrySourceInfo>();

        public static CCKProjectSettingsPackageManager GetSettings()
        {
            if (null == SettingProviderInstance)
            {
                SettingProviderInstance = ScriptableObject.CreateInstance<CCKProjectSettingsPackageManager>();
            }
            SettingProviderInstance.Reload();
            return SettingProviderInstance;
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
            this.TryLoadFrom(SaveLocation);
        }
        public override bool Save()
        {
            return this.TrySaveTo(this.SaveLocation);
        }


        void OnValidate()
        {
            ResolveRegistryNames();
        }

        protected bool ResolveRegistryNames()
        {
            Dictionary<string, int> nameCount = new Dictionary<string, int>();
            bool modified = false;
            foreach(var re in registries)
            {
                if (null == re)
                {
                    continue;
                }
                if (string.IsNullOrWhiteSpace(re.name))
                {
                    re.name = "Registry";
                }

                string cleanName = re.name.TrimStart(' ').TrimEnd(' ');

                if (nameCount.ContainsKey(cleanName))
                {
                    nameCount[cleanName]++;
                    re.name = $"{cleanName} ({nameCount[cleanName]})";
                    modified = true;
                }
                else
                {
                    nameCount.Add(cleanName, 1);
                }
            }

            return modified;
        }
    }
}