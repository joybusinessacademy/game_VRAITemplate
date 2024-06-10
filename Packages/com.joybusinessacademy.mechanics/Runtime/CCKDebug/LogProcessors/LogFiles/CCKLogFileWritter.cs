using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SkillsVRNodes.Diagnostics
{
    public class CCKLogFileWritter : ICCKLogProcessor 
    {
        public string FilePath { get; protected set; }
        public IFilter<CCKLogEntry> LogFilter { get; set; } = new LogFilter();
        public CCKLogFileWritter(string filePath)
        {
            FilePath = filePath;

            if(IsEmptyFile(FilePath))
            {
                GenerateWelcomeMessage();
            }
        }

        private bool IsEmptyFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return true;
            }
            if (!File.Exists(filePath))
            {
                return true;
            }

            FileInfo fileInfo = new FileInfo(FilePath);
            return fileInfo.Length <= 0;
        }

        public void OnReceiveLog(CCKLogEntry log)
        {
            if (LogFilter?.IsBlocked(log) ?? false)
            {
                return;
            }
            Log(log);
        }

        private void GenerateWelcomeMessage()
        {
            string platform = Application.isEditor ? "Editor " : "";
            Log($"SkillsVR CCK {platform}Log");

            //TODO FIX THIS
            //Not Using CCK Version
            Log("CCK Version: " + "1.0.0");

            Log(LoginLogInfoUtil.GetLoginInfoText());

            Log("Project: " + Application.dataPath);
            Log("Platform:" + Application.platform);
            string timeFormat = LogFileManager.Main.GetTimeFormatString();
            Log("Time (UTC): " + DateTime.UtcNow.ToString(timeFormat));
            Log("Time (Local): " + DateTime.Now.ToString(timeFormat));
            Log("Time Zone: " + TimeZoneInfo.Local.DisplayName);
        }

        public void Log(string message)
        {
            SerializedLogLine line = new SerializedLogLine();
            line.SetUTC(DateTime.UtcNow);
            line.lv = LogType.Log.ToString();
            line.msg = message;
            AppendLine(JsonUtility.ToJson(line));
        }

        public void Log(CCKLogEntry log)
        {
            SerializedLogLine line = new SerializedLogLine();
            line.SetUTC(log.timeUTC);
            line.lv = log.logType.ToString();
            switch (log.logType)
            {
                case LogType.Log:
                case LogType.Warning:
                    line.msg = log.ToMsgBodyString();
                    break;
                default:
                    line.msg = log.ToMsgBodyStringWithStackTrace();
                    break;
            }
            line.tags = log.GetTagText();
            AppendLine(JsonUtility.ToJson(line));
        }

        protected void AppendLine(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(FilePath))
            {
                return;
            }
            File.AppendAllText(FilePath, text + Environment.NewLine);
        }

        protected void AppendLines(params string[] lines)
        {
            if (null == lines)
            {
                return;
            }

            string text = string.Join(Environment.NewLine, lines);
            File.AppendAllText(FilePath, text + Environment.NewLine);
        }

        protected void AppendLines(IEnumerable<string> lines)
        {
            if (null == lines)
            {
                return;
            }

            string text = string.Join(Environment.NewLine, lines);
            File.AppendAllText(FilePath, text + Environment.NewLine);
        }
    }
}

