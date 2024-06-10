using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class DataBundle
{
	protected Dictionary<string, object> data = new Dictionary<string, object>();

	public ICollection<string> Keys => data.Keys;

	public ICollection<object> Values => data.Values;

	public int Count => data.Count;

	public DataBundle Add(string key, object value)
	{
		if (string.IsNullOrWhiteSpace(key) || null == value)
		{
			return this;
		}
		if (data.ContainsKey(key))
		{
			data[key] = value;
		}
		else
		{
			data.Add(key, value);
		}
		return this;
	}

	public DataBundle Add(KeyValuePair<string, object> item)
	{
		Add(item.Key, item.Value);
		return this;
	}

	public void Clear()
	{
		data.Clear();
	}

	public bool Contains(KeyValuePair<string, object> item)
	{
		return data.Contains(item);
	}

	public bool ContainsKey(string key)
	{
		if (string.IsNullOrWhiteSpace(key))
		{
			return false;
		}
		return data.ContainsKey(key);
	}

	public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
	{
		return data.GetEnumerator();
	}

	public bool Remove(string key)
	{
		if (string.IsNullOrWhiteSpace(key))
		{
			return false;
		}
		return data.Remove(key);
	}

	public bool Remove(KeyValuePair<string, object> item)
	{
		if (string.IsNullOrWhiteSpace(item.Key))
		{
			return false;
		}
		return data.Remove(item.Key);
	}

	public bool TryGetValue(string key, out object value)
	{
		value = null;
		if (string.IsNullOrWhiteSpace(key))
		{
			return false;
		}
		return data.TryGetValue(key, out value);
	}

	public bool TryGetValue<T>(string key, out T value)
	{
		value = default(T);
		object valueObject = null;
		bool success = TryGetValue(key, out valueObject);

		if (success && null != valueObject && (valueObject.GetType() == typeof(T) || valueObject.GetType().IsSubclassOf(typeof(T))))
		{
			value = (T)valueObject;
		}
		else
		{
			success = false;
		}

		return success;
	}

	public T Get<T>(string key, T defaultValue = default(T))
	{
		T value = defaultValue;
		TryGetValue(key, out value);
		return value;
	}

	public bool GetBool(string key, bool defaultValue = false)
	{
		return Get<bool>(key, defaultValue);
	}

	public int GetInt(string key, int defaultValue = 0)
	{
		return Get<int>(key, defaultValue);
	}

	public string GetString(string key, string defaultValue = "")
	{
		return Get<string>(key, defaultValue);
	}

	public float GetFloat(string key, float defaultValue = 0.0f)
	{
		return Get<float>(key, defaultValue);
	}
}