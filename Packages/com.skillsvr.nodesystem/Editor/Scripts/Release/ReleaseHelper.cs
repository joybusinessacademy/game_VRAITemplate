using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ReleaseHelper
{
	private const string LicenseDefName = "com.SkillsVR.License.asmdef";
	private static readonly string AssemblyName = "com.SkillsVR.License";
	private static readonly string SourcePath = Path.Combine(GetProjectPath(), "Library/ScriptAssemblies/", $"{AssemblyName}.dll");

#if NODE_DEVELOPMENT
	[MenuItem("SkillsVR CCK/Prepare Release")]
#endif
	public static void PrepareRelease()
	{
		//Removing Defines for Release
		string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
		defineSymbols = defineSymbols.Replace(";NODE_DEVELOPMENT", string.Empty);
		PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defineSymbols);

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		string asmdefFilePath = FindAssemblyDefinition(LicenseDefName);

		if (!string.IsNullOrEmpty(asmdefFilePath))
		{
			string assetScriptDir = Path.GetDirectoryName(asmdefFilePath);
			FindDLLFromAssembly(assetScriptDir);
		}
		else
		{
			Debug.Log($"Assembly Definition file not found: {LicenseDefName}");
		}
	}

	private static string FindAssemblyDefinition(string asmdefName) =>
		Directory.GetFiles(Application.dataPath, asmdefName, SearchOption.AllDirectories).FirstOrDefault();

	private static void FindDLLFromAssembly(string assetScriptDir)
	{
		if (File.Exists(SourcePath))
		{
			DeleteAllFilesInDirectory(assetScriptDir);

			string destinationFilePath = Path.Combine(assetScriptDir, Path.GetFileName(SourcePath));
			File.Copy(SourcePath, destinationFilePath, true);
		}
		else
		{
			Debug.Log($"Unable to find DLL at: {SourcePath}");
		}
	}

	private static string GetProjectPath() =>
		Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;

	private static void DeleteAllFilesInDirectory(string directoryPath)
	{
		try
		{
			Directory.GetFiles(directoryPath).ToList().ForEach(filePath =>
			{
				File.Delete(filePath);
			});
		}
		catch (Exception e)
		{
			Debug.LogError($"Error deleting files: {e.Message}");
		}
	}
}
