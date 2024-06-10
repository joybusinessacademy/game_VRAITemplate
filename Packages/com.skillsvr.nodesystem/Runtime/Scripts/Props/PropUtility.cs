using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Props
{
	public static class PropUtility
	{
		public static string FormatName(string name)
		{
			name = Path.GetFileNameWithoutExtension(name);
			name = name.Replace("_", " ");
			name = name.Replace("-", " ");

#if UNITY_EDITOR
			name = ObjectNames.NicifyVariableName(name);
#endif

			return name;
		}
	}
}