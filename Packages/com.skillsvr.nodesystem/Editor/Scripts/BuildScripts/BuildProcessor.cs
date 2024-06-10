using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace BuildScripts.Build
{
	public class BuildProcessor : IPostprocessBuildWithReport
	{
		public int callbackOrder => 0;
		public void OnPostprocessBuild(BuildReport report)
		{
			string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
			defineSymbols = defineSymbols.Replace(";PICO_XR", string.Empty);
			defineSymbols += ";PICO_XR";
			PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defineSymbols);
			AssetDatabase.Refresh();
		}
	}
}