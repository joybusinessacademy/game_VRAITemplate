using System;
using System.IO;
using UnityEngine;

namespace SkillsVRNodes.Diagnostics
{
    public class LogFileManagerEditor : LogFileManager
    {
        private const string EDITOR_START_TIME_KEY = "EDITOR_START_TIME_TXT_e9y3bsdf0";

        protected string UnitySrcProjectBasedPersistentDataPath { get; set; }

        public override string GetLogFileDir()
        {
            if (string.IsNullOrWhiteSpace(UnitySrcProjectBasedPersistentDataPath))
            {
                UnitySrcProjectBasedPersistentDataPath = MakeUnityProjectBasedPersistentDataPath();
            }
            return UnitySrcProjectBasedPersistentDataPath;
        }

        protected string MakeUnityProjectBasedPersistentDataPath()
        {
            // Note: the Application.persistentDataPath is a dynamic value based on product name and company name.
            // So switch cck projects in one editor session may cause multiple persistent data folders.
            // Currently editor log is based on editor session.
            // So make the log dir based on project source folder name instead of product name.
            // The output dir may:
            // Windows Editor: %userprofile%\AppData\LocalLow\SkillsVR CCK\<ProjectName>
            // Mac: ~/Library/Application Support/SkillsVR CCK/<ProjectName>

            string name = GetLogFolderName();
            // Windows Editor: %userprofile%\AppData\LocalLow\<companyname>\<productname>
            // Mac: ~/Library/Application Support/<company name>/<product name>
            string path = Application.persistentDataPath.Replace("\\", "/").TrimEnd('/');
            path = Path.GetDirectoryName(path); // Remove product name
            path = Path.GetDirectoryName(path); // Remove company name

            string projectPath = Path.GetDirectoryName(Application.dataPath);
            string projectName = Path.GetFileNameWithoutExtension(projectPath);
            path = Path.Combine(path, "SkillsVR CCK", projectName, name).Replace("/", "\\");
            if (!path.EndsWith("\\"))
            {
                path += "\\";
            }
            return path;
        }

        public override string GetLogFolderName()
        {
            return "EditorLogs";
        }

        public override DateTime GetLogFileStartTime()
        {
#if UNITY_EDITOR
            string timeTxt = UnityEditor.SessionState.GetString(EDITOR_START_TIME_KEY, null);
            if (string.IsNullOrWhiteSpace(timeTxt))
            {
                var time = DateTime.Now;
                timeTxt = time.ToString(GetTimeFormatString());
                UnityEditor.SessionState.SetString(EDITOR_START_TIME_KEY, timeTxt);
                return time;
            }
            else
            {
                if (DateTime.TryParseExact(timeTxt, GetTimeFormatString(), null, System.Globalization.DateTimeStyles.None, out DateTime logFileTime))
                {
                    return logFileTime;
                }
                else
                {
                    return DateTime.Now;
                }
            }
#endif
            return DateTime.Now;
        }
    }
}

