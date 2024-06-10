using System.Collections;
using SkillsVR.VisualElements;
using SkillsVRNodes.Managers.Utility;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SkillsVRNodes.Editor.Graph;
using SkillsVRNodes.Managers.GraphNavigator;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Scripts
{
	public class DeleteProjectWindow : EditorWindow
	{
		private static Button confirmButton;
		private static string projectName;

		public DeleteProjectWindow()
		{
			titleContent = new GUIContent("Delete Project");
			minSize = new Vector2(300, 100);
			maxSize = new Vector2(300, 100);
		}

		private void OnEnable()
		{
			rootVisualElement.name = "new-scene-window";
			rootVisualElement.styleSheets.Add(Resources.Load<StyleSheet>("StyleSheets/NewSceneWindow"));

			rootVisualElement.Add(new Label("Delete Project") { name = "heading" });
			rootVisualElement.Add(new Divider());

			GraphProjectData project = GraphFinder.CurrentActiveProject;
			if (project != null)
			{
				projectName = project.GetProjectName;
			}
			
			CreateProjectDropdown();
			CreateConfirmButtons();
			
			rootVisualElement.Focus();
		}
		
		public static void ShowWindow()
		{
			var window = CreateInstance<DeleteProjectWindow>();
			window.ShowModal();
		}

		
		private void CreateProjectDropdown()
		{
			List<string> projectNames = new();

			foreach (var item in GraphFinder.GetAllGraphData())
			{
				projectNames.Add(item.mainGraphData.graphGraph.name);
			}

			rootVisualElement.Add(new Label("Project To Delete") { name = "sub-heading" });

			DropdownField dropdownMenu = new("Select Project to Delete", projectNames, 0);
			dropdownMenu.RegisterCallback<ChangeEvent<string>>(evt =>
			{
				projectName = evt.newValue;
			});

			if (projectName.IsNullOrWhitespace())
			{
				projectName = projectNames[0];
			}
			else
			{
				dropdownMenu.SetValueWithoutNotify(projectName);
			}
			
			rootVisualElement.Add(dropdownMenu);
		}

		private void CreateConfirmButtons()
		{
			VisualElement buttons = new()
			{
				name = "buttons"
			};

			rootVisualElement.Add(buttons);

			buttons.Add(new Button(()=> CloseWindows(this)) { text = "Cancel", name = "cancel" });
			confirmButton = new Button(() => ConfirmAction(this)) { text = "Delete", name = "confirm" };
			buttons.Add(confirmButton);
			rootVisualElement.Add(buttons);
		}

		public static void ConfirmAction(DeleteProjectWindow newSceneWindow)
		{
			if (EditorUtility.DisplayDialog("Delete Project?", "Are you sure you want to delete this project?", "Yes", "No"))
			{
				CCKDebug.Log("Project Item: Delete - Project was deleted" + projectName);
				newSceneWindow.Close();

				EditorCoroutineUtility.StartCoroutineOwnerless(DeleteProject(newSceneWindow));
			}
		}

		public static void CloseWindows(DeleteProjectWindow deleteProjectWindow)
		{
			deleteProjectWindow.Close();
		}
		
		private static IEnumerator DeleteProject(DeleteProjectWindow newSceneWindow)
		{
			EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);

			yield return new EditorWaitForSeconds(0.1f);
			
			string sourceFolderPath = Application.dataPath + "/Contexts/" + projectName;

			if (Directory.Exists(sourceFolderPath))
			{
				FileUtil.DeleteFileOrDirectory(sourceFolderPath);
				File.Delete(sourceFolderPath + ".meta");

				AssetDatabase.Refresh();
			}
			else
			{
				Debug.LogError("Folder does not exist: " + sourceFolderPath);
			}

			
			var currentScenes = EditorBuildSettings.scenes;
			var filteredScenes = currentScenes.Where(s => File.Exists(s.path)).ToArray();
			EditorBuildSettings.scenes = filteredScenes;
			
			SkillsVRGraphWindow.RefreshGraph();
			GraphNavigator.RefreshWindow();
		}
	}
}