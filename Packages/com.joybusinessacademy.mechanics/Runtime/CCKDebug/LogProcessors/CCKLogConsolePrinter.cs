namespace SkillsVRNodes.Diagnostics
{
    /// <summary>
    /// Print cck log to console 
    /// </summary>
    public class CCKLogConsolePrinter : ICCKLogProcessor
    {
        public IFilter<CCKLogEntry> LogFilter { get; set; } = new LogFilter();

        public void OnReceiveLog(CCKLogEntry log)
        {
            if (null == log)
            {
                return;
            }
            
            if (LogFilter?.IsBlocked(log) ?? false)
            {
                return;
            }
            
            log.logType.PrintConsoleLog(log.ToUnityConsoleText());
        }
    }
}

