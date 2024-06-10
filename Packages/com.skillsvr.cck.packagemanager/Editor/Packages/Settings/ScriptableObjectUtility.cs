using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVR.CCK.PackageManager.Settings
{
    public static class ScriptableObjectUtility
    {
        public static bool TryLoadFrom(this ScriptableObject scriptable, string projectRelativePath)
        {
            try
            {
                var projectPath = Path.GetDirectoryName(Application.dataPath);
                string fullpath = Path.Combine(projectPath, projectRelativePath);
                if (!File.Exists(fullpath))
                {
                    return false;
                }
                var json = File.ReadAllText(fullpath);
                JsonUtility.FromJsonOverwrite(json, scriptable);
                return true;
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }
            return false;
        }

        public static bool TrySaveTo(this ScriptableObject scriptable, string projectRelativePath)
        {
            try
            {
                var json = JsonUtility.ToJson(scriptable, true);
                var projectPath = Path.GetDirectoryName(Application.dataPath);
                string fullpath = Path.Combine(projectPath, projectRelativePath);
                File.WriteAllText(fullpath, json);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            return false;
        }


        public static bool TrySaveAsEditorPrefs(this ScriptableObject scriptable, string key)
        {
            if (null == scriptable
               || string.IsNullOrWhiteSpace(key))
            {
                return false;
            }
            try
            {
                string json = JsonUtility.ToJson(scriptable);
                EditorPrefs.SetString(key, json);
                return true;
            }
            catch(Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        public static bool TryLoadFromEditorPrefs(this ScriptableObject scriptable, string key)
        {
            if (null == scriptable
                || string.IsNullOrWhiteSpace(key))
            {
                return false;
            }
            string json = EditorPrefs.GetString(key, string.Empty);
            if (string.IsNullOrEmpty(json))
            {
                return false;
            }
            try
            {
                JsonUtility.FromJsonOverwrite(json, scriptable);
                return true;
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }
            return false;
        }

        public static SerializedObject CreateSerializedObject(this ScriptableObject scriptable)
        {
            return new SerializedObject(scriptable);
        }

        
    }
}