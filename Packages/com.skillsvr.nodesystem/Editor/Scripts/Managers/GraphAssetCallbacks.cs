using UnityEngine;
using UnityEditor;
using GraphProcessor;
using UnityEditor.Callbacks;
using SkillsVRNodes.Editor.Graph;

public static class GraphAssetCallbacks
{
	[MenuItem("Assets/Create/GraphProcessor", false, 10)]
	public static void CreateGraphProcessor()
	{
		BaseGraph graph = ScriptableObject.CreateInstance< BaseGraph >();
		ProjectWindowUtil.CreateAsset(graph, "GraphProcessor.asset");
	}

	[OnOpenAsset(0)]
	public static bool OnBaseGraphOpened(int instanceID, int line)
	{
		BaseGraph asset = EditorUtility.InstanceIDToObject(instanceID) as BaseGraph;

		if (asset != null)
		{
			SkillsVRGraphWindow.OpenGraph(asset);
			return true;
		}
		return false;
	}
}
