using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SkillsVR;
using UnityEditor;

namespace DialogExporter
{
	public static class APIManager
	{
		public static List<ITextToSpeechAPI> allAPIs = new();
		public static List<ITextToSpeechAPI> AllAPIs => allAPIs;

		private const string currentAPIKey = "currentAPI";

		private static ITextToSpeechAPI currentAPI;

		public static ITextToSpeechAPI CurrentAPI
		{
			get => currentAPI;
			set
			{
				EditorPrefs.SetString(currentAPIKey, CurrentAPI.GetTypeName());
				currentAPI = value;
			}
		}


		[InitializeOnLoadMethod]
		public static void AfterLoad()
		{
			UnityEditor.EditorApplication.update += RefreshListOfAPIs;
		}

		public static void RefreshListOfAPIs()
		{
			UnityEditor.EditorApplication.update -= RefreshListOfAPIs;
			allAPIs = new List<ITextToSpeechAPI>();
            
			List<Type> temp = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(assembly => assembly.GetTypes())
				.Where(type => typeof(ITextToSpeechAPI).IsAssignableFrom(type) && type != typeof(ITextToSpeechAPI)).ToList();

			foreach (ConstructorInfo constructor in temp.Select(type => type.GetConstructor(Type.EmptyTypes)))
			{
				if (constructor == null)
				{
					continue;
				}
                
				allAPIs.Add(constructor.Invoke(new object[] { }) as ITextToSpeechAPI);
			}
            
			currentAPI = allAPIs.Find(t => t.GetTypeName() == EditorPrefs.GetString(currentAPIKey, "")) ?? allAPIs[0];
		}
	}
}