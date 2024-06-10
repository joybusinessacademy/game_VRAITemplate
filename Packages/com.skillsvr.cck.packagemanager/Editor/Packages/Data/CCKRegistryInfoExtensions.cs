using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkillsVR.CCK.PackageManager.Data
{
    public static class CCKRegistryInfoExtensions
    {
        public static IEnumerable<TreeViewItemData> FetchAllCategoryData(this CCKRegistryInfo registryInfo)
        {
            if (null == registryInfo || null == registryInfo.packages)
            {
                return new List<TreeViewItemData>();
            }
            var allCategories = registryInfo.packages
               .Where(pkg => null != pkg.assets)
               .SelectMany(pkg => pkg.assets)
               .Where(asset => null != asset && !string.IsNullOrWhiteSpace(asset.category))
               .Select(asset => asset.category);

            Dictionary<string, TreeViewItemData> managedData = new Dictionary<string, TreeViewItemData>();

            foreach (var category in allCategories)
            {
                var fullName = category.Replace("\\", "/");
                var parts = fullName.Split("/");
                var currNameKey = "";
                foreach (var name in parts)
                {
                    currNameKey += "/" + name;
                    currNameKey = currNameKey.TrimStart('/');

                    if (managedData.ContainsKey(currNameKey))
                    {
                        var data = managedData[currNameKey];
                        data.count++;
                    }
                    else
                    {
                        var data = new TreeViewItemData();
                        data.name = name;
                        data.fullName = currNameKey;
                        data.count = 1;
                        managedData.Add(currNameKey, data);
                    }
                }
            }

            return managedData.Values.OrderBy(x => x.fullName).ToList();
        }

        public static bool LoadFromJson(this CCKRegistryInfo registryInfo, string json)
        {
            if (null == registryInfo)
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }
            JsonUtility.FromJsonOverwrite(json, registryInfo);

            var registryAuthor = registryInfo.author;

            if (null == registryInfo.packages)
            {
                return false;
            }
            foreach(var pkg in registryInfo.packages)
            {
                if (null == pkg)
                {
                    continue;
                }
                pkg.Parent = registryInfo;
                if (pkg.author.IsEmpty())
                {
                    pkg.author = registryAuthor;
                }

                if (null == pkg.assets)
                {
                    continue;
                }
                foreach(var asset in pkg.assets)
                {
                    if (null == asset)
                    {
                        continue;
                    }
                    asset.Parent = pkg;
                    if (asset.author.IsEmpty())
                    {
                        asset.author = pkg.author;
                    }
                    asset.packageName = pkg.name;
                    if (string.IsNullOrWhiteSpace(asset.version))
                    {
                        asset.version = pkg.version;
                    }
                }
            }
            return true;
        }
    }
}