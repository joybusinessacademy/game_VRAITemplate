using System;
using System.Collections.Generic;

namespace SkillsVRNodes.Diagnostics
{
    public interface ILogFileManager
    {
        string GetLogFileDir();
        string GetTimeFormatString();
        string GetTimeSearchingPatternString();
        string GetLogFolderName();
        string GetLogFileExtension();
        DateTime GetLogFileStartTime();

        string MakeLogFile(string fileNamePrefix);
        IEnumerable<string> FindFileRegex(string pattern);
    }
}

