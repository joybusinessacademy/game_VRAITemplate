using System;
using System.Collections.Generic;
using System.Linq;


public static class ListHelper
{
	public static bool IsNullOrEmpty<T>(this IEnumerable<T> list)
	{
		return list == null || !list.Any();
	}
	
	public static bool IsNullOrWhitespace(this string text)
	{
		return text == null || text.Trim().Length == 0;
	}
	
	public static IEnumerable<T> ForEach<T>(this IEnumerable<T> list, Action<T> action)
	{
		foreach (T obj in list)
		{
			action(obj);
		}
		return list;
	}
}
