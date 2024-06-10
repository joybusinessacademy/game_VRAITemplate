using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace SkillsVRNodes.Diagnostics
{
    public static class EditorLogSetup 
    {
        private static UnityLogToCCKAutoDispatcher autoLogDispatcher;

        private const string FIRST_TIME_OPEN_EDITOR_KEY = "FIRST_TIME_OPEN_EDITOR_KEY_78hwerfn";

        [InitializeOnLoadMethod]
        private static void InitEditorLog()
        {
            if (SessionState.GetBool(FIRST_TIME_OPEN_EDITOR_KEY, true))
            {
                SessionState.SetBool(FIRST_TIME_OPEN_EDITOR_KEY, false);
                OnEditorStart();
            }

            AddEditorLogHandlers(); 
        }
        private static void OnEditorStart()
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(LogFileManager.Main.WaitLoginAndSendLastLogFileToPortal("editor_log"));
        }

        private static void AddEditorLogHandlers()
        {
            LogFileManager.Main.RemoveAllLogFilesOlderThanDays(30);

            autoLogDispatcher = new UnityLogToCCKAutoDispatcher();
            autoLogDispatcher.LogFilter
                .ErrorsOnly()
                .BlockDuplicate();

            CCKDebug.RegisterLogProcessor<CCKLogConsolePrinter>()
                .LogFilter
                .ErrorsOnly()
                .BlockTags(CCKTags.HideInConsole)
                .BlockDuplicate();

            CCKDebug.RegisterLogProcessor<CCKLogFileWritter>(
                LogFileManager.Main.MakeLogFile("editor_log"))
                .LogFilter
                .BlockLogTypes(LogType.Warning)
                .BlockDuplicate();

            CCKDebug.RegisterLogProcessor<CCKLogFileWritter>(
                LogFileManager.Main.MakeLogFile("editor_log_error"))
                .LogFilter
                .ErrorsOnly()
                .BlockDuplicate();
        }
    }
}

