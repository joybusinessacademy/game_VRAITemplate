using GraphProcessor;
using SkillsVR.VisualElements;
using SkillsVRNodes.Editor.Graph;
using SkillsVRNodes.Managers.Setup;
using SkillsVRNodes.Managers.Utility;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Scripts
{
	public class ExportProjectWindow : EditorWindow
	{
		private static string projectName;

		private static bool focused = false;

		private void OnGUI()
		{
			if (focused == false)
			{
				rootVisualElement.Focus();
			}

			focused = true;
		}

		public static void Show()
		{
			focused = false;
			ExportProjectWindow window = CreateInstance<ExportProjectWindow>();
			window.titleContent = new GUIContent("Export Project");
			window.minSize = new Vector2(600, 125);
			window.maxSize = new Vector2(600, 125);
			window.rootVisualElement.name = "new-scene-window";
			window.rootVisualElement.styleSheets.Add(Resources.Load<StyleSheet>("StyleSheets/NewSceneWindow"));

			window.rootVisualElement.Add(new Label("Export Project") { name = "heading" });
			window.rootVisualElement.Add(new Divider());

			ExportProjectDropdown(window);
			CreateConfirmButtons(window);

			window.ShowModal();

		}

		private static void ExportProjectDropdown(ExportProjectWindow window)
		{
			List<string> projectNames = new List<string>();

			foreach (var item in GraphFinder.GetAllGraphData())
			{
				projectNames.Add(item.mainGraphData.graphGraph.name);
			}

			window.rootVisualElement.Add(new Label("Project To Export") { name = "sub-heading" });

			DropdownField dropdownMenu = new DropdownField("Select Project to Export", projectNames, 0);
			dropdownMenu.RegisterCallback<ChangeEvent<string>>(evt =>
			{
				projectName = evt.newValue;
			});

			projectName = projectNames[0];

			window.rootVisualElement.Add(dropdownMenu);
		}

		private static Button confirmButton;
		private static void CreateConfirmButtons(ExportProjectWindow window)
		{
			VisualElement buttons = new()
			{
				name = "buttons"
			};

			window.rootVisualElement.Add(buttons);

			buttons.Add(new Button(window.Close) { text = "Cancel", name = "cancel" });
			confirmButton = new Button(() => ConfirmAction(window)) { text = "Export", name = "confirm" };
			buttons.Add(confirmButton);
			window.rootVisualElement.Add(buttons);
		}

		public static void ConfirmAction(ExportProjectWindow newSceneWindow)
		{
			DuplicateFolder();

            CCKDebug.Log("Project Item: Export - Exporting Project");

			EditorCoroutineUtility.StartCoroutineOwnerless(DelayRefresh());
			newSceneWindow.Close();
		}

		private static IEnumerator DelayRefresh()
		{
			yield return null;
			SkillsVRGraphWindow.RefreshGraph();
		}


		public static void DuplicateFolder()
		{
			string sourceFolderPath = "";
			string destinationFolderPath = "";

			sourceFolderPath = Application.dataPath + "/Contexts/" + projectName;

			if (Directory.Exists(sourceFolderPath))
			{
				destinationFolderPath = EditorUtility.SaveFolderPanel("Choose Location to export project", "", "");

				if (destinationFolderPath == "")
				{
					return;
				}

				destinationFolderPath = destinationFolderPath + "/" + projectName;
				if (!Directory.Exists(destinationFolderPath))
				{
					Directory.CreateDirectory(destinationFolderPath);
				}

				// Get a list of all files and subdirectories in the source folder.
				string[] files = Directory.GetFiles(sourceFolderPath, "*", SearchOption.AllDirectories);

				foreach (string filePath in files)
				{
					// Calculate the relative path of each file from the source folder.
					string relativePath = filePath.Substring(sourceFolderPath.Length + 1);

					// Create the full path to the corresponding file in the destination folder.
					string destinationFilePath = Path.Combine(destinationFolderPath, relativePath);

					// Create the directory structure in the destination folder if it doesn't exist.
					Directory.CreateDirectory(Path.GetDirectoryName(destinationFilePath));

					// Copy the file from the source to the destination.
					File.Copy(filePath, destinationFilePath, true);
				}

				Debug.Log("Folder duplicated successfully.");
			}
			else
			{
				Debug.LogError("Source folder does not exist.");
			}
		}
	}
}
