
using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Collections;
using UnityEngine.UI;
using System.Globalization;

namespace SkillsVR.UnityExtenstion
{
	public static class ObjectExtensions
	{
		public static float Signed(this float t)
		{
			if (t < 0)
				return -1f;

			return 1f;
		}

		public static int NthIndexOf(this string target, string value, int n)
		{
			Match m = Regex.Match(target, "((" + Regex.Escape(value) + ").*?){" + n + "}");

			if (m.Success)
				return m.Groups[2].Captures[n - 1].Index;
			else
				return -1;
		}

		public static string ToRomanNumeral(this int number)
		{

			var retVal = new StringBuilder(5);
			var valueMap = new SortedDictionary<int, string>
		{
			{ 1, "I" },
			{ 4, "IV" },
			{ 5, "V" },
			{ 9, "IX" },
			{ 10, "X" },
			{ 40, "XL" },
			{ 50, "L" },
			{ 90, "XC" },
			{ 100, "C" },
			{ 400, "CD" },
			{ 500, "D" },
			{ 900, "CM" },
			{ 1000, "M" },
		};

			foreach (var kvp in valueMap.Reverse())
			{
				while (number >= kvp.Key)
				{
					number -= kvp.Key;
					retVal.Append(kvp.Value);
				}
			}

			return retVal.ToString();
		}

		public static Color WithAlpha(this Color color, float alpha)
		{

			return new Color(color.r, color.g, color.b, alpha);

		}

		public static T Get<T>(this List<T> list, int index)
		{
			if (index >= 0 && index < list.Count)
				return list[index];

			return default(T);
		}

		public static T[] Add<T>(this T[] array, T item)
		{
			var list = array.ToList();
			list.Add(item);

			return list.ToArray();
		}

		public static T[] Insert<T>(this T[] array, int index, T item)
		{
			var list = array.ToList();
			list.Insert(index, item);

			return list.ToArray();
		}

		public static bool LayerMaskContains(this int layerMask, string layerName)
		{
			var layer = LayerMask.NameToLayer(layerName);

			return layerMask.LayerMaskContains(layer);
		}

		public static bool LayerMaskContains(this int layerMask, int layer)
		{
			return layerMask == (layerMask | (1 << layer));
		}

		public static bool LayerMaskContains(this LayerMask layerMask, int layer)
		{
			return layerMask.value.LayerMaskContains(layer);
		}

		public static bool LayerMaskContains(this LayerMask layerMask, GameObject go)
		{
			var layer = go.layer;

			return layerMask.value.LayerMaskContains(layer);
		}

		public static bool LayerMaskContains(this LayerMask layerMask, Component c)
		{
			var layer = c.gameObject.layer;

			return layerMask.value.LayerMaskContains(layer);
		}

		/// <summary>
		/// Provides the same functionality as the ToShortDateString() method, but
		/// with leading zeros.
		/// <example>
		/// ToShortDateString: 5/4/2011 |
		/// ToShortDateStringZero: 05/04/2011
		/// </example>
		/// </summary>
		/// <param name="source">Source date.</param>
		/// <returns>Culture safe short date string with leading zeros.</returns>
		public static string ToShortDateStringZero(this DateTime source)
		{
			return ToShortStringZero(source,
				CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern,
				CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator);
		}

		/// <summary>
		/// Provides the same functionality as the ToShortTimeString() method, but
		/// with leading zeros.
		/// <example>
		/// ToShortTimeString: 2:06 PM |
		/// ToShortTimeStringZero: 02:06 PM
		/// </example>
		/// </summary>
		/// <param name="source">Source date.</param>
		/// <returns>Culture safe short time string with leading zeros.</returns>
		public static string ToShortTimeStringZero(this DateTime source)
		{
			return ToShortStringZero(source,
				CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern,
				CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator);
		}

		private static string ToShortStringZero(this DateTime source,
			string pattern,
			string seperator)
		{
			var format = pattern.Split(new[] { seperator }, StringSplitOptions.None);

			var newPattern = string.Empty;

			for (var i = 0; i < format.Length; i++)
			{
				newPattern = newPattern + format[i];
				if (format[i].Length == 1)
					newPattern += format[i];
				if (i != format.Length - 1)
					newPattern += seperator;
			}

			return source.ToString(newPattern, CultureInfo.InvariantCulture);
		}

		public static T Next<T>(this T src) where T : struct
		{
			if (!typeof(T).IsEnum)
				throw new ArgumentException(String.Format("Argumnent {0} is not an Enum", typeof(T).FullName));

			T[] Arr = (T[])Enum.GetValues(src.GetType());
			int j = Array.IndexOf<T>(Arr, src) + 1;
			return (Arr.Length == j) ? Arr[0] : Arr[j];
		}

		public static Dictionary<string, string> JsonToDictionary(this string text)
		{
			var dict = new Dictionary<string, string>();

			// Remove open and closing brackets.
			if (text.StartsWith("{"))
				text = text.TrimStart('{');
			if (text.EndsWith("}"))
				text = text.TrimEnd('}');

			// Split by comma
			var lines = text.Split(',');

			// For each line
			foreach (var line in lines)
			{
				// Get the key within the first set of quotes.
				int quote1 = line.IndexOf("\"") + 1;
				int quote2 = line.IndexOf("\"", quote1 + 1);
				int length = quote2 - quote1;

				var key = line.Substring(quote1, length);

				// Get the value from beyond the first colon.
				var value = line.Substring(line.IndexOf(':') + 1);

				dict.Add(key, value);
			}

			return dict;
		}

		public static string EncodeToJPGString(this Texture2D tex)
		{
			return System.Text.Encoding.Default.GetString(tex.EncodeToJPG());
		}

		public static string EncodeToPNGString(this Texture2D tex)
		{
			return System.Text.Encoding.Default.GetString(tex.EncodeToPNG());
		}

		public static void AddFieldsFromDict(this WWWForm form, Dictionary<string, string> dict)
		{
			foreach (var item in dict)
				form.AddField(item.Key, item.Value);
		}

		public static Dictionary<K, V> ToDictionary<K, V>(this Hashtable table)
		{
			return table
				.Cast<DictionaryEntry>()
				.ToDictionary(kvp => (K)kvp.Key, kvp => (V)kvp.Value);
		}

		public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek = DayOfWeek.Monday)
		{
			int diff = dt.DayOfWeek - startOfWeek;
			if (diff < 0)
			{
				diff += 7;
			}

			return dt.AddDays(-1 * diff).Date;
		}

		public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
		{
			return go.AddComponent<T>().GetCopyOf(toAdd) as T;
		}

		public static T GetCopyOf<T>(this Component comp, T other) where T : Component
		{
			Type type = comp.GetType();

			if (type != other.GetType())
				return null; // type mis-match

			BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default;

			PropertyInfo[] pinfos = type.GetProperties(flags);
			foreach (var pinfo in pinfos)
			{
				if (!pinfo.CanWrite)
					continue;

				// Check if field is obsolete.
				{
					bool obsolete = false;

					IEnumerable attrData = pinfo.CustomAttributes;
					foreach (CustomAttributeData data in attrData)
					{
						if (data.AttributeType == typeof(System.ObsoleteAttribute))
						{
							obsolete = true;
							break;
						}
					}

					if (obsolete)
						continue;
				}

				try
				{
					pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
				}
				catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.

			}

			FieldInfo[] finfos = type.GetFields(flags);

			foreach (var finfo in finfos)
				finfo.SetValue(comp, finfo.GetValue(other));

			return comp as T;
		}

		#region TRANSFORM

		public static T GetOrAddComponent<T>(this GameObject go) where T : Component
		{
			var comp = go.GetComponent<T>();

			if (!comp)
				comp = go.AddComponent<T>();

			return comp;
		}

		/// <summary>
		/// Resets the specified transform to the default pos / rot / scale.
		/// </summary>
		/// <param name="transform">Transform.</param>
		public static void Reset(this Transform transform)
		{
			transform.localPosition = Vector3.zero;
			transform.localEulerAngles = Vector3.zero;
			transform.localScale = Vector3.one;
		}

		public static Transform AddChild(this GameObject gameObject, GameObject child, bool reset = true)
		{
			if (child == null)
				throw new UnityException();

			return gameObject.transform.AddChild(child.transform, reset);
		}

		public static Transform AddChild(this Transform transform, GameObject child, bool reset = true)
		{
			if (child == null)
				throw new UnityException();

			return transform.AddChild(child.transform, reset);
		}

		public static Transform AddChild(this Transform transform, Transform child, bool reset = true)
		{
			if (child == null)
				throw new UnityException();

			child.transform.SetParent(transform, !reset);

			if (reset)
				child.transform.Reset();

			return child;
		}

		public static Transform Search(this Transform target, string name)
		{
			if (target.name == name)
				return target;

			for (int i = 0; i < target.childCount; ++i)
			{
				var result = Search(target.GetChild(i), name);

				if (result != null)
					return result;
			}

			return null;
		}

		/// <summary>
		/// Finds the Component among the Transform's parents.
		/// </summary>
		/// <returns>The in parents.</returns>
		/// <param name="trans">Trans.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T FindInParents<T>(this Transform trans) where T : Component
		{
			if (trans == null)
				return null;

			T comp = trans.GetComponent<T>();

			if (comp == null)
			{
				Transform t = trans.transform.parent;

				while (t != null && comp == null)
				{
					comp = t.gameObject.GetComponent<T>();
					t = t.parent;
				}
			}

			return comp;
		}

		/// <summary>
		/// Gets all children gameObjects.
		/// </summary>
		/// <returns>The children.</returns>
		/// <param name="component">Component.</param>
		public static List<Transform> GetChildrenTransforms(this Component component, bool includeInactive = true)
		{
			//		return component.GetComponentsInChildren<Transform>()
			//			.Where( x => x.transform.parent == component.transform ).Select ( y => y.gameObject )
			//				.ToList();
			List<Transform> list = new List<Transform>();

			foreach (Transform t in component.transform)
			{
				if (t == component.transform)
					continue;
				if (!includeInactive && !t.gameObject.activeSelf)
					continue;

				list.Add(t);
			}

			return list;
		}

		/// <summary>
		/// Gets all children gameObjects.
		/// </summary>
		/// <returns>The children.</returns>
		/// <param name="component">Component.</param>
		public static List<GameObject> GetChildren(this Component component, bool includeInactive = true)
		{
			//		return component.GetComponentsInChildren<Transform>()
			//			.Where( x => x.transform.parent == component.transform ).Select ( y => y.gameObject )
			//				.ToList();
			List<GameObject> list = new List<GameObject>();

			foreach (Transform t in component.transform)
			{
				if (t == component.transform)
					continue;
				if (!includeInactive && !t.gameObject.activeSelf)
					continue;

				list.Add(t.gameObject);
			}

			return list;
		}

		public static List<T> GetComponentsInChildrenDepth<T>
		(this Component c, int depth = 1) where T : Component
		{
			return c.gameObject.GetComponentsInChildrenDepth<T>(depth);
		}

		public static List<T> GetComponentsInChildrenDepth<T>
		(this Transform t, int depth = 1) where T : Component
		{
			return t.gameObject.GetComponentsInChildrenDepth<T>(depth);
		}

		public static List<T> GetComponentsInChildrenDepth<T>
		(this GameObject go, int depth = 1) where T : Component
		{
			List<T> components = new List<T>();

			if (depth <= 0)
				return components;

			Transform root = go.transform;

			foreach (Transform t in root)
			{
				components.AddRange(t.GetComponents<T>());
				components.AddRange(t.GetComponentsInChildrenDepth<T>(depth - 1));
			}

			return components;
		}

		public static void SetLayerRecursive(this GameObject obj, int layer)
		{
			obj.GetChildrenRecursive().ForEach(child => child.layer = layer);
		}

		public static List<GameObject> GetChildrenRecursive(this GameObject obj, bool includeInactive = true)
		{
			var list = obj.GetComponentsInChildren<Transform>(includeInactive);

			return list.Select(x => x.gameObject).ToList();
		}

		public static List<GameObject> GetChildrenRecursive(this Component component, bool includeInactive = true)
		{
			var list = component.GetComponentsInChildren<Transform>(includeInactive);

			return list.Select(x => x.gameObject).ToList();
		}

		public static void DestroyChildren(this Transform transform)
		{
			if (Application.isEditor && !Application.isPlaying)
				transform.DestroyChildrenImmediate();
			else
				foreach (Transform child in transform)
					GameObject.Destroy(child.gameObject);
		}

		public static void DestroyChildrenImmediate(this Transform transform, Transform exclusion = null)
		{
			List<GameObject> gameObjects = new List<GameObject>();
			foreach (Transform child in transform)
			{
				if (child == exclusion)
					continue;

				gameObjects.Add(child.gameObject);
			}

			// descending order, cant delete immediate using foreach as it changes transforms everytime child get destroyed
			for (int j = gameObjects.Count - 1; j >= 0; j--)
				GameObject.DestroyImmediate(gameObjects[j]);
		}

		public static void ToggleChildren(this Transform transform, bool state = false, Transform exception = null)
		{
			foreach (Transform child in transform)
			{
				if (child != exception)
					child.gameObject.SetActive(state);
			}
		}

		/// <summary>
		/// Finds the component among a GameObject's parents.
		/// </summary>
		/// <returns>The Component.</returns>
		/// <param name="go">Go.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T FindInParents<T>(this GameObject go) where T : Component
		{
			return go.transform.FindInParents<T>();
		}



		#endregion

		public static string ToHex(this Color color)
		{
			Color32 c = color;
			string hex = c.r.ToString("X2") + c.g.ToString("X2") + c.b.ToString("X2");

			return hex;
		}

		public static Color ToColor(this string hex)
		{
			byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
			byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
			byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

			return new Color32(r, g, b, 255);
		}

		public static float NegPos(this float num)
		{
			var r = UnityEngine.Random.value;
			var d = r < 0.5f ? -1f : 1f;

			return d * num;
		}

		#region STRING

		public static string WrapColor(this string text, string replace, Color col)
		{
			return text.Replace(replace, replace.WrapColor(col));
		}

		public static string WrapColor(this string text, Color col)
		{
			return string.Format("<color=#{0}>{1}</color>", col.ToHex(), text);
		}

		public static string WrapSize(this string text, float size)
		{
			return string.Format("<size={0}>{1}</size>", size, text);
		}

		public static string WrapItalic(this string text)
		{
			return string.Format("<i>{0}</i>", text);
		}

		public static string WrapBold(this string text)
		{
			return string.Format("<b>{0}</b>", text);
		}

		public static string Unrichen(this string text)
		{
			var input = text;
			var output = Regex.Replace(input, "<.*?>", string.Empty);

			return output;
		}

		public static string SplitCamelCase(this string str)
		{
			return Regex.Replace(Regex.Replace(str, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2"), @"(\p{Ll})(\P{Ll})", "$1 $2");
		}

		public static string ToStringList(this List<string> list)
		{
			string s = "";

			for (int i = 0; i < list.Count; i++)
			{
				var item = list[i];

				s += item;

				if (i != list.Count - 1)
					s += "\n";
			}

			return s;
		}

		#endregion

		#region USER-INTERFACE

		public static void ToggleChildren(this ToggleGroup tg, Transform exception)
		{
			tg.transform.ToggleChildren(false);
			exception.gameObject.SetActive(true);
		}

		public static Toggle GetActive(this ToggleGroup aGroup)
		{
			return aGroup.ActiveToggles().FirstOrDefault();
		}

		#endregion

		public static Rect GetRect(this Texture2D tex)
		{
			return new Rect(0, 0, tex.width, tex.height);
		}

		public static float GetAverage(this Vector3 v)
		{
			return (v.x + v.y + v.z) / 3.0f;
		}

		#region LIST

		public static int IndexOfMin(this IEnumerable<float> source)
		{
			if (source == null)
				throw new System.ArgumentNullException("source");

			float minValue = float.MaxValue;
			int minIndex = -1;
			int index = -1;

			foreach (float num in source)
			{
				index++;

				if (num <= minValue)
				{
					minValue = num;
					minIndex = index;
				}
			}

			if (index == -1)
				throw new System.InvalidOperationException("Sequence was empty");

			return minIndex;
		}

		/// <summary>
		/// Shifts an element in a list.
		/// </summary>
		/// <param name="list">List.</param>
		/// <param name="oldIndex">Old index.</param>
		/// <param name="newIndex">New index.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static void Shift<T>(this List<T> list, int oldIndex = 0, int newIndex = -1)
		{
			if (newIndex < 0)
				newIndex = list.Count - 1;

			var item = list[oldIndex];

			list.RemoveAt(oldIndex);

			list.Insert(newIndex, item);
		}

		public static void ShiftToTop<T>(this List<T> list, T item)
		{
			if (!list.Contains(item))
				return;

			int oldIndex = list.IndexOf(item);

			if (oldIndex < 1)
				return;

			int newIndex = 0;

			list.Shift(oldIndex, newIndex);
		}

		public static void ShiftToBottom<T>(this List<T> list, T item)
		{
			if (!list.Contains(item))
				return;

			int oldIndex = list.IndexOf(item);

			if (oldIndex > list.Count - 2)
				return;

			int newIndex = list.Count - 1;

			list.Shift(oldIndex, newIndex);
		}

		public static void ShiftUp<T>(this List<T> list, T item)
		{
			if (!list.Contains(item))
				return;

			int oldIndex = list.IndexOf(item);

			if (oldIndex < 1)
				return;

			int newIndex = oldIndex - 1;

			list.Shift(oldIndex, newIndex);
		}

		public static void ShiftDown<T>(this List<T> list, T item)
		{
			if (!list.Contains(item))
				return;

			int oldIndex = list.IndexOf(item);

			if (oldIndex > list.Count - 2)
				return;

			int newIndex = oldIndex + 1;

			list.Shift(oldIndex, newIndex);
		}

		/// <summary>
		/// Randomly shuffles the objects in a list.
		/// </summary>
		/// <param name="list">List.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static List<T> Shuffle<T>(this List<T> list)
		{
			var finalList = list;

			int n = finalList.Count;
			while (n > 1)
			{
				n--;
				int k = UnityEngine.Random.Range(0, n + 1);
				T value = finalList[k];
				finalList[k] = finalList[n];
				finalList[n] = value;
			}

			return finalList;
		}

		public static List<T> GetShuffle<T>(this List<T> list)
		{
			var finalList = new List<T>();

			foreach (var item in list)
				finalList.Add(item);

			int n = finalList.Count;
			while (n > 1)
			{
				n--;
				int k = UnityEngine.Random.Range(0, n + 1);
				T value = finalList[k];
				finalList[k] = finalList[n];
				finalList[n] = value;
			}

			return finalList;
		}

		/// <summary>
		/// Gets a random element in a list.
		/// </summary>
		/// <returns>A random element.</returns>
		/// <param name="list">List.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T GetRandom<T>(this List<T> list)
		{
			return list[UnityEngine.Random.Range(0, list.Count)];
		}

		public static T PopRandom<T>(this List<T> list)
		{
			var item = list.GetRandom();
			list.Remove(item);
			return item;
		}

		/// <summary>
		/// Gets a random element in an array.
		/// </summary>
		/// <returns>A random element.</returns>
		/// <param name="list">List.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T GetRandom<T>(this T[] array)
		{
			return array[UnityEngine.Random.Range(0, array.Length)];
		}

		public static T GetRandomRiggedByIndex<T>(this T[] array, bool reverse = false)
		{
			int count = array.Length;
			int divs = (int)Mathf.Pow(2, count) - 1;// 2 ^ count;
			int rand = UnityEngine.Random.Range(0, divs);
			int next = Mathf.NextPowerOfTwo(rand);
			int index = (int)Mathf.Sqrt(next);

			if (reverse)
				index = count - index;

			return array[index];
		}


		#endregion

		#region Reflective

		public static T GetReference<T>(object inObj, string fieldName) where T : class
		{
			return GetField(inObj, fieldName) as T;
		}

		public static T GetValue<T>(object inObj, string fieldName) where T : struct
		{
			return (T)GetField(inObj, fieldName);
		}

		public static void SetField(object inObj, string fieldName, object newValue)
		{
			FieldInfo info = inObj.GetType().GetField(fieldName);
			if (info != null)
				info.SetValue(inObj, newValue);
		}

		private static object GetField(object inObj, string fieldName)
		{
			object ret = null;
			FieldInfo info = inObj.GetType().GetField(fieldName);
			if (info != null)
				ret = info.GetValue(inObj);
			return ret;
		}

		#endregion

		public static string PadLeft(this int num, int totalWidth = 3, char paddingChar = '0')
		{
			return num.ToString().PadLeft(totalWidth, paddingChar);
		}

		public static IEnumerator Play(this Animation animation, string clipName, bool useTimeScale, System.Action onComplete)
		{
			//We Don't want to use timeScale, so we have to animate by frame..
			if (!useTimeScale)
			{

				AnimationState _currState = animation[clipName];
				bool isPlaying = true;
				float _progressTime = 0F;
				float _timeAtLastFrame = 0F;
				float _timeAtCurrentFrame = 0F;
				float deltaTime = 0F;


				animation.Play(clipName);

				_timeAtLastFrame = Time.realtimeSinceStartup;
				while (isPlaying)
				{
					_timeAtCurrentFrame = Time.realtimeSinceStartup;
					deltaTime = _timeAtCurrentFrame - _timeAtLastFrame;
					_timeAtLastFrame = _timeAtCurrentFrame;

					_progressTime += deltaTime;
					_currState.normalizedTime = _progressTime / _currState.length;
					animation.Sample();

					//Debug.Log(_progressTime);

					if (_progressTime >= _currState.length)
					{
						//Debug.Log(&quot;Bam! Done animating&quot;);
						if (_currState.wrapMode != WrapMode.Loop)
						{
							//Debug.Log(&quot;Animation is not a loop anim, kill it.&quot;);
							//_currState.enabled = false;
							isPlaying = false;
						}
						else
						{
							//Debug.Log(&quot;Loop anim, continue.&quot;);
							_progressTime = 0.0f;
						}
					}

					yield return new WaitForEndOfFrame();
				}
				yield return null;
				if (onComplete != null)
				{

					onComplete();
				}
			}
			else
			{
				animation.Play(clipName);
			}
		}
	}
}
