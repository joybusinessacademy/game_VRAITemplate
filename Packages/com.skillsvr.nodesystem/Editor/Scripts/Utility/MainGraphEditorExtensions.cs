using GraphProcessor;
using SkillsVRNodes.Managers.Setup;
using SkillsVRNodes.Scripts.Nodes;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace SkillsVRNodes.Managers.Utility
{
    public static class MainGraphEditorExtensions
	{
		public static IEnumerable<SceneNode> GetAllSceneNodes(this MainGraph mainGraph)
		{
			if (null == mainGraph || null == mainGraph.nodes)
			{
				return new List<SceneNode>();
			}

			return mainGraph.nodes.Where(x => null != x && x is SceneNode).Select(x => (SceneNode)x);
		}

		public static IEnumerable<string> GetAllBuildScenePath(this MainGraph mainGraph)
		{
            if (null == mainGraph || null == mainGraph.nodes)
            {
                return new List<string>();
            }

			var sceneNodes = GetAllSceneNodes(mainGraph);
			List<string> list = new List<string>();
			list.AddRange(sceneNodes.Where(x => null != x && !string.IsNullOrWhiteSpace(x.scenePath)).Select(x => x.scenePath));
			list.AddRange(sceneNodes.Where(x => null != x && null != x.additiveScenePaths).SelectMany(x => x.additiveScenePaths));
			return list.Distinct();
        }

		public static void AddMainGraphSceneAndChildrenScenesToBuildSettings(this MainGraph mainGraph)
		{
			if (null == mainGraph)
			{
				return;
			}
            string scenePath = mainGraph.GetDefaultGraphScenePath();
            GraphSetupTools.AddOrEnableSceneInBuildSettings(scenePath);
            var buildScenes = mainGraph.GetAllBuildScenePath();
            GraphSetupTools.AddOrEnableScenesInBuildSettings(buildScenes);
        }

		public static bool IsStartUpGraph(this MainGraph mainGraph)
		{
			var listScenes = EditorBuildSettings.scenes;
			foreach(var item in listScenes)
			{
				if (null == item || !item.enabled || string.IsNullOrWhiteSpace(item.path))
				{
					continue;
				}
				return item.path == mainGraph.GetDefaultGraphScenePath();
			}
			return false;
        }

		public static void SetAsStartUpGraph(this MainGraph mainGraph)
		{
			var items = EditorBuildSettings.scenes.ToList();
			var startup = items.Find(x => null != x && x.path == mainGraph.GetDefaultGraphScenePath());
			if (null == startup)
			{
				startup = new EditorBuildSettingsScene() { enabled = true, path = mainGraph.GetDefaultGraphScenePath() };
			}
			else
			{
				startup.enabled = true;
				items.Remove(startup);
			}
            items.Insert(0, startup);
			EditorBuildSettings.scenes = items.ToArray();
        }
    }
}
