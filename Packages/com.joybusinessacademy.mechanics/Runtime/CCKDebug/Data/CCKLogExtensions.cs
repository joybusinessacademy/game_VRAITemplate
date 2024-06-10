using UnityEngine;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;

namespace SkillsVRNodes.Diagnostics
{
    public static class CCKLogExtensions
	{
        public static bool HasTag(this CCKLogEntry log, string tag)
        {
            if (null == log
                || null == log.tags
                || string.IsNullOrWhiteSpace(tag))
            {
                return false;
            }
            return log.tags.Contains(tag);
        }

        public static bool HasAnyTagIn(this CCKLogEntry log, IEnumerable<string> tags)
        {
            if (null == log
                || null == log.tags
                || null == tags)
            {
                return false;
            }
            return log.tags.Any(t=> tags.Contains(t));
        }

        public static int GetConentHashCode(this CCKLogEntry log)
        {
            if (null == log)
            {
                return 0;
            }

            string txt = string.Join("|", 
                log.GetTagText(), 
                log.message.ObjectText(), 
                log.context.ObjectText(), 
                log.GetStackTraceText()
            );
            return txt.GetHashCode();
        }

        public static string ToString(this CCKLogEntry log, bool stackTrace, bool stackTraceUrl = false)
        {
            if (null == log)
            {
                return "";
            }
            string text = AutoText("[", log.GetTagText(), "] ") + log.message.ObjectText();
            text += NewLine(AutoText("Context: ", log.context.ObjectText("")));

            if (stackTrace)
            {
                if (stackTraceUrl)
                {
                    text += NewLine(log.GetStackTraceUrl());
                }
                else
                {
                    text += NewLine(log.GetStackTraceText());
                }
            }
            return text;
        }

        public static string ToMsgBodyString(this CCKLogEntry log)
        {
            if (null == log)
            {
                return "";
            }
            string text = log.message.ObjectText();
            text += NewLine(AutoText("Context: ", log.context.ObjectText("")));

            return text;
        }

        public static string ToMsgBodyStringWithStackTrace(this CCKLogEntry log)
        {
            var text = ToMsgBodyString(log);
            text += NewLine(log.GetStackTraceText());
            return text;
        }

        public static string ToUnityConsoleText(this CCKLogEntry log)
		{
            return log.ToString(true, true);
		}

        public static string ToLogS(this CCKLogEntry log)
        {
            return log.ToString(false, false);
        }

        public static string ToLog(this CCKLogEntry log)
        {
            return log.ToString(true, false);
        }

        public static void PrintConsoleLog(this LogType logType, string message)
		{
            switch (logType)
            {
                case LogType.Error:
                    UnityEngine.Debug.LogError(message);
                    break;
                case LogType.Assert:
                    UnityEngine.Debug.LogAssertion(message);
                    break;
                case LogType.Warning:
                    UnityEngine.Debug.LogWarning(message);
                    break;
                case LogType.Log:
                    UnityEngine.Debug.Log(message);
                    break;
                case LogType.Exception:
                    UnityEngine.Debug.LogError(message);
                    break;
                default:
                    UnityEngine.Debug.LogError(message);
                    break;
            }
        }

        private static string AutoText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return "";
            }

            return text;
        }

        private static string NewLine(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return "";
			}

			return "\r\n" + text;
		}

        private static string AutoText(string prefix, string text, string suffix = null)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return "";
            }
            prefix = string.IsNullOrWhiteSpace(prefix) ? "" : prefix;
            suffix = string.IsNullOrWhiteSpace(suffix) ? "" : suffix;
            return prefix + text + suffix;
        }

        public static string ObjectText(this object obj, string nullText = "null")
		{
			if (null == obj)
			{
				return nullText;
			}

			return obj.ToString();
		}


        public static string GetTagText(this CCKLogEntry log)
		{
			if (null == log|| null == log.tags)
			{
				return "";
			}

            var list = log.tags.ToList();
            list.Remove(CCKTags.HideInConsole.ToString());
			return string.Join("|", list);
		}

        public static IEnumerable<StackFrame> FilterFrames(this StackTrace stackTrace, string filterClassName)
        {
            if (null == stackTrace)
            {
                return new StackFrame[0];
            }
            var frames = stackTrace.GetFrames().ToList();
            try
            {
                var lastIndex = frames.FindLastIndex(0, x => x.GetMethod().DeclaringType.Name == filterClassName);
                if (lastIndex < 0)
                {
                    return frames;
                }
                frames.RemoveRange(0, lastIndex);
            }
            catch
            {
            }

            
            return frames;
        }

        public static string GetText(this StackFrame frame)
        {
            if (null == frame)
            {
                return "";
            }
            var fullFilePath = frame.GetFileName();
            if  (string.IsNullOrWhiteSpace(fullFilePath))
            {
                return "";
            }
            var procPath = Application.dataPath.Replace("Assets", "");
            var filePath = fullFilePath.Substring(procPath.Length);
            return $"{frame.GetMethod().DeclaringType.Name}:" +
                $"{frame.GetMethod().Name}({string.Join(", ", frame.GetMethod().GetParameters().Select(x=>x.GetType().Name))}) " +
                $"(at {filePath}:{frame.GetFileLineNumber()})";
        }

        public static string GetUrl(this StackFrame frame)
        {
            var fullFilePath = frame.GetFileName();
            var procPath = Application.dataPath.Replace("Assets", "").Replace("\\", "/");
            var filePath = fullFilePath.Substring(procPath.Length).Replace("/","\\" );
            var line = frame.GetFileLineNumber();
            return $"{frame.GetMethod().DeclaringType.Name}:" +
                $"{frame.GetMethod().Name}({string.Join(", ", frame.GetMethod().GetParameters().Select(x => x.GetType().Name))}) " +
                $"(at <a href=\"{filePath}\" line=\"{line}\">{filePath}:{line}</a>)";
        }


        public static string GetStackTraceUrl(this CCKLogEntry log)
        {
            if (null == log)
            {
                return "";
            }

            string stackTraceText = "";

            if (log.stackTrace != null)
            {
                var frames = log.stackTrace.FilterFrames(nameof(CCKDebug));
                foreach (var frame in frames)
                {
                    stackTraceText += frame.GetUrl() + "\r\n";
                }
            }

            if (!string.IsNullOrEmpty(log.stackTraceString))
            {
                stackTraceText += log.stackTraceString;
            }

            return stackTraceText;
        }
        public static string GetStackTraceText(this CCKLogEntry log)
		{
			if (null == log)
			{
				return "";
			}

            string stackTraceText = "";

            if (log.stackTrace != null)
            {
                var frames = log.stackTrace.FilterFrames(nameof(CCKDebug));
                foreach(var frame in frames)
                {
                    stackTraceText += frame.GetText() + "\r\n";
                }
            }

            if (!string.IsNullOrEmpty(log.stackTraceString))
            {
                stackTraceText += log.stackTraceString;
            }

            return stackTraceText;
        }
	}
}

