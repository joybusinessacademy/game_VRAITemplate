using System;
using SkillsVRNodes.Props;
using UnityEditor;
using UnityEngine;

namespace SkillsVRNodes.Scripts.Hierarchy
{
	public class PropsHierarchyWindow : EditorWindow
	{
        public static string highlightedName;

        private void CreateGUI()
		{
			titleContent = new UnityEngine.GUIContent("Asset Hierarchy", Resources.Load<Texture2D>("Icon/PropsHeirarchy"));

			rootVisualElement.Add(new PropsHierarchyVisualElement());
		}

		[MenuItem("SkillsVR CCK/Asset Hierarchy")]
		public static void OpenWindow()
		{
            CCKDebug.Log("Used Menu Item: Asset Hierarchy");

			GetWindow<PropsHierarchyWindow>();
		}

		public void Refresh()
		{
			rootVisualElement.Clear();
			rootVisualElement.Add(new PropsHierarchyVisualElement());
		}
		
		public static void RefreshWindow()
		{
			if (HasOpenInstances<PropsHierarchyWindow>())
			{
				GetWindow<PropsHierarchyWindow>().Refresh();
			}
		}
	}
}