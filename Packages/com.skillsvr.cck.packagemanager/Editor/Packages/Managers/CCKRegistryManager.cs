using SkillsVR.CCK.PackageManager.AsyncOperation.Registry;
using SkillsVR.CCK.PackageManager.Data;
using SkillsVR.CCK.PackageManager.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SkillsVR.CCK.PackageManager.Managers
{
    // Manage all registries
    public class CCKRegistryManager
    {
        public static CCKRegistryManager Main { get; } = new CCKRegistryManager();

        private const string LAST_ACTIVITY_REGISTRY_NAME_KEY = "cck.packagemanager.activeregistry.e83w";

        public CCKRegistry ActiveRegistry { get; protected set; }
        public IEnumerable<CCKRegistry> Registries => ManagedRegistries;
        protected HashSet<CCKRegistry> ManagedRegistries { get; } = new HashSet<CCKRegistry>();


        public void SetActiveRegistry(CCKRegistry registry)
        {
            ActiveRegistry = registry;
            var name = null == ActiveRegistry || string.IsNullOrWhiteSpace(ActiveRegistry.name) ? "" : ActiveRegistry.name;
            EditorPrefs.SetString(LAST_ACTIVITY_REGISTRY_NAME_KEY, name);
            Debug.Log("Set main registry " + name);
        }

        public CCKRegistry TryRestoreLastActivityRegistry()
        {
            var last = EditorPrefs.GetString(LAST_ACTIVITY_REGISTRY_NAME_KEY, null);
            if(string.IsNullOrWhiteSpace(last))
            {
                return ActiveRegistry;
            }
            var registry = ManagedRegistries.FirstOrDefault(x => x.name == last);
            if (null == registry)
            {
                ActiveRegistry = null;
            }

            ActiveRegistry = registry;
            return ActiveRegistry;
        }

        public CCKRegistry GetRegistry(Predicate<CCKRegistry> predicate)
        {
            if (null == predicate)
            {
                return null;
            }
            return ManagedRegistries.FirstOrDefault(x => null != x && predicate.Invoke(x));
        }

        public CCKRegistry GetRegistry(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }
            return ManagedRegistries.FirstOrDefault(x=> null != x && x.name == name);
        }

        public CCKRegistry GetRegistry(CCKRegistrySourceInfo source)
        {
            if (null == source
               || string.IsNullOrWhiteSpace(source.url))
            {
                return null;
            }

            var existing = ManagedRegistries.FirstOrDefault(x => null != x && null != x.Source && x.Source.url == source.url);
            return existing;
        }


        public CCKRegistry AddRegistry(CCKRegistrySourceInfo source)
        {
            if (null == source || string.IsNullOrWhiteSpace(source.url))
            {
                return null;
            }

            var existing = GetRegistry(source);
            if (null != existing)
            {
                return existing;
            }
            
            var item = new CCKRegistry(source);
            ManagedRegistries.Add(item);
            return item;
        }
    }
}