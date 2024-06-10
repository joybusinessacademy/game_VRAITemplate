using SkillsVRNodes;
using SkillsVRNodes.Managers.Setup;
using SkillsVRNodes.Managers.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;

namespace BuildScripts.Build
{
	public static class BuildEXE
	{
		[MenuItem("SkillsVR CCK/Windows Build/Build exe")]
		public static void BuildGameNoReturn()
		{
            CCKDebug.Log("Used Menu Item: Building to .exe");

			WindowsBuildPreCheck();
			BuildGame();
		}

		[MenuItem("SkillsVR CCK/Windows Build/Build exe - Development")]
		public static void BuildGameNoReturnDevelopment()
		{
			WindowsBuildPreCheck();
			BuildGame(development: true);
		}

		public static void CreateWindowsItems()
		{
			if (EditorSceneManager.GetActiveScene().buildIndex != 0)
			{
				string scenePath = GraphFinder.HomeGraph.GetDefaultGraphScenePath();
				EditorSceneManager.OpenScene(scenePath);
			}

			//TODO Remove Link From Mechanics
			GraphSetupTools.ValidateOrCreateGameObjectWithComponent<WindowsEXEController>("Windows EXE Controller");
			EditorSceneManager.MarkAllScenesDirty();
			EditorSceneManager.SaveOpenScenes();
		}

		private static void WindowsBuildPreCheck()
		{
			CreateWindowsItems();
			EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
		}

		public static BuildReport BuildGame(string filename = "/BuiltGame.exe", bool development = false)
		{
			// Get filename.
			string path = BuildUtilities.GetBuildFolderDir();

            List<string> levels = (from scene in EditorBuildSettings.scenes where scene.enabled select scene.path).ToList();

			string outputFilePath = path + filename;
			BuildUtilities.SaveLastBuildPath(outputFilePath);
			// Build player.
            BuildReport buildReport = BuildPipeline.BuildPlayer(levels.ToArray(), outputFilePath, BuildTarget.StandaloneWindows, development == true ? BuildOptions.Development : BuildOptions.None);

			if (BuildResult.Succeeded == buildReport.summary.result )
			{
                BuildUtilities.OpenFolder(BuildUtilities.GetLastBuildPath());
            }
            
            return buildReport;
		}
	}
}