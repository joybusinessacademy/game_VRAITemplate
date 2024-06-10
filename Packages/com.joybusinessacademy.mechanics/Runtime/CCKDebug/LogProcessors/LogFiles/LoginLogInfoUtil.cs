#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SkillsVRNodes.Diagnostics
{
    public static class LoginLogInfoUtil
    {
        //User Email
        internal const string EDITORFILE_UE = "EDITORLOGGER_UE";
        //User License Status
        internal const string EDITORFILE_ULS = "EDITORLOGGER_ULS";
        //User Expiry Date
        internal const string EDITORFILE_UED = "EDITORLOGGER_UED";


        public static string GetLoginInfoText()
        {
#if UNITY_EDITOR
            return $"User Email: {SessionState.GetString(EDITORFILE_UE, "No Email Address Found")}\r\n" +
                $"License Details\r\n" +
                $"License Status: {SessionState.GetString(EDITORFILE_ULS, "No License Status Found")}\r\n" +
                $"License Expiration Date:  {SessionState.GetString(EDITORFILE_UED, "No Email Address Found")}";
#else
            return "";
#endif

        }
    }
}

