using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SkillsVR.CCK.PackageManager.Settings
{

    public abstract class CustomSettings : ScriptableObject
    {
        public abstract string Name { get; protected set; }
        public abstract string Path { get; protected set; }
        public abstract SettingsScope Scopes { get; protected set; }
        public abstract IEnumerable<string> Keywords { get; protected set; }
        public abstract string SaveLocation { get; protected set; }

        public abstract bool Save();
        public abstract void Reload();
    }
}