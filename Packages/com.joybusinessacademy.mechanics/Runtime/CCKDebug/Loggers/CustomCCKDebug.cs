using SkillsVRNodes.Diagnostics.Loggers;

namespace SkillsVRNodes.Diagnostics
{
    // Static log interfaces for custom logger class;
    // And also redirect any logs to CCKDebug.
    public abstract class CustomCCKDebug<HANDLER_TYPE> : StaticLoggerHandler<HANDLER_TYPE, CCKLoggerRedirector>
    {
        protected static void OnInitMainLogger(CCKLoggerRedirector logger)
        {
            logger.Tag.Add(typeof(HANDLER_TYPE).Name);
        }
    }

    namespace Loggers
    {
        // A cck logger that also send log to CCKDebug logger.
        // This is used for making custom loggers that handled by CCKDebug as well.
        public sealed class CCKLoggerRedirector : CCKLogger
        {
            public override CCKLogEntry ProcessLog(CCKLogEntry log)
            {
                base.ProcessLog(log);
                return CCKDebug.MainLogger.ProcessLog(log);
            }
        }
    }

    
}

