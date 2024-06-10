using System;
using System.Collections.Generic;
using System.Linq;
using Props.PropInterfaces;
using UnityEngine.UIElements;
using UnityEngine;
#if UNITY_EDITOR 
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

namespace Props
{
	[Serializable]
	public class PropReferences
	{
		public PropReferences(PropComponent propComponent)
		{
			PropComponent = propComponent;
			GUID = new PropGUID<IBaseProp>();
		}


		public PropGUID<IBaseProp> GUID;
		public PropComponent PropComponent;
	}
	
	[DefaultExecutionOrder(-1)]
	[RequireComponent(typeof(AudioSource))]
	public class PropManager : MonoBehaviour
	{
		
		[SerializeField] public List<PropReferences> allProps = new();

		public static PropManager instance;
		private AudioSource audioSource;
		public GameObject captionManager;

		public static PropManager Instance
		{
			get
			{
#if UNITY_EDITOR // makes sure that all the static functions still run in edit mode
				return Application.isPlaying ? instance : FindObjectOfType<PropManager>();
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
				Debug.LogWarning("Only one PropManager May be active in the scene at a time");
			}

			Instance = this;
			audioSource = GetComponent<AudioSource>();
		}
		
		// TODO: THIS NEEDS TO BE REDONE
		// Should be typed to the specific type of prop
		public static List<PropReferences> GetAllProps()
		{
			if (Instance == null || Instance.allProps.IsNullOrEmpty() )
			{
				return null;
			}
			
			return Instance.allProps;
		}
		
		public static List<PropReferences> GetAllProps<TPropType>() where TPropType : IBaseProp
		{
			if (Instance == null || Instance.allProps.IsNullOrEmpty() )
			{
				return null;
			}

			//Remove any Null Props from Instance
			Instance.allProps?.RemoveAll(item => item == null);
			Instance.allProps?.RemoveAll(item => item.PropComponent == null);

			return Instance.allProps?.FindAll(t => t.PropComponent.propType is TPropType);
		}

		public static void Validate()
		{
			if (Instance == null)
			{
				return;
			}
			Instance.OnValidate();
		}
	
		public static bool TrySetPropName(PropComponent propComponent, string name)
		{
			if (!CanSetName(name))
			{
				return false;
			}

			PropReferences propReferences = GetAllProps().Find(t => t.PropComponent == propComponent);
			propReferences.PropComponent.PropName = name;
			propComponent.gameObject.name = name;
			return true;
		}

		public static bool CanSetName(string name)
		{
			return GetAllProps().Find(t => t.PropComponent.PropName == name) == null;
		}

		public void OnValidate()
		{
			allProps.RemoveAll(item => item == null);
			allProps.RemoveAll(item => item.PropComponent == null);
			allProps.RemoveAll(item => item.PropComponent.gameObject == null);
			allProps.RemoveAll(item => item.PropComponent.gameObject.scene != gameObject.scene);
			
			allProps = allProps.Distinct(new ProductComparer()).ToList();

#if UNITY_EDITOR
			List<PropReferences> tempAllProps = new();
			foreach (PropReferences prop in allProps)
			{
				if (tempAllProps.Where(t => t.PropComponent.PropName == prop.PropComponent.PropName).IsNullOrEmpty())
				{
					tempAllProps.Add(prop);
					continue;
				}
				
				prop.PropComponent.PropName = ObjectNames.GetUniqueName(tempAllProps.Select(t => t.PropComponent.PropName).ToArray(), prop.PropComponent.PropName);
			}
#endif
		}
		
		// Custom comparer for the Product class
		class ProductComparer : IEqualityComparer<PropReferences>
		{
			// Products are equal if their names and product numbers are equal.
			public bool Equals(PropReferences x, PropReferences y)
			{
				//Check whether the compared objects reference the same data.
				if (ReferenceEquals(x, y))
				{
					return true;
				}
				//Check whether any of the compared objects is null.
				if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
				{
					return false;
				}
				//Check whether the products' properties are equal.
				return x.PropComponent == y.PropComponent && Equals(x.GUID, y.GUID);
			}
			
			public int GetHashCode(PropReferences product)
			{
				//Check whether the object is null
				if (ReferenceEquals(product, null))
				{
					return 0;
				}

				//Get hash code for the Name field if it is not null.
				int hashProductName = product.GUID == null ? 0 : product.GUID.GetHashCode();

				//Get hash code for the Code field.
				int hashProductCode = product.PropComponent.GetHashCode();

				//Calculate the hash code for the product.
				return hashProductName ^ hashProductCode;
			}
		}
		
		public static TPropType GetProp<TPropType>(PropGUID<TPropType> propGUID) where TPropType : class, IBaseProp
		{
			PropReferences prop = GetAllProps<TPropType>()?.Find(t => t?.GUID.propGUID == propGUID.propGUID);
			return prop?.PropComponent.propType as TPropType;
		}
		
		public static PropGUID<IBaseProp> GetGUIDByName<TPropType>(string propName) where TPropType : class, IBaseProp
		{
			PropReferences test = GetAllProps<TPropType>()?.Find(t => t.PropComponent.PropName == propName);
			return test == null ? string.Empty : test.GUID;
		}
		
		public static string GetPropName<TPropType>(PropGUID<TPropType> propGuid) where TPropType : class, IBaseProp
		{
			if(propGuid == null)
				return string.Empty;
			else
				return GetProp(propGuid)?.GetPropComponent().PropName;
		}

		public static string GetPropName(string propGuid)
		{
			PropReferences prop = GetAllProps()?.Find(t => t?.GUID.propGUID == propGuid);
			return prop?.PropComponent.propType?.GetPropComponent().PropName;
		}

		public static void PlayAudio(AudioClip audioClip)
		{
			Instance.audioSource.clip = audioClip;
			Instance.audioSource.Play();
		}

		public static AudioSource GetAudioSource()
		{
			return Instance.audioSource;
		}

		public static void DisplayCaption(string text, float duration)
		{
			object[] p = { text, duration };
			
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

		private static Action onPropListChanged = () => { };
		
		public static void OnPropListChanged(Action action)
		{
			if (null == action)
			{
				return;
			}
			onPropListChanged += action;
		}

		
		public static void RemovePropListChangedEvent(Action action)
		{
			if (null == action)
			{
				return;
			}
			onPropListChanged -= action;
		}


		public static void PropListChanged()
		{
			Validate();
			onPropListChanged?.Invoke();
		}

#if UNITY_EDITOR
		public void CreateNewPropFromGraph(string propName, Type basePropType)
		{
			if (!typeof(PropType).IsAssignableFrom(basePropType))
			{
				return;
			}

			propName = ObjectNames.GetUniqueName(GetAllProps()?.Select(t => t.PropComponent.PropName).ToArray(), propName);
			GameObject go = new GameObject(propName);
			go.transform.parent = transform;
			go.transform.localPosition = Vector3.zero;
			go.transform.localRotation = Quaternion.identity;
			
			PropComponent propComponent = go.AddComponent<PropComponent>();
			propComponent.name = propName;
			propComponent.propType = (PropType) Activator.CreateInstance(basePropType, new object[] {propComponent});
			propComponent.propType.SetPropComponent(propComponent);
			propComponent.propType.AutoConfigProp();

			Undo.RegisterCreatedObjectUndo(go, $"Created {propName}");

			//Instance.allProps.Add(new PropReferences(propComponent));

			EditorSceneManager.MarkSceneDirty(gameObject.scene);
			PropListChanged();

		}
		/// <summary>
		/// Adds a scene element to the Manager
		/// </summary>
		/// <param name="propComponent">The element to be added</param>
		/// <returns>Returns the name of the object which was added</returns>
		public static string AddElementEditor(PropComponent propComponent)
		{
			Instance = FindObjectOfType<PropManager>();

			if (Instance == null)
			{
				return null;
			}
			
			// Checks if the object is in any scene
			if (!propComponent.gameObject.scene.IsValid())
			{
				return null;
			}

			// Moves the prop to the correct scene
			if (propComponent.gameObject.scene != Instance.gameObject.scene)
			{
				SceneManager.MoveGameObjectToScene(propComponent.gameObject, Instance.gameObject.scene);
				
			}
			
			// if it is allready in the list
			if (Instance.allProps.Find(i => i.PropComponent == propComponent) != null)
			{
				return null;
			}
			
			GameObject gameObject = propComponent.gameObject;
			
			
			string[] allNames = GetAllProps()?.Select(t => t.PropComponent.PropName).ToArray(); 
			string objectName = ObjectNames.GetUniqueName(allNames, propComponent.PropName);

			propComponent.PropName = objectName;
			propComponent.gameObject.name = objectName;
			
			Instance.allProps.Add(new PropReferences(propComponent));
			
			EditorSceneManager.MarkSceneDirty(gameObject.scene);
			PropListChanged();
			
			return propComponent.PropName;
		}
		
		public void FindAndAddAllProps()
		{
			PropComponent[] props = FindObjectsOfType<PropComponent>();

			foreach (PropComponent prop in props)
			{
				if (prop.gameObject.scene != gameObject.scene)
				{
					continue;
				}
				
				if (allProps.FirstOrDefault(p => p.PropComponent == prop) == null)
				{
					allProps.Add(new PropReferences(prop));
				}
			}

			Validate();
		}

		public static DropdownField GetPropsDropdown<TIProp>(string elementName, EventCallback<ChangeEvent<string>> callback, string label = "")
			where TIProp : IBaseProp
		{
			if (label == "")
			{
				label = typeof(TIProp).ToString();
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

			// TODO: THIS NEEDS TO BE REDONE
			List<PropReferences> allProps = GetAllProps<TIProp>();

			sceneDropdown.RegisterCallback(callback);
			if (allProps.IsNullOrEmpty())
			{
				return sceneDropdown;
			}
			
			foreach (PropReferences namedProps in allProps)
			{
				sceneDropdown.choices.Add(namedProps.PropComponent.PropName);
			}

			// registers the callback
			return sceneDropdown;
		}
#endif
	}
}