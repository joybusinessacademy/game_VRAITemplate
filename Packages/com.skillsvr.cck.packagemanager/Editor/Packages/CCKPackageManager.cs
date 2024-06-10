using SkillsVR.CCK.PackageManager.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace SkillsVR.CCK.PackageManager
{
    public class CCKPackageManager
    {
        public static CCKPackageManager Current { get; } = new CCKPackageManager();


        public IEnumerable<CCKPackageInfo> LoadAllCCKPackageInfo(string fileNamePrefix = "CCKPackageIndex")
        {
            string filter = string.IsNullOrWhiteSpace(fileNamePrefix) ? "CCKPackageIndex" : fileNamePrefix;
            return AssetDatabase.FindAssets("t:" + nameof(TextAsset) + " " + filter).
                Select(g => AssetDatabase.GUIDToAssetPath(g))
                .Select(p => AssetDatabase.LoadAssetAtPath<TextAsset>(p).text)
                .Select(txt => { 
                    try
                    {
                        return JsonUtility.FromJson<CCKPackageManifest>(txt);
                    }
                    catch
                    {
                        return new CCKPackageManifest();
                    }
                })
                .SelectMany(manifest => manifest.packages)
                .Where(pkg => pkg.IsValid());
        }
    }

    public static class PackageCoroutineExtensions
    {
        public static EditorCoroutine StartCoroutine(this IEnumerator routine)
        {
            return EditorCoroutineUtility.StartCoroutineOwnerless(routine);
        }

        public static bool IsEmbed(this PackageInfo packageInfo)
        {
            if (null == packageInfo)
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(packageInfo.resolvedPath))
            {
                return false;
            }
            string packageLocation = packageInfo.resolvedPath.Replace("\\", "/");
            var pkgFolder = Application.dataPath.Replace("\\", "/").Replace("/Assets", "/Package");
            return packageLocation.StartsWith(pkgFolder);
        }
    }
}