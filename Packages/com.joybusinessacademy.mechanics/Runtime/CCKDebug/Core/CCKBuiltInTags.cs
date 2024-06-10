
using SkillsVRNodes.Diagnostics;


namespace SkillsVRNodes
{
    /// <summary>
    /// Built in tag for quick code with CCKDebug logs.
    /// i.e. CCKDebug.Log("msg", CCKTags.Asset | CCKTags.Mechanic | "my Tag" | MyEnum.V1);
    /// </summary>
    public partial class CCKTags
    {
        public static LogTag AllCCKTags => LogTag.Make(CCK | Mechanic | Asset | CCKWindow);

        // Used by CCKDebug class
        public static LogTag CCK => LogTag.Make(nameof(CCK));
        public static LogTag Mechanic => LogTag.Make(nameof(Mechanic));
        public static LogTag Asset => LogTag.Make(nameof(Asset));
        public static LogTag CCKWindow => LogTag.Make(nameof(CCKWindow));
        public static LogTag HideInConsole => LogTag.Make(nameof(HideInConsole));
    }

    namespace Diagnostics
    {
        public class Mechanic : CustomCCKDebug<Mechanic> { }
        public class Asset : CustomCCKDebug<Asset> { }
        public class CCKWindow : CustomCCKDebug<CCKWindow> { }


        /*
        public class BOA : CustomCCKDebug<BOA> { }
        class BOAExample
        {
            [UnityEditor.InitializeOnLoadMethod]
            static void MakeBoALogFiles()
            {
                CCKDebug.RegisterLogProcessor<CCKLogFileWritter>(EditorLogFileUtil.MakeEditorSessionLogFile(nameof(BOA)))
                    .LogFilter
                    .MatchAnyTags(BOA.Tag | CCKTags.AllCCKTags);
            }
        }
        */
    }
}

