using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Scripts.VisualElements;
using SkillsVR.VisualElements;
using SkillsVRNodes.Editor.Graph;
using SkillsVRNodes.Managers.Setup;
using SkillsVRNodes.Managers.Utility;
using SkillsVRNodes.Scripts.Nodes;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Scripts
{
	public class NewSceneWindow : EditorWindow
	{
		private static string sceneName;
		private static string environmentScene = "";
		private static SceneNode currentSceneNode;

		private static bool focused = false;

		private static NewSceneWindow window;
		private static AudioClip audioClip;

		private static List<string> cachedScenesAssets = new List<string>();

		private void OnGUI()
		{
			if (focused == false)
			{
				rootVisualElement.Focus();
				sceneNameTextBox.Focus();
			}

			focused = true;
		}

		private static TextField sceneNameTextBox;

		public static void Show(SceneNode sceneNode)
		{
			focused = false;
			currentSceneNode = sceneNode;
			if (currentSceneNode.audioClip != null)
			{
				audioClip = currentSceneNode.audioClip;
			}

			window = CreateInstance<NewSceneWindow>();
			window.titleContent = new GUIContent("New Scene");
			window.minSize = new Vector2(600, 400);
			window.maxSize = new Vector2(600, 400);
			window.rootVisualElement.name = "new-scene-window";
			window.rootVisualElement.styleSheets.Add(Resources.Load<StyleSheet>("StyleSheets/NewSceneWindow"));

			window.rootVisualElement.Add(new Label("New Scene") { name = "heading" });
			window.rootVisualElement.Add(new Divider());

			cachedScenesAssets.Clear();
			cachedScenesAssets = GraphSetupTools.GetAllScenesInProject();

			DropdownField templates = new DropdownField("Templates");
			templates.choices.Add("Coaching conversation");
			templates.choices.Add("Ask me anything");
			templates.value = "-";

			templates.RegisterCallback<ChangeEvent<string>>(evt =>
			{
				sceneNode.templateId = evt.newValue;
				switch (evt.newValue)
				{
					case "Coaching conversation":						
						break;
					case "Ask me anything":
						break;
				}
			});

			CreateEnvironmentButtons(window);
			CreateSceneName(window);

			window.rootVisualElement.Add(templates);
			window.rootVisualElement.Add(new AudioSelector(evt => audioClip = evt, audioClip));

			window.rootVisualElement.Add(new Divider());
			CreateConfirmButtons(window);

			window.ShowModal();

			ResetValues();
		}

		private static void ResetValues()
		{

			currentSceneNode = null;
			sceneName = null;
			environmentScene = "";
		}

		private static void CreateSceneName(NewSceneWindow window)
		{
			List<string> currentSceneNames = GraphFinder.GetAllProjectsSceneNames();

			var subHeading = new Label("") { name = "error-message" };

			window.rootVisualElement.Add(new Label("Scene Name") { name = "sub-heading" });
			sceneNameTextBox = new TextField();

			sceneNameTextBox.RegisterCallback<KeyDownEvent>((evt) =>
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
			sceneNameTextBox.RegisterCallback<ChangeEvent<string>>(evt =>
			{
				sceneName = evt.newValue;
				if (sceneName.IsNullOrWhitespace())
				{
					confirmButton.SetEnabled(false);
					subHeading.text = "ERROR Please input a valid name";
				}
				else if (GraphSetupTools.DoesSceneExist(cachedScenesAssets, sceneName))
				{
					confirmButton.SetEnabled(false);
					subHeading.text = "ERROR Scene Already Exists";
				}
				else
				{
					confirmButton.SetEnabled(true);
					subHeading.text = "";
				}
			});

			sceneNameTextBox.Focus();
			window.rootVisualElement.Add(sceneNameTextBox);

			window.rootVisualElement.Add(subHeading);
		}

		private static void CreateEnvironmentButtons(NewSceneWindow window)
		{
			window.rootVisualElement.Add(new Label("Environment") { name = "sub-heading" });
			window.rootVisualElement.Add(AllAssets());
			window.rootVisualElement.Add(ClearSelection());
		}

		private static Button ClearSelection()
		{
			Button clearSelectionButton = new()
			{
				name = "scene-button",
			};

			clearSelectionButton.text = "Clear Environment Selection";
			clearSelectionButton.style.flexGrow = 1;
			clearSelectionButton.style.flexDirection = FlexDirection.Row;

			clearSelectionButton.clicked += () =>
			{
				environmentScene = string.Empty;
				ClearOtherButtons();
			};

			return clearSelectionButton;
		}

		private static Button confirmButton;
		private static void CreateConfirmButtons(NewSceneWindow window)
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

		public static void ConfirmAction(NewSceneWindow newSceneWindow)
		{
			if (sceneName.IsNullOrWhitespace() || GraphSetupTools.DoesSceneExist(cachedScenesAssets, sceneName))
			{
				return;
			}

			string currentScene = SceneManager.GetActiveScene().path;
			SessionState.SetString("new-scene-path", environmentScene);

			currentSceneNode.additiveScenePaths = environmentScene.IsNullOrWhitespace() ? new List<string>() : new List<string>() { environmentScene };
			currentSceneNode.audioClip = audioClip;

			EditorCoroutineUtility.StartCoroutineOwnerless(DelayStartBuild(sceneName, currentScene, currentSceneNode));

			newSceneWindow.Close();

			EditorUtility.DisplayProgressBar($"Create New Scene {sceneName}", "Init...", 0.0f);
        }

		private static IEnumerator DelayStartBuild(string sceneName, string currentScene, SceneNode currentSceneNode)
		{
            // wait one frame in case of some actions cannot start from init on load callbacks
            // i.e. create new scene when build addressables.
            yield return null;
            EditorUtility.DisplayProgressBar($"Create New Scene {sceneName}", "Making scene asset...", 0.3f);
            currentSceneNode.scenePath = GraphSetupTools.CreateSceneAndAddToBuild(sceneName);
            EditorUtility.DisplayProgressBar($"Create New Scene {sceneName}", "Setup default scene objects...", 0.5f);
            SetupSceneGraph.SetUpScene();

			EditorUtility.DisplayProgressBar($"Create New Scene {sceneName}", "Saving asset...", 0.7f);
            EditorSceneManager.SaveOpenScenes();
            EditorUtility.DisplayProgressBar($"Create New Scene {sceneName}", "Adding scene to project...", 0.9f);
            GraphProjectData graphProjectData = GraphFinder.GetGraphData(GraphFinder.CurrentGraph);
			ProjectController.GenerateSceneProjectData(graphProjectData);

			EditorUtility.SetDirty(graphProjectData);

			SkillsVRGraphWindow.RefreshGraph();

			if (File.Exists(currentScene))
				EditorSceneManager.OpenScene(currentScene);

			AssetDatabase.SaveAssets();

			EditorUtility.ClearProgressBar();
        }

		public static VisualElement AllAssets()
		{
			ScrollView container = new ScrollView()
			{
				name = "all-scenes",
			};
			container.mode = ScrollViewMode.Horizontal;
			var allAssets = AssetDatabase.FindAssets("t:scene l:Environment").Select(AssetDatabase.GUIDToAssetPath).ToList();

			allAssets.Sort((i, j) => String.Compare(Path.GetFileName(i), Path.GetFileName(j), StringComparison.Ordinal));

			foreach (Button assetCreate in allAssets.Select(SceneButton))
			{
				container.contentContainer.Add(assetCreate);
			}

			return container;
		}

		private static void ClearOtherButtons()
		{
			window.rootVisualElement.Query("all-scenes").ForEach(x => {
				x.Query("scene-button").ForEach(x => x.style.backgroundColor = new Color(0.23f, 0.23f, 0.23f, 1));
			});
		}

		private static Button SceneButton(string assetPath)
		{
			Button assetCreate = new(() => environmentScene = assetPath)
			{
				name = "scene-button",
			};
			assetCreate.clicked += () =>
			{
				ClearOtherButtons();
				environmentScene = assetPath;
				assetCreate.style.backgroundColor = new Color(0.17f, 0.36f, 0.52f);
			};

			// Label
			string assetName = Path.GetFileNameWithoutExtension(assetPath);
			assetName = assetName.Replace("_", " ");
			assetName = assetName.Replace("-", " ");
			assetName = ObjectNames.NicifyVariableName(assetName);
			assetCreate.Add(new Label(assetName));

			// Image
			Image icon = new Image
			{
				image = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath.Replace(".unity", ".png")),
				name = "scene-icon"
			};
			assetCreate.Add(icon);

			string spawnPath = assetPath.Replace(".unity", "-spawn.prefab");
			GameObject spawnPoints = AssetDatabase.LoadAssetAtPath<GameObject>(spawnPath);
			if (spawnPoints != null)
			{
				VisualElement horizontal = new();
				horizontal.Add(new Label("Spawn Points: "));
				List<Transform> allChildren = spawnPoints.transform.Cast<Transform>().ToList();

				DropdownField dropdownField = new DropdownField(allChildren.Select(t => t.name).ToList(), 0);
				dropdownField.RegisterValueChangedCallback(evt => { SwitchSpawn(assetPath, evt.newValue, icon); });
				horizontal.Add(dropdownField);
				assetCreate.Add(horizontal);
				SwitchSpawn(assetPath, dropdownField.choices[0], icon);
			}

			return assetCreate;
		}

		private static void SwitchSpawn(string assetPath, string spawnName, Image icon)
		{
			UpdateSpawnPos(assetPath, spawnName);

			string iconPath = Path.GetDirectoryName(assetPath);
			iconPath += "/Spawns/" + spawnName + ".png";
			Texture2D image = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);

			icon.image = image != null ? image : AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath.Replace(".unity", ".png"));
		}

		public static void UpdateSpawnPos(string scenePath, string spawnName)
		{
			SessionState.SetString("new-scene-pos" + scenePath, spawnName);
		}

		public static void MovePlayer(Transform playerObject)
		{
			string scenePath = SessionState.GetString("new-scene-path", "");

			if (scenePath.IsNullOrWhitespace())
			{
				return;
			}

			var posData = GetSpawnPos(scenePath);
			playerObject.transform.position = posData.Key;
			playerObject.transform.rotation = posData.Value;

			SessionState.EraseString("new-scene-path");
		}

		public static KeyValuePair<Vector3, Quaternion> GetSpawnPos(string scenePath)
		{
			string spawnName = SessionState.GetString("new-scene-pos" + scenePath, "");

			if (spawnName.IsNullOrWhitespace())
			{
				return new KeyValuePair<Vector3, Quaternion>();
			}


			string spawnPath = scenePath.Replace(".unity", "-spawn.prefab");
			GameObject spawnPoints = AssetDatabase.LoadAssetAtPath<GameObject>(spawnPath);

			if (spawnPoints == null)
			{
				return new KeyValuePair<Vector3, Quaternion>();
			}

			List<Transform> transforms = spawnPoints.transform.Cast<Transform>().ToList();

			Transform spawnPos = transforms.FirstOrDefault(t => t.name == spawnName);

			if (spawnPos == null)
			{
				return new KeyValuePair<Vector3, Quaternion>();
			}

			return new KeyValuePair<Vector3, Quaternion>(spawnPos.position, spawnPos.rotation);
		}
	}
}