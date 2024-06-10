using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

using Props;
using Scripts.VisualElements;
using SkillsVR;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using Slider = UnityEngine.UIElements.Slider;

namespace Scripts.Props
{
	public class PropManagerVisualElement : VisualElement, IDisposable
	{
		private const string PREVIEW_SCALE_EDITOR_PREF = "PreviewScale";
		private List<VisualElement> AllVisualElements => scrollView.contentContainer.Children().ToList();
		private ScrollView scrollView;
		private Slider slider;
		private Breadcrumbs breadcrumbs;
		private ToolbarSearchField searchField;
		
		private string currentFolder = "";
		
		private static List<PropComponent> propCache = new();

		public PropManagerVisualElement()
		{
			if (SceneView.lastActiveSceneView == null || SceneView.lastActiveSceneView.rootVisualElement == null)
			{
				return;
			}
			Refresh();
			RegisterCallback<AttachToPanelEvent>(evt => SetupCallbacks());
			RegisterCallback<DetachFromPanelEvent>(evt => Dispose());
		}
		
		

		private void Refresh()
		{
			Clear();
			GenCache();
			
			VisualElement toolbar = new()
			{
				name = "tool-bar"
			};
			searchField ??= new ToolbarSearchField();
			searchField.RegisterValueChangedCallback(evt => UpdateElements(evt.newValue));
			toolbar.Add(searchField);

			Button importButton = new Button();
			importButton.text = "Import";
			Action importAction = ImportDirectorWindow.ShowWindow;
			importButton.clicked += importAction;

			toolbar.Add(importButton);
			
			Add(toolbar);

			VisualElement breadcrumbContainer = new()
			{
				name = "breadcrumbs"
			};
			
			breadcrumbs = new Breadcrumbs(new Dictionary<string, Action> { { "Home", null } });
			breadcrumbContainer.Add(breadcrumbs);
			Add(breadcrumbContainer);
			
			styleSheets.Add(Resources.Load<StyleSheet>("StyleSheets/PropManager"));
			styleSheets.Add(Resources.Load<StyleSheet>("StyleSheets/SkillsVR"));


			scrollView ??= new ScrollView();
			scrollView.contentContainer.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
			scrollView.contentContainer.style.flexWrap = new StyleEnum<Wrap>(Wrap.Wrap);
			scrollView.RegisterCallback<WheelEvent>(evt =>
			{
				if (evt.ctrlKey)
				{
					slider.value -= evt.delta.y * 3;
					evt.StopPropagation();
				}
			});
			Add(scrollView);
			
			VisualElement footer = new()
			{
				name = "footer"
			};
			
			footer.Add(new Label(""));
			slider = new Slider("", 31, 128);
			slider.RegisterValueChangedCallback(evt => ChangeZoomLevel(evt.newValue));
			footer.Add(slider);
			
			Add(footer);
			
			UpdateElements();
			slider.value = EditorPrefs.GetFloat(PREVIEW_SCALE_EDITOR_PREF, 100);
		}

		private void ChangeZoomLevel(float zoomLevel)
		{
			EditorPrefs.SetFloat(PREVIEW_SCALE_EDITOR_PREF, zoomLevel);
			
			float width = zoomLevel + 16;
			float height = zoomLevel + 36;
			bool isSmall = Math.Abs(zoomLevel - 31) < 0.5f;
			
			scrollView.contentContainer.style.flexDirection = isSmall ? new StyleEnum<FlexDirection>(FlexDirection.Column) : new StyleEnum<FlexDirection>(FlexDirection.Row);
			scrollView.contentContainer.style.flexWrap = isSmall ? new StyleEnum<Wrap>(Wrap.NoWrap) : new StyleEnum<Wrap>(Wrap.Wrap);
			
			foreach (VisualElement propVisualElement in AllVisualElements)
			{
				if (isSmall)
				{
					propVisualElement.style.height = new StyleLength(StyleKeyword.Initial);
					propVisualElement.style.width = new StyleLength(StyleKeyword.Initial);
					propVisualElement.AddToClassList("small");
				}
				else 
				{
					propVisualElement.style.height = height;
					propVisualElement.style.width = width;
					propVisualElement.RemoveFromClassList("small");
				}
			}
		}

		public void SetFolder(string folder = "")
		{
			currentFolder = folder;
			Dictionary<string, Action> breadCrumbs;
			if (folder.IsNullOrEmpty())
			{
				currentFolder = "";
				breadCrumbs = new Dictionary<string, Action>
				{
					{ "Home", null }
				};
			}
			else
			{
				breadCrumbs = new Dictionary<string, Action>
				{
					{ "Home", () => SetFolder() },
					{ currentFolder, null },
				};
			}
			breadcrumbs.SetupBreadcrumb(breadCrumbs);
			UpdateElements();
		}

		[InitializeOnLoadMethod]
		public static void GenCache()
		{
			Debug.Log("Refreshing Database Please");

			propCache.Clear();


			propCache = new List<PropComponent>();

			foreach (string assetGUID in AssetDatabaseFileCache.GetAssetGUIDsOfType<GameObject>("Prop"))
			{
				GameObject gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(assetGUID));
				PropComponent propComponent = gameObject.GetComponent<PropComponent>();
				if (propComponent == null)
				{
					propComponent = SetupProp(gameObject);
				}
				propCache.Add(propComponent);
			}
		}
		
		private void UpdateElements(string search = "")
		{
			scrollView.Clear();

			if (search.IsNullOrEmpty() && currentFolder.IsNullOrEmpty())
			{
				foreach (string folder in GetFolders())
				{
					PropVisualElement propVisualElement = new(ObjectNames.NicifyVariableName(folder), () =>
					{
						SetFolder(folder);
						UpdateElements();
					});
					scrollView.Add(propVisualElement);
				}
			}
			else
			{
				foreach (PropComponent propComponent in propCache)
				{
					if (!search.IsNullOrEmpty() && !propComponent.name.ToLower().Contains(search.ToLower()))
					{
						continue;
					}
					if (!currentFolder.IsNullOrEmpty() && propComponent.propType.GetType().Name != currentFolder)
					{
						continue;
					}
					scrollView.Add(new PropVisualElement(propComponent, this));
				}
			}
			
			if (scrollView.contentContainer.childCount == 0)
			{
				scrollView.Add(new Label(search.IsNullOrEmpty() ? "This Folder is Empty" : "No props found") {name = "empty-text"});
			}
			
			ChangeZoomLevel(slider.value);
		}

		public List<string> GetFolders()
		{
			return GetAllTypes().Select(type => type.Name).ToList();
		}
		
		public IEnumerable<Type> GetAllTypes()
		{
			// Get all the types in the assembly
			return AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(s => s.GetTypes())
				.Where(p => typeof(PropType).IsAssignableFrom(p) && !p.IsAbstract && p != typeof(PropType));
		}
		
		private static PropComponent SetupProp(GameObject gameObject)
		{
			PropComponent propComponent;
			string[] allLabels = AssetDatabase.GetLabels(gameObject);
					
			gameObject.AddComponent<PropComponent>();
			propComponent = gameObject.GetComponent<PropComponent>();

			if (allLabels.Contains("Character"))
			{
				propComponent.propType = new CharacterProp(propComponent);
				propComponent.propType.AutoConfigProp();
			}

			return propComponent;
		}

		public void Dispose()
		{
			AssetDatabaseFileCache.OnDatabaseChanged -= RefreshElements;
		}

		private void RefreshElements()
		{
			GenCache();
			UpdateElements(searchField.value);
		}

		public void SetupCallbacks()
		{
			AssetDatabaseFileCache.OnDatabaseChanged += RefreshElements;
		}
	}
}