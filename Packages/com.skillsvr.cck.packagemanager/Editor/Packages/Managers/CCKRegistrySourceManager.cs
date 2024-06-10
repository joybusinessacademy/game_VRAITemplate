using SkillsVR.CCK.PackageManager.AsyncOperation;
using SkillsVR.CCK.PackageManager.AsyncOperation.Registry;
using SkillsVR.CCK.PackageManager.AsyncOperations;
using SkillsVR.CCK.PackageManager.Data;
using SkillsVR.CCK.PackageManager.Settings;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SkillsVR.CCK.PackageManager.Managers
{
    public class CCKRegistrySourceManager
    {
        [InitializeOnLoadMethod]
        static void InitReigstryManager()
        {
            var items = CCKProjectSettingsPackageManager.GetSettings().registries;
            foreach(var item in items)
            {
                var reg = CCKRegistryManager.Main.AddRegistry (item);
            }
        }

        public IEnumerable<string> GetAllRegisteryNames()
        {
            return CCKProjectSettingsPackageManager.GetSettings().registries.Select(src => src.name);
        }

        public CCKRegistrySourceInfo GetRegistrySource(string name)
        {
            return CCKProjectSettingsPackageManager.GetSettings().registries.FirstOrDefault(x => null != x && x.name == name);
        }
    }
}