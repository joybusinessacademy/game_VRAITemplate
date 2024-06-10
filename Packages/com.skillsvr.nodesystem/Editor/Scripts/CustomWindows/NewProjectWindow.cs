using GraphProcessor;
using SkillsVR.VisualElements;
using SkillsVRNodes.Editor.Graph;
using SkillsVRNodes.Managers.Setup;
using System.Collections;
using System.Collections.Generic;
using SkillsVRNodes.Managers.GraphNavigator;
using SkillsVRNodes.Managers.Utility;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Scripts
{
	public class NewProjectWindow : EditorWindow
	{
		private static string projectName;

		private static bool focused = false;

		private static List<string> cachedScenesAssets = new List<string>();

		private void OnGUI()
		{
			if (focused == false)
			{
				rootVisualElement.Focus();
				projectNameTextBox.Focus();
			}

			focused = true;
		}

		private static TextField projectNameTextBox;


		public static void Show()
		{
			focused = false;
			NewProjectWindow window = CreateInstance<NewProjectWindow>();
			window.titleContent = new GUIContent("New Project");
			window.minSize = new Vector2(600, 125);
			window.maxSize = new Vector2(600, 125);
			window.rootVisualElement.name = "new-scene-window";
			window.rootVisualElement.styleSheets.Add(Resources.Load<StyleSheet>("StyleSheets/NewSceneWindow"));

			window.rootVisualElement.Add(new Label("New Project") { name = "heading" });
			window.rootVisualElement.Add(new Divider());

			cachedScenesAssets.Clear();
			cachedScenesAssets = GraphSetupTools.GetAllScenesInProject();

			CreateSceneName(window);
			CreateConfirmButtons(window);

			window.ShowModal();

			ResetValues();
		}

		private static void ResetValues()
		{
			projectName = null;
		}

		private static void CreateSceneName(NewProjectWindow window)
		{
			var subHeading = new Label("") { name = "error-message" };


			window.rootVisualElement.Add(new Label("Project Name") { name = "sub-heading" });
			projectNameTextBox = new TextField();

			projectNameTextBox.RegisterCallback<KeyDownEvent>((evt) =>
			{
				if (evt.keyCode == KeyCode.Return)
				{
					ConfirmAction(window);
				}

				// add this so that when scenenametexbox is focus escape will close it
				if (evt.keyCode == KeyCode.Escape)
				{
					window.Close();
				}
			});
			projectNameTextBox.RegisterCallback<ChangeEvent<string>>(evt =>
			{
				projectName = evt.newValue;
				if (projectName.IsNullOrWhitespace())
				{
					confirmButton.SetEnabled(false);
					subHeading.text = "ERROR Please input a valid name";
				}
				else if (GraphSetupTools.DoesSceneExist(cachedScenesAssets, projectName))
				{
					confirmButton.SetEnabled(false);
					subHeading.text = "ERROR Project or Scene Already Exists";
				}
				else
				{
					confirmButton.SetEnabled(true);
					subHeading.text = "";
				}
			});

			projectNameTextBox.Focus();
			window.rootVisualElement.Add(projectNameTextBox);

			window.rootVisualElement.Add(subHeading);
		}

		private static Button confirmButton;
		private static void CreateConfirmButtons(NewProjectWindow window)
		{
			VisualElement buttons = new()
			{
				name = "buttons"
			};

			window.rootVisualElement.Add(buttons);

			buttons.Add(new Button(window.Close) { text = "Cancel", name = "cancel" });
			confirmButton = new Button(() => ConfirmAction(window)) { text = "Confirm", name = "confirm" };
			confirmButton.SetEnabled(false);
			buttons.Add(confirmButton);
			window.rootVisualElement.Add(buttons);
		}

		public static void ConfirmAction(NewProjectWindow newSceneWindow)
		{
			if (projectName.IsNullOrWhitespace() || GraphSetupTools.DoesSceneExist(cachedScenesAssets, projectName))
			{
				return;
			}

            CCKDebug.Log("Project Item: Creation - Made New Project Called: " + projectName);

			EditorCoroutineUtility.StartCoroutineOwnerless(DelayRefresh(projectName));
			newSceneWindow.Close();
		}

		private static IEnumerator DelayRefresh(string projectNamePass)
		{
			// wait one frame in case of some actions cannot start from init on load callbacks
			// i.e. create new scene when build addressables.
			yield return null;

			MainGraph mainGraph = SetupMainGraph.InitOrNewMainGraphAndScene(projectNamePass);
			//GraphNavigatorVisualElement.ChangeBuildSettingsBasedOnGraphSet(mainGraph);
			
			yield return null;
			
			GraphProjectData data = mainGraph.GetGraphData();
			GraphNavigatorVisualElement.OpenMainGraph(data.mainGraphData.graphGraph, data.mainGraphData.GetGraphScenePath);

		}
	}
}