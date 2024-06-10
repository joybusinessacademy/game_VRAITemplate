using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SkillsVRNodes.Managers.Utility
{
    public static class ScriptableObjectExtension
    {
        public static void Save(this ScriptableObject scriptableObject)
        {
            if (null == scriptableObject)
            {
                return;
            }

            EditorUtility.CopySerialized(scriptableObject, scriptableObject);
            AssetDatabase.SaveAssetIfDirty(scriptableObject);
        }

        public static void SaveAsAsset(this ScriptableObject scriptableObject, string assetPath)
        {
            if (null == scriptableObject)
            {
                return;
            }

            assetPath = assetPath.Replace("\\", "/");
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (!string.IsNullOrWhiteSpace(guid))
            {
                EditorUtility.CopySerialized(scriptableObject, scriptableObject);
            }
            else
            {
                var path = assetPath.Split("/", StringSplitOptions.RemoveEmptyEntries);
                string parentDir = path.First();
                for (int i = 1; i < path.Length - 1; i++)
                {
                    string currDirName = path[i];
                    string dir = Path.Combine(parentDir, currDirName);
                    if (!AssetDatabase.IsValidFolder(dir))
                    {
                        AssetDatabase.CreateFolder(parentDir, currDirName);
                    }
                    parentDir = dir;
                }

                AssetDatabase.CreateAsset(scriptableObject, assetPath);
                AssetDatabase.SaveAssets();
            }

            AssetDatabase.SaveAssets();
        }
    }
}