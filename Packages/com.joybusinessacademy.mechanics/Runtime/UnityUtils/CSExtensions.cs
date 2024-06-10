using SkillsVR.UnityExtenstion;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


	public static class CSExtensions
	{

		#region Integer ................................................................................................

		public static bool IsInteger(this float value)
		{
			return value == Mathf.Floor(value);
		}

		public static bool IsEven(this int value)
		{
			return MathUtil.IsEven(value);
		}

		public static bool IsOdd(this int value)
		{
			return MathUtil.IsOdd(value);
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
		public static string ToStringOfObjectNamesWithLinebreaks<T>(this ICollection<T> objects) where T : Object
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

