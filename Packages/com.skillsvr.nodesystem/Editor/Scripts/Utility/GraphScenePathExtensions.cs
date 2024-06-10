using GraphProcessor;
using System.IO;
using UnityEditor;

namespace SkillsVRNodes.Managers.Utility
{
    public static class GraphScenePathExtensions
	{
		public static string GetDefaultGraphScenePath(this BaseGraph graph)
		{
			if (null == graph)
			{
				return null;
			}
			string path = AssetDatabase.GetAssetPath(graph);
            return GetDefaultScenePathFromGraphPath(path);
		}

		public static string GetDefaultGraphAssetPath(this SceneAsset sceneAsset)
		{
            if (null == sceneAsset)
            {
                return null;
            }
            string path = AssetDatabase.GetAssetPath(sceneAsset);
			return GetDefaultGraphAssetPathFromScenePath(path);
        }

		public static string GetDefaultGraphAssetPathFromScenePath(string scenePath)
		{
            return scenePath.Replace(".unity", "Graph.asset");
        }

		public static string GetDefaultScenePathFromGraphPath(string graphPath)
		{
            string fileName = Path.GetFileName(graphPath);

            string graphFileNameMark = "Graph.asset";
            if (fileName.EndsWith(graphFileNameMark))
            {
                return graphPath.Replace(graphFileNameMark, ".unity");
            }
            else
            {
                return graphPath.Replace(".asset", ".unity");
            }
        }

    }
}
