using UnityEditor;

namespace BuildScripts.Build
{
	[InitializeOnLoad]
	public static class ProjectOpenEvent
	{
		private const string k_ProjectOpened = "ProjectOpened";

		static ProjectOpenEvent()
		{
			if (!SessionState.GetBool(k_ProjectOpened, false))
			{
				SessionState.SetBool(k_ProjectOpened, true);
				string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
				defineSymbols = defineSymbols.Replace(";PICO_XR", string.Empty);
				defineSymbols += ";PICO_XR";
				PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defineSymbols);
				AssetDatabase.Refresh();
			}
		}
	}
}