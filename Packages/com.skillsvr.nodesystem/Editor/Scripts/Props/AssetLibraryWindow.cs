using Scripts.Props;
using SkillsVRNodes.Props;
using UnityEditor;
using UnityEngine;

namespace Scripts.Props
{
	public class AssetLibraryWindow : EditorWindow
	{
	private void CreateGUI()
	{
		titleContent = new UnityEngine.GUIContent("Asset Library", Resources.Load<Texture2D>("Icon/Asset_Library"));

		rootVisualElement.Add(new PropManagerVisualElement());
	}
		
	[MenuItem("SkillsVR CCK/Asset Library")]
	public static void OpenWindow()
	{
		GetWindow<AssetLibraryWindow>();
	}
		
	public void Refresh()
	{
		rootVisualElement.Clear();
		rootVisualElement.Add(new PropManagerVisualElement());
	}
	}
}