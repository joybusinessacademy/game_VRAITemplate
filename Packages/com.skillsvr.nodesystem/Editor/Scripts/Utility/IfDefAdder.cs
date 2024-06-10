using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace SkillsVRNodes.Managers.Utility
{
	public static class IfDefAdder
	{
		private const string nodeDevDefinition = "NODE_DEVELOPMENT";

		#if !NODE_DEVELOPMENT
		//[MenuItem("SkillsVR CCK/Dev/Add Defines")]
		private static void AddDefines()
		{
			PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Android, out string[] arrayOfDefs);

			List<string> listOfDefs = arrayOfDefs.ToList();
			
			if (!listOfDefs.Contains(nodeDevDefinition))
			{
				listOfDefs.Add(nodeDevDefinition);
			}
			
			PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Android, listOfDefs.ToArray());

			AssetDatabase.Refresh();
			UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
		}
		#else
		
		[MenuItem("SkillsVR CCK/Dev/Remove Defines")]
		private static void RemoveDefines()
		{
			PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Android, out string[] arrayOfDefs);

			List<string> listOfDefs = arrayOfDefs.ToList();
			
			if (listOfDefs.Contains(nodeDevDefinition))
			{
				listOfDefs.Remove(nodeDevDefinition);
			}
			
			PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Android, listOfDefs.ToArray());

			AssetDatabase.Refresh();
			UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
		}
		#endif
	}
}