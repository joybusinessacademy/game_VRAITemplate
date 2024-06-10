using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace SkillsVRNodes.Diagnostics
{
    public abstract class LogFileManager : ILogFileManager
    {
        public abstract string GetLogFileDir();
        public abstract string GetLogFolderName();

        private static ILogFileManager ReplaceableMainManager;

        public static ILogFileManager Main
        {
            get
            {
                if (null == ReplaceableMainManager)
                {
                    ReplaceableMainManager = MakeManager();
                }
                return ReplaceableMainManager;
            }
            set
            {
                ReplaceableMainManager = value;
            }
        }
        
        protected static ILogFileManager MakeManager()
        {
            if (Application.isEditor)
            {
                return new LogFileManagerEditor();
            }
            else
            {
                return new LogFileManagerRuntime();
            }
        }

        protected LogFileManager() { }

        public virtual IEnumerable<string> FindFileRegex(string pattern)
        {
            string dir = GetLogFileDir();
            if (!Directory.Exists(dir))
            {
                return new List<string>(0);
            }
            if (string.IsNullOrWhiteSpace(pattern))
            {
                return new List<string>(0);
            }
             
            var files = Directory.GetFiles(dir)
            .Where(p => Regex.IsMatch(Path.GetFileName(p), pattern));
            return files;
        }

        public virtual string MakeLogFile(string fileNamePrefix)
        {
            string name = string.IsNullOrWhiteSpace(fileNamePrefix) ? "log" : fileNamePrefix;
            name = Path.GetFileNameWithoutExtension(name);
            string dir = GetLogFileDir();
            string time = GetLogFileStartTime().ToString(GetTimeFormatString());
            string path = Path.Combine(dir, $"{name}_{time}.{GetLogFileExtension()}");
            Directory.CreateDirectory(dir);
            return path;
        }

        public virtual DateTime GetLogFileStartTime()
        {
            return DateTime.Now;
        }

        public string GetTimeFormatString()
        {
            return "yyyyMMddHHmmss";
        }

        public string GetTimeSearchingPatternString()
        {
            return "\\d{14}";  // match the numbers of yyyyMMddHHmmss
        }

        public string GetLogFileExtension()
        {
            return "txt";
        }
    }
}

