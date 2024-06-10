using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VRMechanics.Mechanics.GestureDetection
{
	internal static class DetectStepManager
	{
		private static Dictionary<string, IGestureDetectStep> managedDetectors = new Dictionary<string, IGestureDetectStep>();

		public static IGestureDetectStep GetDetector(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return null;
			}
			IGestureDetectStep detector = null;
			managedDetectors.TryGetValue(name, out detector);
			return detector;
		}


		public static bool Detect(GestureDetectStepData data, IGestureBody one, IGestureBody other)
		{
			if (null == data || string.IsNullOrWhiteSpace(data.detectorName))
			{
				return true;
			}

			IGestureDetectStep detector = null;
			if (!managedDetectors.TryGetValue(data.detectorName, out detector))
			{
				return true;
			}
			return detector.Detect(data, one, other);
		}


		[RuntimeInitializeOnLoadMethod]
#if UNITY_EDITOR
		[InitializeOnLoadMethod]
#endif
		private static void InitDetectorTypes()
		{
			managedDetectors.Clear();
			var detectorTypes = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(x => x.GetTypes())
				.Where(t => !t.IsAbstract && !t.IsInterface && t.GetInterfaces().Contains(typeof(IGestureDetectStep)));
			foreach (Type detectorType in detectorTypes)
			{
				try
				{
					IGestureDetectStep instance = Activator.CreateInstance(detectorType) as IGestureDetectStep;
					managedDetectors.Add(detectorType.Name, instance);
				}
				catch(Exception e)
				{
					Debug.LogError("Cannot init ICharacterNodeDetector from type " + detectorType.FullName + ": " + e.Message);
				}
			}
			//Debug.Log(string.Join(" ", managedDetectors.Count, "detectors found"));
		}
	}
}
