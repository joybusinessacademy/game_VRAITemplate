using SkillsVRNodes.Diagnostics.Loggers;

namespace SkillsVRNodes
{
    public class CCKDebug : StaticLoggerHandler<CCKDebug, CCKLogger>
	{
        protected static void OnInitMainLogger(CCKLogger logger)
        {
            logger.Tag.Add(CCKTags.CCK);
        }
    }
}

