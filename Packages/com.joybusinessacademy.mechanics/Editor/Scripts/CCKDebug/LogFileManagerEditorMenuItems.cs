using System.IO;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;

namespace SkillsVRNodes.Diagnostics
{
    internal class LogFileManagerEditorMenuItems
    {
        [MenuItem("SkillsVR CCK/Logs/Open Editor Logs Folder")]
        private static void OpenLogFolder()
        {
            string dir =LogFileManager.Main.GetLogFileDir();

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var item = Directory.GetFiles(dir).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(item))
            {
                item = Directory.GetDirectories(dir).FirstOrDefault();
            }

            var path = string.IsNullOrWhiteSpace(item) ? dir : item;

            EditorUtility.RevealInFinder(path);
        }

        [MenuItem("SkillsVR CCK/Logs/Remove Old Log Files")]
        private static void RemoveOldLogFiles()
        {
            LogFileManager.Main.RemoveAllLogFilesOlderThanDays(30);
        }

        [MenuItem("SkillsVR CCK/Logs/Send Log")]
        private static void SendLastLog()
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(LogFileManager.Main.WaitLoginAndSendLastLogFileToPortal("editor_log"));
        }
    }
}

