using SkillsVR.Telemetry.Networking.API;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SkillsVRNodes.Diagnostics
{
    public static class ILogFileManagerExtraJobs
    {
        public static IEnumerator WaitLoginAndSendLastLogFileToPortal(this ILogFileManager manager, string logFilePrefix)
        {
            var lastFile = manager.GetLastLogFileByName(logFilePrefix);
            if (string.IsNullOrWhiteSpace(lastFile))
            {
                yield break;
            }

            // Note: CCK Login not working at 
            var waitLogin = new WaitForCCKLogin();
            while (waitLogin.MoveNext())
            {
                yield return null;
            }

            var sendLogOp = new SendLogFileAsync(lastFile);
            while (sendLogOp.MoveNext())
            {
                yield return null;
            }
            if (false == sendLogOp.Success)
            {
                Debug.LogError(sendLogOp.Error);
            }
        }

        public static string GetLastLogFileByName(this ILogFileManager manager, string fileNamePrefix)
        {
            try
            {
                string timePattern = manager.GetTimeSearchingPatternString();
                var files = manager.FindFileRegex($"{fileNamePrefix}_{timePattern}\\.{manager.GetLogFileExtension()}");

                files = files.OrderByDescending(x => x);

                var currentSessionTime = manager.GetLogFileStartTime().ToString(manager.GetTimeFormatString());
                foreach(var file in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    if (fileName.EndsWith(currentSessionTime))
                    {
                        continue;
                    }
                    return file;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            return "";
        }

        public static IEnumerable<string> RemoveAllLogFilesOlderThanDays(this ILogFileManager manager, int days)
        {
            var allFiles = manager.FindFileRegex($"\\w+\\.{manager.GetLogFileExtension()}");
            DateTime nowTime = DateTime.Now;
            string timeFormat = manager.GetTimeFormatString();
            List<string> removedFiles = new List<string>();
            foreach (var file in allFiles)
            {
                try
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    string timeStr = fileName.Substring(fileName.Length - timeFormat.Length);
                    if (DateTime.TryParseExact(timeStr, timeFormat, null, System.Globalization.DateTimeStyles.None, out DateTime logFileTime))
                    {
                        if ((nowTime - logFileTime).TotalDays > days)
                        {
                            File.Delete(file);
                            removedFiles.Add(file);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            return removedFiles;
        }
    }
}

