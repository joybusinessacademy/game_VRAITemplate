using UnityEngine;
using UnityEditor;
using SkillsVRNodes.Managers.Utility;

namespace GraphProcessor
{
	[ExecuteAlways]
	public class DeleteCallback : UnityEditor.AssetModificationProcessor
	{
		static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions options)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				return AssetDeleteResult.DidNotDelete;
			}
            // Do not hook assets that may not graph.
            // And remove error "Do not use ReadObjectThreaded on scene objects!" when delete scene.
            if (!path.EndsWith(".asset"))
			{
				return AssetDeleteResult.DidNotDelete;
			}

			var graph = GraphFinder.LoadAssetAtPath<BaseGraph>(path);
			if (null == graph)
			{
				return AssetDeleteResult.DidNotDelete;
			}
            foreach (var graphWindow in Resources.FindObjectsOfTypeAll<BaseGraphWindow>())
			{
                graphWindow.OnGraphDeleted();
            }

            graph.OnAssetDeleted();
			GraphFinder.OnDeleteAsset(path);
			return AssetDeleteResult.DidNotDelete;
		}
	}
}