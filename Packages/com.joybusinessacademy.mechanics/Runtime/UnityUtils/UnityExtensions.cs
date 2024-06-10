using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SkillsVR.UnityExtenstion
{
	/// <summary>
	/// Various Extension Methods for core Unity classes
	/// </summary>
	public static class UnityExtensions
	{
		#region Int ........................................................................................................

		/// <summary>
		/// Restricts value to supplied min/max
		/// </summary>
		/// <param name="value"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		public static void Clamp(this int value, int min, int max)
		{
			if (value < min)
				value = min;

			if (value > max)
				value = max;

		}

		#endregion

		#region GameObject / Component .....................................................................................

		/// <summary>
		/// Returns true if the supplied Component is a prefab, not an instance
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static bool IsPrefab(this Component obj)
		{
			return obj.gameObject.scene.rootCount == 0;
		}

		public static void RemoveComponent<T>(this Component parent) where T : Component
		{
			var component = parent.GetComponent<T>();

			if (component == null)
			{
				return;
			}
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(component);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(component, true);
			}
		}

		public static Component GetOrAddComponent(this Component child, Type type, bool findInChildren = false)
		{
			return child.gameObject.GetOrAddComponent(type, findInChildren);
		}
		public static Component GetOrAddComponent(this GameObject child, Type type, bool findInChildren = false)
		{
			Component comp = null;
			if (findInChildren)
				comp = child.GetComponentInChildren(type);
			else
				comp = child.GetComponent(type);
			if (comp == null)
			{
				comp = child.AddComponent(type);
			}
			return comp;
		}

		public static T GetOrAddComponent<T>(this Component child, bool findInChildren = false) where T : Component
		{
			T result;

			if (findInChildren)
			{
				result = child.GetComponentInChildren<T>();
			}
			else
			{
				result = child.GetComponent<T>();
			}

			if (result == null)
			{
				result = child.gameObject.AddComponent<T>();
			}
			return result;
		}

		public static T GetOrAddComponent<T>(this GameObject child, bool findInChildren = false) where T : Component
		{
			T result;

			if (findInChildren)
			{
				result = child.GetComponentInChildren<T>();
			}
			else
			{
				result = child.GetComponent<T>();
			}

			if (result == null)
			{
				result = child.gameObject.AddComponent<T>();
			}
			return result;
		}

		public static GameObject GetChild(this GameObject obj, string name)
		{
			foreach (Transform transform in obj.transform)
			{
				if (transform.name == name)
				{
					return transform.gameObject;
				}
			}

			return null;
		}

		public static GameObject GetChildRecursive(this GameObject obj, string name)
		{
			foreach (Transform transform in obj.transform)
			{
				if (transform.name == name)
				{
					return transform.gameObject;
				}
				else
				{
					var target = GetChildRecursive(transform.gameObject, name);
					if (target != null)
					{
						return target;
					}
				}
			}

			return null;
		}

		public static GameObject GetChild(this Component obj, string name)
		{
			return GetChild(obj.gameObject, name);
		}

		public static GameObject GetOrAddChild(this GameObject parent, string childName)
		{
			var child = parent.transform.Find(childName);

			if (child != null)
			{
				return child.gameObject;
			}

			var newChild = new GameObject(childName);

			newChild.transform.parent = parent.transform;

			return newChild;
		}

		public static GameObject GetOrAddChild(this Component parent, string childName)
		{
			return GetOrAddChild(parent.gameObject, childName);
		}

		public static void SetParent(this Transform target, Transform parent)
		{
			target.SetParent(parent, false);
		}

		public static void SetParent(this Component target, GameObject parent)
		{
			SetParent(target.transform, parent.transform);
		}

		public static void SetParent(this Component target, Transform parent)
		{
			SetParent(target.transform, parent);
		}

		public static void SetParent(this GameObject target, GameObject parent)
		{
			SetParent(target.transform, parent.transform);
		}

		public static void SetParent(this MonoBehaviour target, GameObject parent)
		{
			SetParent(target.transform, parent.transform);
		}

		public static void SetParent(this MonoBehaviour target, MonoBehaviour parent)
		{
			SetParent(target.transform, parent.transform);
		}

		public static void SetParent(this Camera target, MonoBehaviour parent)
		{
			SetParent(target.transform, parent.transform);
		}

		/// <summary>
		/// Update transform (world coordinates)
		/// </summary>
		/// <param name="target"></param>
		/// <param name="position"></param>
		/// <param name="rotation"></param>
		public static void SetTransform(this GameObject target, Vector3 position, Vector3 rotationEuler)
		{
			SetTransform(target, position, Quaternion.Euler(rotationEuler));
		}

		/// <summary>
		/// Update transform (world coordinates)
		/// </summary>
		/// <param name="target"></param>
		/// <param name="position"></param>
		/// <param name="rotation"></param>
		public static void SetTransform(this GameObject target, Vector3 position, Quaternion rotation)
		{
			target.transform.position = position;
			target.transform.rotation = rotation;
		}

		/// <summary>
		/// Reset local transform to Vector3.zero, Quaternion.identity and optionally, scale to 1
		/// </summary>
		/// <param name="target"></param>
		/// <param name="resetScale"></param>
		public static void ResetLocalTransform(this GameObject target, bool resetScale = false)
		{
			target.transform.localPosition = Vector3.zero;
			target.transform.localRotation = Quaternion.identity;

			if (resetScale)
			{
				target.transform.localScale = Vector3.one;
			}
		}

		/// <summary>
		/// Reset local transform to Vector3.zero, Quaternion.identity and optionally, scale to 1
		/// </summary>
		/// <param name="target"></param>
		/// <param name="resetScale"></param>
		public static void ResetLocalTransform(this MonoBehaviour target, bool resetScale = false)
		{
			ResetLocalTransform(target.gameObject, resetScale);
		}

		/// <summary>
		/// Reset local transform to Vector3.zero and optionally, scale to 1
		/// </summary>
		/// <param name="target"></param>
		/// <param name="resetScale"></param>
		public static void ResetLocalTransform(this Component target, bool resetScale = false)
		{
			ResetLocalTransform(target.gameObject, resetScale);
		}

		/// <summary>
		/// Rotate to face the supplied <see cref="Transform"/>, optionally locking the Y Axis
		/// </summary>
		/// <param name="target"></param>
		/// <param name="transformToFace"></param>
		/// <param name="lockYRotation"></param>
		public static void FaceTransform(this Transform target, Transform transformToFace, bool lockYRotation = true)
		{
			var lookAtPos = new Vector3
			{
				x = transformToFace.position.x,
				y = (lockYRotation) ? target.position.y : transformToFace.position.y,
				z = transformToFace.position.z
			};

			target.LookAt(lookAtPos);
		}

		/// <summary>
		/// Rotate to face the supplied <see cref="Transform"/>, optionally locking the Y Axis
		/// </summary>
		/// <param name="target"></param>
		/// <param name="transformToFace"></param>
		/// <param name="lockYRotation"></param>
		public static void FaceTransform(this Component target, Transform transformToFace, bool lockYRotation = true)
		{
			FaceTransform(target.transform, transformToFace, lockYRotation);
		}

		/// <summary>
		/// Rotate to face the supplied <see cref="Transform"/>, optionally locking the Y Axis
		/// </summary>
		/// <param name="target"></param>
		/// <param name="transformToFace"></param>
		/// <param name="lockYRotation"></param>
		public static void FaceTransform(this MonoBehaviour target, Transform transformToFace, bool lockYRotation = true)
		{
			FaceTransform(target.transform, transformToFace, lockYRotation);
		}

		/// <summary>
		/// return the combined <see cref="Bounds"/> for all Renderers encapsulated in the supplied <see cref="GameObject"/>
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public static Bounds GetBounds(this GameObject source)
		{
			var bounds = new Bounds(Vector3.zero, Vector3.zero);

			foreach (var renderer in source.GetComponentsInChildren<Renderer>())
			{
				bounds.Encapsulate(renderer.bounds);
			}

			return bounds;
		}

		public static string GetHierarchyPath(this MonoBehaviour target)
		{
			return target.gameObject.GetHierarchyPath() + "/" + target.GetType().Name;
		}

		public static string GetHierarchyPath(this GameObject target)
		{
			string path = target.name;
			var parent = target.transform.parent;
			while (null != parent && null != parent.gameObject)
			{
				path = parent.gameObject.name + "/" + path;
				parent = parent.transform.parent;
			}
			path = (null == target.scene ? "" : target.scene.name) + "/" + path;
			return path;
		}

		#endregion

		#region Meshes .....................................................................................................

		/// <summary>
		/// Centers a mesh to its Parent Transform
		/// </summary>
		/// <param name="target"></param>
		public static void CenterToParent(this MeshFilter target)
		{
			var zCenter = target.mesh.bounds.center.z;

			target.transform.localPosition = new Vector3(0, 0, 0 - zCenter);

		}

		#endregion

		#region Buttons ....................................................................................................

		/// <summary>
		/// Adds an OnClick event handler that accepts the clicked button as a parameter
		/// </summary>
		/// <param name="button"></param>
		/// <param name="handler"></param>
		public static void AddOnClickHandler(this Button button, UnityAction<Button> handler)
		{
			button.onClick.AddListener(delegate { handler(button); });
		}

		#endregion



		#region Integer ................................................................................................

		public static bool IsInteger(this float value)
		{
			return value == Mathf.Floor(value);
		}

		public static bool IsEven(this int value)
		{
			return value % 2 == 0;
		}

		public static bool IsOdd(this int value)
		{
			return !IsEven(value);
		}

		#endregion

		#region String .................................................................................................

		/// <summary>
		/// Returns a string with the supplied trimChars removed from the beginning of the string, if found
		/// </summary>
		/// <param name="target"></param>
		/// <param name="trimChars"></param>
		/// <returns></returns>
		public static string TrimStart(this string target, string trimChars)
		{
			return target.TrimStart(trimChars.ToCharArray());
		}

		/// <summary>
		/// Returns a string with the supplied trimChars removed from the end of the string, if found
		/// </summary>
		/// <param name="target"></param>
		/// <param name="trimChars"></param>
		/// <returns></returns>
		public static string TrimEnd(this string target, string trimChars)
		{
			return target.Substring(0, target.Length - trimChars.Length);
		}

		#endregion

		#region Lists .....................................................................................................

		/// <summary>
		/// Returns the collection of ToString() of multiple objects
		/// with a line-break separating them.
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		public static string ToStringOfObjectsWithLinebreaks<T>(this ICollection<T> objects)
		{
			string objectsString = "";

			foreach (var obj in objects)
				objectsString += obj.ToString() + "\n";

			return objectsString.TrimEnd();
		}

		/// <summary>
		/// Returns the collection of ToString() of multiple objects
		/// with a line-break separating them.
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		public static string ToStringOfObjectsWithSeparator<T>(this ICollection<T> objects, string seperator)
		{
			string objectsString = "";
			if (objects == null)
				return objectsString;
			if (objects.Count == 0)
				return objectsString;
			foreach (var obj in objects)
			{
				objectsString += (obj != null ? obj.ToString() : "null") + seperator;
			}


			objectsString = string.IsNullOrEmpty(seperator) ? objectsString : objectsString.TrimEnd(seperator);
			return objectsString;
		}

		/// <summary>
		/// Returns the names of multiple objects
		/// with a line-break separating them.
		/// </summary>
		/// <param name="objects"></param>
		/// <returns></returns>
		public static string ToStringOfObjectNamesWithLinebreaks<T>(this ICollection<T> objects) where T : UnityEngine.Object
		{
			string objectsString = "";

			foreach (var obj in objects)
				objectsString += obj.name + "\n";

			return objectsString.TrimEnd();
		}

		/// <summary>
		/// Adds an item to the supplied list only if its not already contained
		/// </summary>
		/// <param name="target"></param>
		/// <param name="item"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns>True if added, false if already exists</returns>
		public static bool AddIfNotFound<T>(this List<T> target, T item)
		{
			if (target.Contains(item))
				return false;

			target.Add(item);

			return true;
		}

		public static void RemoveIfFound<T>(this List<T> target, T item)
		{
			if (target.Contains(item))
				target.Remove(item);
		}

		#endregion
	}
}