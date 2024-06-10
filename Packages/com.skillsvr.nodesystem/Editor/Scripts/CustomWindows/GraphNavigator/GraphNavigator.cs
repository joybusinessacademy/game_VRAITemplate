using SkillsVRNodes.Editor.Graph;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Managers.GraphNavigator
{
	public class GraphNavigator : EditorWindow
	{
		public GraphNavigatorVisualElement GraphNavigatorVisualElement => rootVisualElement.Q<GraphNavigatorVisualElement>();

		private void CreateGUI()
		{
			titleContent = new UnityEngine.GUIContent("Graph Navigator", Resources.Load<Texture2D>("Icon/Navigator"));

			Refresh();
		}

		[MenuItem("SkillsVR CCK/Graph Navigator")]
		public static void OpenWindow()
		{
            CCKDebug.Log("Used Menu Item: Graph Navigator");

			GetWindow<GraphNavigator>(typeof(SkillsVRGraphWindow));
		}

		public void Refresh()
		{
			rootVisualElement.Clear();
			rootVisualElement.Add(new GraphNavigatorVisualElement());
		}
		
		public static void RefreshWindow()
		{
			if (HasOpenInstances<GraphNavigator>())
			{
				GetWindow<GraphNavigator>().Refresh();
			}
		}
	}
}