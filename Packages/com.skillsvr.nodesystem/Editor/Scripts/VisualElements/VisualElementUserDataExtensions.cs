using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Scripts.VisualElements
{
	public static class VisualElementUserDataExtensions
    {
		public static bool SetUserData<T>(this VisualElement element, object key, T data)
		{
			if (null == key)
			{
				return false;
			}
			Dictionary<object, object> userData = GetOrCreateUserDataDictionary(element);
			if (userData.ContainsKey(key))
			{
				userData[key] = data;
			}
			else
			{
				userData.Add(key, data);
			}
			return true;
		}

		public static T GetUserData<T>(this VisualElement element, object key)
		{
			Dictionary<object, object> userData = GetOrCreateUserDataDictionary(element);
			if (userData.TryGetValue(key, out object value))
			{
				return (T)value;
			}
			return default;
		}

		public static T GetUserData<T>(this VisualElement element, object key, T defaultValue)
		{
			Dictionary<object, object> userData = GetOrCreateUserDataDictionary(element);
			if (userData.TryGetValue(key, out object value))
			{
				return (T)value;
			}
			return defaultValue;
		}

		public static bool TryGetUserData<T>(this VisualElement element, object key, out T value)
		{
			value = default;
			if (null == key)
			{
				return false;
			}
			Dictionary<object, object> userData = GetOrCreateUserDataDictionary(element);
			if (userData.TryGetValue(key, out object rawValue))
			{
				value = (T)rawValue;
				return true;
			}
			return false;
		}

		private static Dictionary<object, object> GetOrCreateUserDataDictionary(this VisualElement element)
		{
			Dictionary<object, object> userData = element.userData as Dictionary<object, object>;
			if (userData == null)
			{
				userData = new Dictionary<object, object>();
				element.userData = userData;
			}
			return userData;
		}

		public static bool HasUserData<T>(this VisualElement element, object key)
		{
			T value;
			TryGetUserData<T>(element, key, out value);
			return null != value;
		}

		public static bool HasUserData(this VisualElement element, object key, Type dataType)
		{
			if (null == key || null == dataType)
			{
				return false;
			}
			Dictionary<object, object> userData = element.userData as Dictionary<object, object>;
			if (null == userData)
			{
				return false;
			}
			object value;
			userData.TryGetValue(key, out value);
			if (null == value)
			{
				return false;
			}
			return value.GetType() == dataType || dataType.IsAssignableFrom(value.GetType());
		}

		public static Type GetUserDataType(this VisualElement element, object key)
		{
			if (null == key)
			{
				return null;
			}
			Dictionary<object, object> userData = element.userData as Dictionary<object, object>;
			if (userData != null && userData.ContainsKey(key))
			{
				return userData[key].GetType();
			}
			return null;
		}

		public static bool RemoveUserData(this VisualElement element, object key)
		{
			if (null == key)
			{
				return false;
			}
			Dictionary<object, object> userData = element.userData as Dictionary<object, object>;
			if (userData != null && userData.ContainsKey(key))
			{
				userData.Remove(key);
				return true;
			}
			return false;
		}
	}
}