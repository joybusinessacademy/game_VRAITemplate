using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace SkillsVRNodes.Diagnostics.Loggers
{

    /// <summary>
    /// Trigger receive log event by log interfaces.
    /// Note: this class only create log data and pass to callbacks.
    /// </summary>
    public class CCKLogger
    {
        public event Action<CCKLogger> OnDestory;

        public virtual int StackTraceSkipLevel { get; set; } = 3;

        public List<ICCKLogProcessor> LogProcessors { get; } = new List<ICCKLogProcessor>();

        public LogTag Tag { get; } = new LogTag();

        ~CCKLogger()
        {
            try
            {
                OnDestory?.Invoke(this);
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }

        public CCKLogEntry Log(object message, params string[] tags)
        {
            return SendNewLog(LogType.Log, message, null, tags);
        }

        public CCKLogEntry Log(object message, object data, params string[] tags)
        {
            return SendNewLog(LogType.Log, message, data, tags);
        }

        public CCKLogEntry LogFormat(string format, params object[] args)
        {
            return SendNewLogFormat(LogType.Log, null, format, args);
        }

        public CCKLogEntry LogFormat(string[] tags, string format, params object[] args)
        {
            return SendNewLogFormat(LogType.Log, tags, format, args);
        }

        public CCKLogEntry LogWarning(object message, params string[] tags)
        {
            return SendNewLog(LogType.Warning, message, null, tags);
        }

        public CCKLogEntry LogWarning(object message, object data, params string[] tags)
        {
            return SendNewLog(LogType.Warning, message, data, tags);
        }

        public CCKLogEntry LogWarningFormat(string format, params object[] args)
        {
            return SendNewLogFormat(LogType.Warning, null, format, args);
        }

        public CCKLogEntry LogWarningFormat(string[] tags, string format, params object[] args)
        {
            return SendNewLogFormat(LogType.Warning, tags, format, args);
        }

        public CCKLogEntry LogError(object message, params string[] tags)
        {
            return SendNewLog(LogType.Error, message, null, tags);
        }

        public CCKLogEntry LogError(object message, object data, params string[] tags)
        {
            return SendNewLog(LogType.Error, message, data, tags);
        }

        public CCKLogEntry LogErrorFormat(string format, params object[] args)
        {
            return SendNewLogFormat(LogType.Error, null, format, args);
        }

        public CCKLogEntry LogErrorFormat(string[] tags, string format, params object[] args)
        {
            return SendNewLogFormat(LogType.Error, tags, format, args);
        }

        public CCKLogEntry LogException(Exception message, params string[] tags)
        {
            return SendNewLog(LogType.Exception, message, null, tags);
        }

        public CCKLogEntry LogException(Exception message, object data, params string[] tags)
        {
            return SendNewLog(LogType.Exception, message, data, tags);
        }

        public virtual CCKLogEntry ProcessLog(CCKLogEntry log)
        {
            if (null == log)
            {
                return log;
            }

            foreach (var proc in LogProcessors)
            {
                try
                {
                    proc?.OnReceiveLog(log);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                }
            }
            return log;
        }

        public virtual CCKLogEntry SendNewLog(LogType logType, object message, object context, params string[] tags)
        {
            CCKLogEntry log = new CCKLogEntry();
            log.timeUTC = DateTime.UtcNow;
            log.logType = logType;
            log.message = message;
            log.context = context;
            log.stackTrace = new StackTrace(StackTraceSkipLevel, true);
            log.tags = Tag | tags;

            return ProcessLog(log);
        }

        public virtual CCKLogEntry SendNewLogFormat(LogType logType, string[] tags, string format, params object[] args)
        {
            CCKLogEntry log = new CCKLogEntry();
            log.timeUTC = DateTime.UtcNow;
            log.logType = logType;
            string message = "";
            try
            {
                message = string.Format(format, args);
            }
            catch(Exception e)
            {
                message = string.IsNullOrWhiteSpace(format) ? "" : format;
            }
            log.message = message;
            log.stackTrace = new StackTrace(StackTraceSkipLevel, true);
            log.tags = Tag | tags;

            return ProcessLog(log);
        }

        public virtual CCKLogEntry LogUnity(LogType logType, object message, string stackTrace, params string[] tags)
        {
            CCKLogEntry log = new CCKLogEntry();
            log.timeUTC = DateTime.UtcNow;
            log.logType = logType;
            log.message = message;
            log.stackTraceString = stackTrace;
            log.tags = Tag | tags;

            return ProcessLog(log);
        }
    }
}

