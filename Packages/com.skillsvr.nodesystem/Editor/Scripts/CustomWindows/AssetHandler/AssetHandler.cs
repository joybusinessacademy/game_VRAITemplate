using System;
using SkillsVRNodes.Editor.Graph;
using SkillsVRNodes.Managers.GraphNavigator;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SkillsVRNodes.Scripts.CustomWindows
{
	public class AssetHandler : EditorWindow
	{
		public AssetHandlerVisualElement AssetHandlerVisualElement => rootVisualElement.Q<AssetHandlerVisualElement>();

		private void CreateGUI()
		{
			titleContent = new UnityEngine.GUIContent("Asset Handler", Resources.Load<Texture2D>("Icon/Navigator"));
			Refresh();
		}
		
		[MenuItem("SkillsVR CCK/Asset Handler")]
		public static void OpenWindow()
		{
            CCKDebug.Log("Used Menu Item: Asset Handler");

			GetWindow<AssetHandler>(typeof(SkillsVRGraphWindow));
		}
		
		public void Refresh()
		{
			if (AssetHandlerVisualElement == null)
			{
				rootVisualElement.Add(new AssetHandlerVisualElement());
			}
			else
			{
				AssetHandlerVisualElement.Refresh();
			}
		}
		
		public static void RefreshWindow()
		{
			if (HasOpenInstances<AssetHandler>())
			{
				GetWindow<AssetHandler>().Refresh();
			}
		}

		protected virtual void OnGUI()
		{
			HandleKeyboard();
		}
		
		private void HandleKeyboard()
		{
			Event current = Event.current;
			if (current.type != EventType.KeyDown)
				return;

			switch(current.keyCode)
			{
				case KeyCode.F5:
					if (current.shift)
					{
						AssetDatabaseFileCache.PreCache();
					}
					Refresh();
					break;
				case KeyCode.Escape:
					Selection.objects = Array.Empty<Object>();
					break;
				case KeyCode.F:
					if (current.control)
					{
						AssetHandlerVisualElement.Find();
					}
					break;
			}
		}
	}
}