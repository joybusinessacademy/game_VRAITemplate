using System;
using UnityEditor;

namespace SkillsVRNodes.Managers
{
	public abstract class GraphAssetPostprocessor : AssetPostprocessor
	{
		public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			if (!BaseGraphWindow.IsOpen) 
				return;
			ScriptableObjectManager.RefreshAllLists();

			Refresh();

		}

		public static Action onRefresh;

		public static void AddListener(Action action)
		{
			if (null == action)
			{
				return;
			}
			onRefresh += action;
		}

		public static void RemoveListener(Action action)
		{
			if (null == action)
			{
				return;
			}
			onRefresh -= action;
		}
		
		public static void Refresh()
		{
			onRefresh?.Invoke();
		}
	}
}
