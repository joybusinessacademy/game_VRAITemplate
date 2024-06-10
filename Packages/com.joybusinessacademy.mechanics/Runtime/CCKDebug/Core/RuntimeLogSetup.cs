using UnityEngine;

namespace SkillsVRNodes.Diagnostics
{
    public static class RuntimeLogSetup
    {
        private static UnityLogToCCKAutoDispatcher autoLogDispatcher;

        [RuntimeInitializeOnLoadMethod]
        static void AddRuntimeLogHandlers()
        {
            if (Application.isEditor)
            {
                return;
            }

            LogFileManager.Main.RemoveAllLogFilesOlderThanDays(30);

            autoLogDispatcher = new UnityLogToCCKAutoDispatcher();
            autoLogDispatcher.LogFilter
                .ErrorsOnly()
                .BlockDuplicate();


            CCKDebug.RegisterLogProcessor<CCKLogFileWritter>(LogFileManager.Main.MakeLogFile("log"))
                .LogFilter
                .BlockLogTypes(LogType.Warning)
                .BlockDuplicate();
        }
    }
}

