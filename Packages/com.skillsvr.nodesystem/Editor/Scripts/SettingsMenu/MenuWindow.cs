using Scripts.VisualElements;
using SkillsVRNodes.Managers.Utility;
using SkillsVRNodes.Scripts.Nodes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuWindow : EditorWindow
{
	private const string ECCloudClassName = "SkillsVR.EnterpriseCloudSDK.Editor.EnterpriseCloudSDKEditorWindow, com.skillsvr.EnterpriseCloudSDK.editor";

	VisualElement menuWindow;
	static VisualElement leftPane;
	static VisualElement rightPane;
	static List<VisualElement> buttons = new List<VisualElement>();

	static Color darkgrey = new Color(0.12f, 0.12f, 0.12f);
	static Color blue = new Color(0.27f, 0.75f, 1f);

	private const string CCK_PS_SELECTEDPROJECT = "CCK_PS_SELECTEDPROJECT";

	public static void OpenWindow()
    {
		EditorWindow window = GetWindow<MenuWindow>();
        window.titleContent = new GUIContent("CCK Project Settings");
        window.minSize = new Vector2(800, 400);

		// Create a two-pane view with the left pane being fixed with
		var splitView = new TwoPaneSplitView(0, 450, TwoPaneSplitViewOrientation.Horizontal);

		// A TwoPaneSplitView always needs exactly two child elements
		leftPane = new VisualElement();
		leftPane.style.paddingLeft = 10;
		leftPane.style.paddingRight = 10;
		leftPane.style.paddingTop = 10;
		splitView.Add(leftPane);
		rightPane = new VisualElement();
		rightPane.style.paddingLeft = 10;
		rightPane.style.paddingRight = 10;
		rightPane.style.paddingTop = 10;
		splitView.Add(rightPane);

		Label paneTitle = new Label("PROJECTS");
		leftPane.Add(paneTitle);


		foreach (var projectItems in ProjectButtons())
		{
			leftPane.Add(projectItems);
		}

		//Button enterpriseButton = new Button();
		//enterpriseButton.style.paddingTop = 5;
		//enterpriseButton.text = "ENTERPRISE DATA";
		//enterpriseButton.clicked += EnterpriseButtonClicked;

		//leftPane.Add(enterpriseButton);

		window.rootVisualElement.Add(splitView);

		SetCurrentProjectData();
	}
	private VisualElement QuestionBox(VisualElement question)
	{
		return null;
	}

	//private static void EnterpriseButtonClicked()
	//{
	//	Type windowType = Type.GetType(ECCloudClassName);
	//	EditorWindow.GetWindow(windowType, true, null, true);
	//}

	public static void SetCurrentProjectData()
	{
		rightPane.Clear();
		var activeProject = GraphFinder.CurrentActiveProject;
        if (null != activeProject)
		{
            rightPane.Add(new MenuView(activeProject.packageNameScriptable, activeProject.brandingScriptable));
        }
    }

	public static void ChangeRightPanelData(GraphProjectData graphProjectData)
	{
		rightPane.Clear();
		rightPane.Add(new MenuView(graphProjectData.packageNameScriptable, graphProjectData.brandingScriptable));
	}

	public static List<VisualElement> ProjectButtons()
	{
		buttons = new List<VisualElement>();

		foreach (var item in GraphFinder.GetAllGraphData())
		{
			Button projectButton = new Button();
			projectButton.text = item.mainGraphData.graphGraph.name;

			projectButton.clicked += () => ChangeRightPanelData(item);
			projectButton.clicked += () => UpdateClickedColor(projectButton);
			projectButton.clicked += () => SessionState.SetString(CCK_PS_SELECTEDPROJECT, item.mainGraphData.graphGraph.name);

			projectButton.style.backgroundColor =  item.CurrentActiveProject? blue: darkgrey; 
			projectButton.style.SetBorderColor(Color.clear);
			projectButton.style.unityTextAlign = TextAnchor.MiddleLeft;

			buttons.Add(projectButton);
		}

		return buttons;
	}

	private static void UpdateClickedColor(Button buttonClicked)
    {
        foreach (var item in buttons)
        {
			item.style.backgroundColor = darkgrey;
		}

		buttonClicked.style.backgroundColor = blue;

	}
}
