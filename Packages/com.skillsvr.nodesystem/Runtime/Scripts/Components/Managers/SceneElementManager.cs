using System;
using System.Collections.Generic;
using System.Linq;
using SkillsVR.UnityExtenstion;
#if UNITY_EDITOR 
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace SkillsVRNodes
{
	[DefaultExecutionOrder(-1)]
	[RequireComponent(typeof(AudioSource))]
	[Obsolete("All Scene elements and hotspots are now Props. Use Prop instead of SceneElement.")]
	public class SceneElementManager : MonoBehaviour
	{
		[SerializeField] public List<SceneElement> allSceneElements = new();

		public static SceneElementManager instance;
		private AudioSource audioSource;
		public GameObject captionManager;

		public static SceneElementManager Instance
		{
			get
			{
#if UNITY_EDITOR // makes sure that all the static functions still run in edit mode
				return Application.isPlaying ? instance : FindObjectOfType<SceneElementManager>();
#endif
				return instance;
			}
			set => instance = value;
		}

		private void Reset()
		{
			GetComponent<AudioSource>().reverbZoneMix = 0;
		}

		private void Awake()
		{
			if (Instance != null)
			{
				Debug.LogError("Only one SceneElementManager May be active in the scene at a time");
			}

			Instance = this;
			audioSource = GetComponent<AudioSource>();
		}
		
		public static List<T> GetAllSceneElements<T>() where T : SceneElement
		{
			if (Instance == null)
			{
				return null;
			}

			if (Instance.allSceneElements == null || Instance.allSceneElements.Count == 0)
			{
				return null;
			}

			Instance.allSceneElements.RemoveAll(item => item == null);

			return Instance.allSceneElements.Where(sceneElement => sceneElement.GetType() == typeof(T)).Cast<T>().ToList();
		}

		public static T GetSceneElement<T>(string transformName) where T : SceneElement
		{
			if (Instance == null)
			{
				return null;
			}
			if (Instance.allSceneElements == null || Instance.allSceneElements.Count == 0)
			{
				return null;
			}

			return (T)Instance.allSceneElements.Find(t => t.elementName == transformName && t is T);
		}
		
		public static void PlayAudio(AudioClip audioClip)
		{
			Instance.audioSource.clip = audioClip;
			Instance.audioSource.Play();
		}

		public static void DisplayCaption(string text, float duration)
		{
			object[] p = new object[] { text, duration };
			
			Debug.LogWarning("Dialogue: " + text);
			if (Instance.captionManager == null)
			{
				Debug.LogWarning("No Caption Manager Set");
				return;
			}
			Instance.captionManager.SendMessage("DisplayCaption", p);
		}
		
		public static void DisplayCaptionWithName(string name, string text, float duration)
		{
			DisplayCaption(name + ": " + text, duration);
		}


#if UNITY_EDITOR

		/// <summary>
		/// Adds a scene element to the Manager
		/// </summary>
		/// <param name="elementName">The name of the element to be added</param>
		/// <typeparam name="TSceneElement">The type of element</typeparam>
		/// <returns>Returns false if there is already a scene element of that type and that name</returns>
		public static TSceneElement AddElementEditor<TSceneElement>(string elementName) where TSceneElement : SceneElement
		{
			Instance = FindObjectOfType<SceneElementManager>();

			GameObject gameObject = Instance.GetOrAddChild(elementName);
			TSceneElement newSceneElement = gameObject.AddComponent<TSceneElement>();
			Instance.allSceneElements.Add(newSceneElement);

			EditorSceneManager.MarkSceneDirty(gameObject.scene);
			
			return newSceneElement;
		}
		
		public void FindAndAddAllSceneElements()
		{
			SceneElement[] sceneElements = FindObjectsOfType<SceneElement>();

			foreach (SceneElement sceneElement in sceneElements)
			{
				if (sceneElement.gameObject.scene != gameObject.scene)
				{
					continue;
				}

				if (!allSceneElements.Contains(sceneElement))
				{
					allSceneElements.Add(sceneElement);
				}
			}

			allSceneElements.RemoveAll(item => item == null);

			allSceneElements = allSceneElements.Distinct().ToList();
		}

		public static DropdownField GetSceneElementsDropdown<SCENE_ELEMENT>(string elementName, EventCallback<ChangeEvent<string>> callback, string label = "")
			where SCENE_ELEMENT : SceneElement
		{
			if (label == "")
			{
				label = typeof(SCENE_ELEMENT).ToString();
				int lastIndexOf = label.LastIndexOf(".", StringComparison.Ordinal) + 1;
				label = label.Substring(lastIndexOf, label.Length - lastIndexOf);
				label = ObjectNames.NicifyVariableName(label) + ": ";
			}
			
			DropdownField sceneDropdown = new DropdownField
			{
				label = label
			};

			sceneDropdown.choices.Add("none");

			sceneDropdown.value = elementName;

			List<SCENE_ELEMENT> allSceneElements = GetAllSceneElements<SCENE_ELEMENT>();

			if (!(allSceneElements == null || allSceneElements.Count == 0))
			{
				foreach (SCENE_ELEMENT sceneElement in allSceneElements)
				{
					sceneDropdown.choices.Add(sceneElement.name);
				}
			}

			// registers the callback
			sceneDropdown.RegisterCallback(callback);
			return sceneDropdown;
		}
#endif
		public static bool RunUnityEvent(string unityEventName, Action doneCallback)
		{
			SceneUnityEvent sceneElement = GetSceneElement<SceneUnityEvent>(unityEventName);
			if (sceneElement)
			{
				sceneElement.Run(doneCallback);
				return true;
			}

			return false;
		}
	}
}