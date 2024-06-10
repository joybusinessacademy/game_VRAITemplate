using System;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using Scripts.Utility;
using SkillsVRNodes.Managers.Utility;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

namespace SkillsVRNodes.Scripts.CustomWindows
{
	public class AssetHandlerVisualElement : VisualElement, IDisposable
	{
		private const string IS_OPEN = "is-open";

		private GraphProjectData currentProject;
		private Type currentType;
		private VisualElement projectsContainer;
		private VisualElement typesContainer;
		private VisualElement assetContainer;
		private ToolbarSearchField searchBar;

		public static void ResetAllAssets()
		{
			foreach (var handler in AssetHandlerVisualElements)
			{
				handler.ResetAssets();
			}
		}
		
		public static List<AssetHandlerVisualElement> AssetHandlerVisualElements = new();
		
		// Assets not shown in the asset handler
		private static readonly List<Type> DontShowInAssetHandler = new()
		{
			typeof(BaseGraph),
			typeof(MainGraph),
			typeof(SceneGraph),
			typeof(MonoScript),
			typeof(StyleSheet),
			typeof(Sprite),
			typeof(DefaultAsset),
			typeof(PropDataScriptable)
		};

		private static readonly List<Type> TypesToShowInAssetHandler = new()
		{
			typeof(AudioClip),
			typeof(AnimationClip),
			typeof(GameObject),
			typeof(Texture2D),
			typeof(FloatSO),
			typeof(VideoClip)
		};
		
        public AssetHandlerVisualElement()
		{
			AssetHandlerVisualElements.Add(this);
			Resources.Load<VisualTreeAsset>("UXML/AssetHandlerWindow")?.CloneTree(this);
			styleSheets.Add(Resources.Load<StyleSheet>("SkillsStyles/assetHandler"));
			styleSheets.Add(Resources.Load<StyleSheet>("StyleSheets/SkillsVR"));

			this.style.flexGrow = 1;

			Refresh();
			AssetDatabaseFileCache.OnDatabaseChanged += Refresh;

			searchBar = new ToolbarSearchField();
			this.Q<VisualElement>("toolbar").Add(searchBar);
			
			searchBar.RegisterValueChangedCallback(Search);
			
			RegisterCallback<DetachFromPanelEvent>(t => Dispose());
		}

        string searchString = "";
		private void Search(ChangeEvent<string> evt)
		{
			assetContainer ??= this.Q<VisualElement>("assets");
			searchString = evt.newValue.ToLower();
			
			
			UpdateSearchVisibility();
		}

		/// <summary>
		/// Updates the visibility of the assets based on the search string
		/// </summary>
		private void UpdateSearchVisibility()
		{
			IEnumerable<AssetItemVisualElement> items = assetContainer.Children().OfType<AssetItemVisualElement>();

			foreach (AssetItemVisualElement item in items)
			{
				item.style.display = item.ObjectReference.Name.ToLower().Contains(searchString) ? DisplayStyle.Flex : DisplayStyle.None;
			}
		}


		public void Refresh()
		{
			projectsContainer ??= this.Q<VisualElement>("projects");
			projectsContainer.Clear();

			projectsContainer.Add(Button("All Projects", evt => { SwitchProject(); }, currentProject == null));
			
			foreach (GraphProjectData graphProjectData in GraphFinder.GetAllGraphData())
			{
				
				VisualElement newElement = Button(graphProjectData.GetProjectName, evt => { SwitchProject(graphProjectData); }, graphProjectData == currentProject);
				if (newElement != null)
				{
					projectsContainer.Add(newElement);
				}
			}
			
			UpdateTypes();
		}

		private void SwitchProject(GraphProjectData graphProjectData = null)
		{
			currentProject = graphProjectData;
			Refresh();
		}


		public void SwitchType(Type type)
		{
			currentType = type;
			UpdateTypes();
		}

		public void Find()
		{
			this.searchBar.Focus();
		}

		public void UpdateTypes()
		{
			typesContainer ??= this.Q<VisualElement>("types");
			typesContainer.Clear();

			typesContainer.Add(Button("All Assets", evt => { SwitchType(null); }, null == currentType));

			foreach (Type type in TypesToShowInAssetHandler)
			{
				if (DontShowInAssetHandler.Contains(type))
				{
					continue;
				}
				VisualElement newElement = Button(type.Name, evt => { SwitchType(type); }, type == currentType);
				if (newElement != null)
				{
					typesContainer.Add(newElement);
				}
			}

			ResetAssets();
		}
		
		public void ResetAssets()
		{
			assetContainer ??= this.Q<VisualElement>("assets");
			assetContainer.Clear();

			List<ObjectReference> objectReferences = new ();

			if (currentType != null)
			{
				objectReferences = AssetDatabaseFileCache.GetAssetReferencesFromProject(currentType, currentProject);
			}
			
			// Will get all asset references except for blacklisted types
			else
			{
				List<Type> allTypes = TypesToShowInAssetHandler;
				allTypes.RemoveAll(DontShowInAssetHandler.Contains);
				objectReferences = AssetDatabaseFileCache.GetAssetReferencesFromProject(allTypes, currentProject);
			}

			foreach (ObjectReference objectReference in objectReferences)
			{
				assetContainer.Add(new AssetItemVisualElement(objectReference));
			}
			
			if (assetContainer.childCount == 0)
			{
				string text = currentType == null ? "No Assets" : "No Assets of type: " + currentType.Name;
				assetContainer.Add(new Label(text) { name = "empty-text" });
			}

			UpdateSearchVisibility();
		}
		
		public VisualElement Button(string label, EventCallback<ClickEvent> onClick, bool isActive = false)
		{
			VisualElement button = new()
			{
				name = "button"
			};
			VisualElement tickContainer = new()
			{
				name = "tick-container"
			};

			button.Add(tickContainer);
			button.focusable = true;
			
			if (isActive)
			{
				button.AddToClassList(IS_OPEN);
				tickContainer.Add(new Image() { image = Resources.Load<Texture2D>("Icon/Check") });
			}
			else
			{
				button.RemoveFromClassList(IS_OPEN);
			}

			button.Add(new Label(label));

			// Double click
			button.RegisterCallback(onClick);

			return button;
		}

		public void Dispose()
		{
			AssetHandlerVisualElements.Remove(this);
			AssetDatabaseFileCache.OnDatabaseChanged -= Refresh;
		}
	}
}