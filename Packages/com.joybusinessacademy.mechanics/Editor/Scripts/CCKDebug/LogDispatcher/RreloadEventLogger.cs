using UnityEditor;

namespace SkillsVRNodes.Diagnostics
{
    internal class RreloadEventLogger
    {
        [InitializeOnLoadMethod]
        static void Reload()
        {
            CCKDebug.Log("Start Reload Script");

        }
        [UnityEditor.Callbacks.DidReloadScripts]
        static void DidReload()
        {
            CCKDebug.Log("Finish Reload Script");
        }
    }
}

