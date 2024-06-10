using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MechanicUtils
{
	public static Dictionary<string, string> Helper(params string[] paramters)
	{
		var dictionary = new Dictionary<string, string>();
		for (int i = 0; i < paramters.Length; i += 2)
		{
			dictionary.Add(paramters[i], paramters[i + 1]);
		}
		return dictionary;
	}
}
