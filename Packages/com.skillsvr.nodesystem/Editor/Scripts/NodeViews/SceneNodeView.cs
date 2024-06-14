using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using GraphProcessor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scripts.VisualElements;
using SkillsVRNodes.Editor.Graph;
using SkillsVRNodes.Managers;
using SkillsVRNodes.Managers.Setup;
using SkillsVRNodes.Managers.Utility;
using SkillsVRNodes.Scripts;
using SkillsVRNodes.Scripts.Nodes;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using Unity.EditorCoroutines.Editor;
using DialogExporter;
using SFB;
using Props;
using Props.PropInterfaces;

namespace SkillsVRNodes.Editor.NodeViews
{
	[NodeCustomEditor(typeof(SceneNode))]
	public class SceneNodeView : BaseNodeView
	{
		private string graphPathToOpen;
		private static Dictionary<string, string> vectorIdMapping = null;
		private static bool apiIsRequesting = false;

		public bool DisableNewSceneButton
		{
			get
			{
				if (string.IsNullOrWhiteSpace(graphPathToOpen))
				{
					return true;
				}
				BaseGraph graphToOpen = GraphFinder.LoadAssetAtPath<BaseGraph>(graphPathToOpen);
				return null == graphToOpen || string.IsNullOrWhiteSpace(AttachedNode.scenePath);
			}
		}

		public SceneNode AttachedNode => AttachedNode<SceneNode>();

		public override VisualElement GetNodeVisualElement()
		{
			RefreshVectorStore();

			VisualElement visualElement = new();
			ValidateGraphToOpen(out List<string> _);
			
			visualElement.Add(new TextLabel("Scene", GraphFinder.PathToName(AttachedNode.scenePath)));
			visualElement.Add(new TextLabel("Environment", GraphFinder.PathToName(AttachedNode.additiveScenePaths?.FirstOrDefault())));
			visualElement.Add(new TextLabel("Audio", AttachedNode.audioClip ? AttachedNode.audioClip.name : null));
			visualElement.Add(OpenSceneButton());
			return visualElement;
		}

		public void RefreshVectorStore()
        {
			Debug.Log("RefreshVectorStore");
			if (apiIsRequesting)
				return;

			apiIsRequesting = true;
			if (vectorIdMapping == null)
			{
				vectorIdMapping = new Dictionary<string, string>();
				vectorIdMapping.Add("NULL_ID", "None");
				GPTService.GetAllVectorStore(response =>
				{
					apiIsRequesting = false;
					var datas = JObject.Parse(response)["data"];
					datas.ToList().ForEach(i =>
					{
						if (!vectorIdMapping.ContainsKey(i["id"].ToString()))
						{
							vectorIdMapping.Add(i["id"].ToString(), i["name"].ToString());
						}
					});
				});
			}
		}
		public override VisualElement GetInspectorVisualElement()
		{
			var visualElement = new VisualElement();
			
			TextLabel sceneNameLabel = new TextLabel();
			//setLightingButton = new VisualElement();
			
			Button newSceneButton = new()
			{
				text = "Create New Scene",
				name = "NewSceneButton"
			};
			newSceneButton.clicked += () =>
			{
				NewSceneWindow.Show(AttachedNode);
				RefreshNode();
			};
			
			VisualElement additiveScenesVisualElement = new VisualElement
			{
				name = "additive-scene-list"
			};
			
			// adds the main elements
			visualElement.Add(SceneDropdown());
			visualElement.Add(sceneNameLabel);
			visualElement.Add(newSceneButton);
			visualElement.Add(additiveScenesVisualElement);
			visualElement.Add(new AudioSelector( evt => AttachedNode.audioClip = evt, AttachedNode.audioClip));
			visualElement.Add(OpenSceneButton());
			
			UpdateContextLabel(sceneNameLabel);
			
			newSceneButton.style.display = DisableNewSceneButton ? DisplayStyle.Flex : DisplayStyle.None;
			
			sceneNameLabel.SetEnabled(!DisableNewSceneButton);
			SetupAdditiveScenes(additiveScenesVisualElement);
			
			// resets the lighting scene to the base scene if it is not one of the additive scenes
			if (AttachedNode.scenePath != AttachedNode.lightingScene && !AttachedNode.additiveScenePaths.Contains(AttachedNode.lightingScene))
			{
				// Removed Lighting scene options
				//AttachedNode.lightingScene = AttachedNode.scenePath;
			}
			//setLightingButton.Clear();
			//setLightingButton.Add(LightingButton(sceneNode.scenePath, sceneNode));

			// *************************
			// TEMPLATE SECTION
			// *************************
			if (!string.IsNullOrEmpty(AttachedNode.templateId))
			{
				var sceneGraph = GraphFinder.GetGraphData(AttachedNode.Graph).sceneGraphs.Find(a => a.GetGraphAssetPath.Equals(graphPathToOpen)).graphGraph;

				DropdownField npcs = new DropdownField("Main NPC");
				npcs.choices.Add("Dru (Male)");
				npcs.choices.Add("Elle (Female)");
				npcs.value = string.IsNullOrEmpty((sceneGraph as SceneGraph).npcId) ? "Dru (Male)" : (sceneGraph as SceneGraph).npcId;
				npcs.RegisterCallback<ChangeEvent<string>>(evt =>
				{
					(sceneGraph as SceneGraph).npcId = evt.newValue;
				});

				visualElement.Add(new SkillsVR.VisualElements.Divider(10));
				visualElement.Add(npcs);

				var textfield = new TextField();
				textfield.multiline = true;
				textfield.MinHeight(150);
				textfield.WrapText(true);


				textfield.value = AttachedNode.assistantInstruction;

				textfield.RegisterValueChangedCallback(e =>
				{
					AttachedNode.assistantInstruction = e.newValue;
				});

				var uploadBar = new VisualElement();
				Button uploadButton = new() { name = "Upload", text = "+" };

				DropdownField dropdownFiles = new DropdownField("Files");
				dropdownFiles.choices.Add("None");
				if (!vectorIdMapping.ContainsKey(AttachedNode.vectorStoreId))
					AttachedNode.vectorStoreId = "NULL_ID";
				dropdownFiles.value = vectorIdMapping[AttachedNode.vectorStoreId];

				if (vectorIdMapping != null)
				{
					vectorIdMapping.ToList().ForEach(k =>
					{
						dropdownFiles.choices.Add(k.Value);
					});
				}

				dropdownFiles.RegisterCallback <ChangeEvent<string>> (evt =>
				{
					AttachedNode.vectorStoreId = vectorIdMapping.First(k => k.Value.Equals(evt.newValue)).Key;
				});

				uploadButton.clicked += () =>
				{
					var extensions = new[] { new ExtensionFilter("Document Files", "txt", "pdf" ) };

					string[] path = StandaloneFileBrowser.OpenFilePanel("Select Document", "", extensions, false);
					GPTService.UploadFile(path[0], (response) =>	
					{
						GPTService.CreateVectorStore(Path.GetFileName(path[0]),
							(r2) => {
								var id = JObject.Parse(r2)["id"].ToString();
								var name = JObject.Parse(r2)["name"].ToString();

								if (!vectorIdMapping.ContainsKey(name))
									vectorIdMapping[name] = "";

								dropdownFiles.choices.Add(name);
								dropdownFiles.value = name;

							},  JObject.Parse(response)["id"].ToString());

							
					});
				};


				dropdownFiles.style.flexGrow = 1;
				dropdownFiles.style.flexDirection = FlexDirection.Row;
				dropdownFiles.style.flexGrow = 1;
				dropdownFiles.style.flexShrink = 1;
				dropdownFiles.style.width = 50;

				uploadBar.style.flexDirection = FlexDirection.Row;
				uploadBar.Add(dropdownFiles);
				uploadBar.Add(uploadButton);
				
				visualElement.Add(uploadBar);

				Button generateButton = new() { name = "Generate", text = "Generate" };
				generateButton.clicked += () =>
				{
					List<BaseNode> nodes = new List<BaseNode>();

					// do the graph here
					var sceneGraph = GraphFinder.GetGraphData(AttachedNode.Graph).sceneGraphs.Find(a => a.GetGraphAssetPath.Equals(graphPathToOpen)).graphGraph;

					sceneGraph.nodes.RemoveAll(k => true);
					if (sceneGraph.nodes.Find(k => k is StartNode) == null)
						sceneGraph.AddNode(BaseNode.CreateFromType(typeof(StartNode), Vector2.zero));

					if (sceneGraph.nodes.Find(k => k is EndNode) == null)
						sceneGraph.AddNode(BaseNode.CreateFromType(typeof(EndNode), Vector2.zero));

					var startNode = sceneGraph.nodes.Find(k => k is StartNode);
					nodes.Add(startNode);
					var startOutport = startNode.outputPorts.Find(a => a.fieldName.Equals("executes"));


					// self introduction
					var intro = sceneGraph.AddNode(BaseNode.CreateFromType(typeof(DialogueNode), Vector2.zero));
					nodes.Add(intro);
					var introOutport = intro.outputPorts.Find(a => a.fieldName.Equals("Complete"));
					var introInport = intro.inputPorts.Find(a => a.fieldName.Equals("executed"));

					/*
					// information about the topic
					var aboutthetopic = sceneGraph.AddNode(BaseNode.CreateFromType(typeof(DialogueNode), Vector2.zero));
					nodes.Add(aboutthetopic);
					var aboutOutport = aboutthetopic.outputPorts.Find(a => a.fieldName.Equals("Complete"));
					var aboutInport = aboutthetopic.inputPorts.Find(a => a.fieldName.Equals("executed"));

					// what would you like to know?
					var whatwouldyouliketoknow = sceneGraph.AddNode(BaseNode.CreateFromType(typeof(DialogueNode), Vector2.zero));
					nodes.Add(whatwouldyouliketoknow);
					var whatyouliketoknowOutport = whatwouldyouliketoknow.outputPorts.Find(a => a.fieldName.Equals("Complete"));
					var whatyouliketoknowInport = whatwouldyouliketoknow.inputPorts.Find(a => a.fieldName.Equals("executed"));
					*/

					// gpt port
					var gptNode = sceneGraph.AddNode(BaseNode.CreateFromType(typeof(GPTNode), Vector2.zero));
					var gptOutport = gptNode.outputPorts.Find(a => a.fieldName.Equals("Complete"));
					var gptInport = gptNode.inputPorts.Find(a => a.fieldName.Equals("executed"));
					nodes.Add(gptNode);

					var endNode = sceneGraph.nodes.Find(k => k is EndNode);
					nodes.Add(endNode);
					var endInport = endNode.inputPorts.Find(a => a.fieldName.Equals("executed"));

					sceneGraph.Connect(introInport, startOutport);
					sceneGraph.Connect(gptInport, introOutport); 

					for (int i = 1; i < nodes.Count; i++)
						nodes[i].position.position = nodes[i - 1].position.position + new Vector2(nodes[i-1].Width + 100, 0);

					ElevenLabsService.voiceId = (sceneGraph as SceneGraph).npcId.Equals("Dru (Male)") ? "ZY37LYw0WtCyedeNw2EV" : "XfNU2rGpBa01ckF309OY";
					EditorCoroutineUtility.StartCoroutineOwnerless(CreateExperience(dropdownFiles.text));

					// open scene
					// generate npc after
					var druGUID = AssetDatabase.FindAssets($"_Dru t:Prefab").First();
					var elleGUID = AssetDatabase.FindAssets($"_Elle t:Prefab").First();
					EditorSceneManager.OpenScene(AttachedNode.scenePath);

					// find older model 
					GameObject.DestroyImmediate(GameObject.Find("Dru (Male)"));
					GameObject.DestroyImmediate(GameObject.Find("Elle (Female)"));

					PropManager.Validate();

					GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath((sceneGraph as SceneGraph).npcId.Equals("Dru (Male)") ? druGUID : elleGUID));
					var npc = GameObject.Instantiate(prefab);
					npc.name = (sceneGraph as SceneGraph).npcId;

					PropManager.AddElementEditor(npc.GetComponent<PropComponent>());

					(intro as DialogueNode).dialoguePosition = new PropGUID<IPropAudioSource> { propGUID = PropManager.GetGUIDByName<IPropAudioSource>(npc.name)?.propGUID };

					npc.transform.position = new Vector3(0, 0, 2);
					npc.transform.eulerAngles = new Vector3(0, 180, 0);


					(gptNode as GPTNode).dialoguePosition = new PropGUID<IPropAudioSource> { propGUID = PropManager.GetGUIDByName<IPropAudioSource>(npc.name)?.propGUID };
					EditorSceneManager.MarkSceneDirty(npc.scene);
					AssetDatabase.SaveAssets();

					EditorSceneManager.SaveOpenScenes();
					EditorSceneManager.OpenScene(AttachedNode.Graph.GetDefaultGraphScenePath());

				};

				visualElement.Add(new Label("Persona"));
				visualElement.Add(textfield);				
				visualElement.Add(generateButton);
			}


			SendCustomEvent("RefreshGraphWindow", this);

			return visualElement;
		}

		IEnumerator CreateExperience(string vectorStoreId)
        {
			AttachedNode.assistantId = string.Empty;
			var threadId = string.Empty;

			var sceneGraph = GraphFinder.GetGraphData(AttachedNode.Graph).sceneGraphs.Find(a => a.GetGraphAssetPath.Equals(graphPathToOpen)).graphGraph;
			EditorUtility.DisplayProgressBar($"Generating", "Creating assistant...", 0.1f);

			// create assistant
			GPTService.CreateAssistant(AttachedNode.assistantInstruction, vectorStoreId, (response) => {
				JObject data = JObject.Parse(response);
				AttachedNode.assistantId = data["id"].ToString();
				
				(sceneGraph as SceneGraph).assistantId = AttachedNode.assistantId;
			});
		

			// create thread
			GPTService.CreateThread((response) => {
				JObject data = JObject.Parse(response);
				threadId = data["id"].ToString();
			});

			yield return new WaitUntil(() => !string.IsNullOrEmpty(threadId) && !string.IsNullOrEmpty(AttachedNode.assistantId));


			EditorUtility.DisplayProgressBar($"Generating", "Creating voice-overs... (1/6)", 0.3f);
			List<object[]> clips = new List<object[]>();
			yield return EditorCoroutineUtility.StartCoroutineOwnerless(AddMessageAndCreateClip(threadId, (sceneGraph as SceneGraph).npcId.ToLower().Contains("female") ?
				"Short introduction about yourself in a conversational format. Give yourself a random female name. Limit to 30 - 50 words" :
				"Short introduction about yourself in a conversational format. Give yourself a random male name. Limit to 30 - 50 words",
				(response) => {
					(sceneGraph as SceneGraph).assistantInstruction = AttachedNode.assistantInstruction + "\n Your introduction was " + (response[0] as ElevenLabsService.QueuedRequestParameter).text;
					clips.Add(response);
				}));


			EditorUtility.DisplayProgressBar($"Generating", "Creating voice-overs (2/6)...", 0.4f);
			yield return EditorCoroutineUtility.StartCoroutineOwnerless(AddMessageAndCreateClip(threadId, "give me 1example of filler responses atleast 2 sentences that is generic when responding to a conversation it should not be a question, you will respond with just the example nothing else.",
					(response) => {
						clips.Add(response);
					}));

			EditorUtility.DisplayProgressBar($"Generating", "Creating voice-overs (3/6)...", 0.5f);
			yield return EditorCoroutineUtility.StartCoroutineOwnerless(AddMessageAndCreateClip(threadId, "give me 1example of filler responses atleast 2 sentences that is generic when responding to a conversation it should not be a question, you will respond with just the example nothing else.",
					(response) => {
						clips.Add(response);
					}));

			EditorUtility.DisplayProgressBar($"Generating", "Creating voice-overs (4/6)...", 0.6f);
			yield return EditorCoroutineUtility.StartCoroutineOwnerless(AddMessageAndCreateClip(threadId, "give me 1example of filler responses atleast 2 sentences that is generic when responding to a conversation it should not be a question, you will respond with just the example nothing else.",
					(response) => {
						clips.Add(response);
					}));
			EditorUtility.DisplayProgressBar($"Generating", "Creating voice-overs (5/6)...", 0.7f);
			yield return EditorCoroutineUtility.StartCoroutineOwnerless(AddMessageAndCreateClip(threadId, "give me 1example of filler responses atleast 2 sentences that is generic when responding to a conversation it should not be a question, you will respond with just the example nothing else.",
					(response) => {
						clips.Add(response);
					}));
			EditorUtility.DisplayProgressBar($"Generating", "Creating voice-overs (6/6)...", 0.8f);
			yield return EditorCoroutineUtility.StartCoroutineOwnerless(AddMessageAndCreateClip(threadId, "give me 1example of filler responses atleast 2 sentences that is generic when responding to a conversation it should not be a question, you will respond with just the example nothing else.",
					(response) => {
						clips.Add(response);
					}));

			// all files complete
			// refresh all together
			EditorUtility.DisplayProgressBar($"Generating", "Finalizing...", 0.95f);
			AssetDatabase.Refresh();
			bool first = true;
			var gptNode = sceneGraph.nodes.Find(k => k is GPTNode) as GPTNode;
			var introDialog = sceneGraph.nodes.Find(k => k is DialogueNode) as DialogueNode;
			clips.ForEach(k =>
			{
				var queue = k[0] as ElevenLabsService.QueuedRequestParameter;
				var dialog = ScriptableObjectManager.CreateScriptableObject<LocalizedDialog>(ScriptableObjectManager.Path() + "/" + sceneGraph.name, Path.GetFileName(queue.filePath));
				dialog.Dialog = k[1] as string;
				queue.text = dialog.Dialog;
				
				dialog.SetCustomAudio(AssetDatabase.LoadAssetAtPath<AudioClip>(queue.filePath.Replace(Application.dataPath, "Assets")) as AudioClip);
				ScriptableObjectManager.ForceSerialization(dialog);

				if (first == false)
					gptNode.fillerDialogs.Add(dialog);

				if (first)
				{
					first = false;
					introDialog.dialogueAsset = dialog;
				}
			});

			EditorUtility.ClearProgressBar();

		}

		IEnumerator AddMessageAndCreateClip(string threadId, string message, System.Action<object[]> onComplete)
		{
			// create self introduction
			GPTService.AddMessageToThread(threadId, message, (response) => {

			});

			string runId = string.Empty;
			GPTService.ThreadRun(threadId, AttachedNode.assistantId, null, (response) => {
				JObject data = JObject.Parse(response);
				runId = data["id"].ToString();
				threadId = data["thread_id"].ToString();
			});

			yield return new WaitUntil(() => !string.IsNullOrEmpty(runId));

			string status = string.Empty;
			// poll run status
			while (!status.Equals("completed"))
			{
				bool responsed = false;
				GPTService.RetrieveRun(threadId, runId, (response) =>
				{
					JObject data = JObject.Parse(response);
					status = data["status"].ToString();
					responsed = true;
				});

				yield return new WaitUntil(() => responsed == true);
				yield return new WaitForSeconds(1);
			}

			bool requestCompleted = false;
			GPTService.GetMessages(threadId, AttachedNode.assistantId, (response) =>
			{
				JObject data = JObject.Parse(response);
				string value = data["data"][0]["content"][0]["text"]["value"].ToString();
				var sceneGraph = GraphFinder.GetGraphData(AttachedNode.Graph).sceneGraphs.Find(a => a.GetGraphAssetPath.Equals(graphPathToOpen)).graphGraph;
				var path = ScriptableObjectManager.Path() + "/" + sceneGraph.name + "/clips/" + System.DateTime.Now.Ticks.ToString() + ".mp3";

				var queue = new ElevenLabsService.QueuedRequestParameter();
				queue.text = value;
				queue.filePath = path;		
				
				queue.onComplete = (response) => {					
					onComplete.Invoke(new object[] {queue , value});
					requestCompleted = true;
				};
				
				ElevenLabsService.Request(queue, true);
			});

			yield return new WaitUntil(() => requestCompleted == true);
		}

		/// <summary>
		/// Creates the dropdown for the scene
		/// </summary>
		/// <returns></returns>
		private VisualElement SceneDropdown()
		{
			// Scene dropdown
			DropdownField sceneDropdown = new()
			{
				label = "Scene"
			};

			List<string> allScenePath = ContextSceneManager.GetAllContextScenePath().ToList();
			allScenePath.Insert(0, "none");
			allScenePath.Remove(allScenePath.Find(t => t.Contains("MainGraph")));

			foreach (var item in GraphFinder.GetAllGraphData())
			{
				if(allScenePath.Exists(x => x == item.mainGraphData.GetGraphScenePath))
					allScenePath.Remove(item.mainGraphData.GetGraphScenePath);
			}

			sceneDropdown.choices = allScenePath.Select(GraphFinder.PathToName).ToList();
			sceneDropdown.value = AttachedNode.scenePath == "" ? "none" : GraphFinder.PathToName(AttachedNode.scenePath);
			sceneDropdown.RegisterCallback<ChangeEvent<string>>(_ =>
			{
				AttachedNode.scenePath = 0 == sceneDropdown.index ? "" : allScenePath[sceneDropdown.index];
                RefreshNode();
			});

			//sceneSelector.Add(setLightingButton);
			return sceneDropdown;
		}


		/// <summary>
		/// Updates the text below the scene to display either what graph is attached or what the error is
		/// </summary>
		/// <param name="sceneNode"></param>
		private void UpdateContextLabel(TextLabel sceneNameLabel)
		{
			sceneNameLabel.style.height = AttachedNode.scenePath == "" ? 0 : 20;
			sceneNameLabel.visible = !string.IsNullOrWhiteSpace(AttachedNode.scenePath);
			
			// Error checker
			GraphFinder.SceneGraphError sceneError = ValidateGraphToOpen(out List<string> graphPathList);
			if (sceneError != GraphFinder.SceneGraphError.None)
			{
				sceneNameLabel.Label = "Error";
				sceneNameLabel.Text = ObjectNames.NicifyVariableName(sceneError.ToString());
				graphPathToOpen = null;
			}
			else
			{
				graphPathToOpen = graphPathList?.FirstOrDefault();
                sceneNameLabel.Label = "Graph: ";
                sceneNameLabel.Text = GraphFinder.PathToName(graphPathToOpen);
			}
		}

		public GraphFinder.SceneGraphError ValidateGraphToOpen(out List<string> graphPathList)
		{
			GraphFinder.SceneGraphError sceneError = GraphFinder.CheckSceneForErrors(AttachedNode.scenePath, out graphPathList);

			graphPathToOpen = sceneError != GraphFinder.SceneGraphError.None ? null : graphPathList?.FirstOrDefault();

			return sceneError;
		}
		
		/// <summary>
		/// refreshes the additive scene visual elements 
		/// </summary>
		/// <param name="sceneNode">the node this is attached to</param>
		private void SetupAdditiveScenes(VisualElement additiveScenesVisualElement)
		{
			additiveScenesVisualElement.Clear();
			var dropdown = new DropdownField
			{
				label = "Additive Scenes",
				name = "additive-scenes-dropdown"
			};
			string scenePath = AttachedNode?.additiveScenePaths?.FirstOrDefault();
			scenePath = scenePath.IsNullOrWhitespace() ? "None" : GraphFinder.PathToName(scenePath);
			
			dropdown.value = scenePath;
			dropdown.choices = GraphFinder.EnvironmentScenes.Select(GraphFinder.PathToName).ToList();
			dropdown.choices.Insert(0, "None");
			dropdown.choices.Insert(1, "");
			dropdown.RegisterValueChangedCallback(evt =>
			{
				if (evt.newValue == "None")
				{
					AttachedNode.additiveScenePaths = new List<string>();
					RefreshNode();
					return;
				}
				string path = GraphFinder.EnvironmentScenes.Find(t => GraphFinder.PathToName(t) == evt.newValue);
				AttachedNode.additiveScenePaths = new List<string> { path };
				RefreshNode();
			});
			additiveScenesVisualElement.Add(dropdown);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sceneNode"></param>
		/// <returns>Returns the visual element</returns>
		private VisualElement OpenSceneButton()
		{
			// OpenSceneButton
			Button openSceneButton = new()
			{
				name = "open-scene-button",
				text = "Open"
			};
			openSceneButton.SetEnabled(!DisableNewSceneButton);
			openSceneButton.clicked += () =>
			{
				if (string.IsNullOrWhiteSpace(graphPathToOpen))
				{
					return;
				}
				BaseGraph graphToOpen = GraphFinder.LoadAssetAtPath<BaseGraph>(graphPathToOpen);
				if (null == graphToOpen)
				{
					return;
				}
				OpenScene(graphToOpen, AttachedNode.scenePath,
					AttachedNode.lightingScene, AttachedNode.additiveScenePaths);
                owner.OpenGraph(graphToOpen);
            };


			return openSceneButton;
		}

		public static void OpenScene(BaseGraph graph, string mainScene, string lightingScene, List<string> scenePaths = null)
		{
			if (!Application.isPlaying && EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
			{
				LoadSceneList(mainScene, lightingScene, scenePaths);
			}
		}

		public static void LoadSceneList(string mainScene, string lightingScene, List<string> scenePaths)
		{
			List<string> list = new(scenePaths) { mainScene };

			Scene loadedLightingScene = EditorSceneManager.OpenScene(lightingScene);
			list.Remove(lightingScene);

			List<Scene> openScenes = new();
			
			// load remaining scene list
			foreach (string scene in list)
			{
				openScenes.Add(EditorSceneManager.OpenScene(scene, OpenSceneMode.Additive));
			}

			Scene lightingSceneItem = SceneManager.GetSceneByPath(lightingScene);
			if (lightingSceneItem.IsValid())
			{
				SceneManager.SetActiveScene(lightingSceneItem);
			}
			
			
			if (scenePaths.Count == 0)
			{
				return;
			}
			
			// All below here is for making a collider for the objects in the scene
			
			// Adds the lighting scene to loaded scenes

			List<Scene> physicsScenes = openScenes;
			physicsScenes.Add(loadedLightingScene);
			
			// removes the main scene
			physicsScenes.RemoveIfFound(openScenes.Find(scene => scene.path == mainScene));
			
			CreatePhysicsScene(physicsScenes);
			SetupSceneGraph.ValidateSceneElements();
		}

		private static void CreatePhysicsScene(List<Scene> physicsScenes)
		{
			if (physicsScenes.Count == 0)
			{
				return;
			}
			
			Scene currentScene = SceneManager.GetActiveScene();
			Scene colliderScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

			colliderScene.name = "Editor Scene";
			
			List<GameObject> gameObjects = new();
			foreach (Scene openScene in physicsScenes.Distinct().Where(openScene => openScene.IsValid()))
			{
				gameObjects.AddRange(openScene.GetRootGameObjects());
			}

			GameObject go = new("All Physics Objects");
			SceneManager.MoveGameObjectToScene(go, colliderScene);
			Directory.CreateDirectory(Application.dataPath + "/Editor");
			EditorSceneManager.SaveScene(colliderScene, Application.dataPath + "/Editor/PhysicsScene.unity");

			foreach (GameObject gameObject in gameObjects)
			{
				foreach (MeshFilter meshFilter in gameObject.GetComponentsInChildren<MeshFilter>())
				{
					GameObject newGo = new(meshFilter.gameObject.name)
					{
						transform =
						{
							position = meshFilter.gameObject.transform.position,
							rotation = meshFilter.gameObject.transform.rotation,
							localScale = meshFilter.gameObject.transform.localScale,
							parent = go.transform
						}
					};
					
					MeshCollider collider = newGo.AddComponent<MeshCollider>();
					collider.sharedMesh = meshFilter.sharedMesh;

					int layerIndex = LayerMask.NameToLayer("Editor");

					if (layerIndex != -1)
					{
						collider.gameObject.layer = LayerMask.NameToLayer("Editor");
					}
					else
					{
						SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
						SerializedProperty layers = tagManager.FindProperty("layers");

						for (int i = 18; i < layers.arraySize; i++)
						{
							SerializedProperty layerSP = layers.GetArrayElementAtIndex(i);

							if (layerSP.stringValue == "")
							{
								layerSP.stringValue = "Editor";
								tagManager.ApplyModifiedProperties();
								return;
							}
						}
					}
				}
			}
			
			SceneManager.SetActiveScene(currentScene);
		}
	}
}
