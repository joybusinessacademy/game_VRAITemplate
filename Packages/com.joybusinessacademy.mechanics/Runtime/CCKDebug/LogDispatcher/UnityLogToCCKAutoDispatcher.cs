using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkillsVRNodes.Diagnostics
{
    /// <summary>
    /// Receive console log errors and resend as cck log
    /// </summary>
    public class UnityLogToCCKAutoDispatcher
    {
        public IFilter<CCKLogEntry> LogFilter { get; set; } = new LogFilter();

        public UnityLogToCCKAutoDispatcher()
        {
            Start();
        }

        ~UnityLogToCCKAutoDispatcher()
        {
            Stop();
        }

        public void Start()
        {
            Stop();
            Application.logMessageReceived += HandleLogForConsole;
        }

        public void Stop()
        {
            Application.logMessageReceived -= HandleLogForConsole;
        }

        private void HandleLogForConsole(string logText, string stackTrace, LogType type)
        {
            var list = stackTrace.Split("\n");
            if (list.Length >= 3)
            {
                var line = list[2];
                if (line.StartsWith("SkillsVRNodes.Diagnostics.CCKLogConsolePrinter:OnReceiveLog"))
                {
                    return;
                }
            }

            CCKLogEntry log = new CCKLogEntry();
            log.logType = type;
            log.message = logText;
            log.stackTraceString = stackTrace;
            log.timeUTC = DateTime.UtcNow;
            if (LogFilter?.IsBlocked(log)??false)
            {
                return;
            }
            CCKDebug.LogUnity(type,  stackTrace, logText, CCKTags.HideInConsole);
        }
    }

}

