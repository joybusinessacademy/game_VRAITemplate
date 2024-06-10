using System;
using UnityEngine;

namespace SkillsVRNodes.Diagnostics
{
    public class LogFileManagerRuntime : LogFileManager
    {
        protected DateTime sessionStartTime = DateTime.Now;
        public override string GetLogFileDir()
        {
            return Application.persistentDataPath + "/" + GetLogFolderName();
        }

        public override string GetLogFolderName()
        {
            return "RuntimeLogs";
        }

        public override DateTime GetLogFileStartTime()
        {
            return sessionStartTime;
        }

    }
}

