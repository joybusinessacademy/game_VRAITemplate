using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
	public class NodeViewDataValidationService : INodeViewDataValidationService
	{

	}


	public static class ValidationManager
	{
		static Dictionary<Type, Type> managedCache = new Dictionary<Type, Type>();

		public static IDataValidator GetDataValidationByType(Type type)
		{
			var validationClassType = GetDataValidationClassTypeByTargetType(type);
			if (null == validationClassType)
			{
				return new InvalidNodeViewDataValidation();
			}

			try
			{
				return Activator.CreateInstance(validationClassType) as IDataValidator;
			}
			catch
			{
				return new InvalidNodeViewDataValidation();
			}
	}

		private static Type GetDataValidationClassTypeByTargetType(Type targetType)
		{
			if (null == targetType)
			{
				return null;
			}
			ScanAll();
			Type item = null;
			managedCache.TryGetValue(targetType, out item);
			return item;
		}

		private static bool allScaned;
		private static void ScanAll()
		{
			if (allScaned)
			{
				return;
			}
			allScaned = true;
			var targetInterfaceType = typeof(IDataValidator);
			var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
				.Where(t => !t.IsAbstract 
				&& !t.IsInterface 
				&& t.IsClass 
				&& targetInterfaceType.IsAssignableFrom(t)
				&& t.GetCustomAttribute<CustomDataValidationAttribute>() != null);
			foreach(var type in types)
			{
				var bindings = type.GetCustomAttributes<CustomDataValidationAttribute>();
				foreach(var binding in bindings)
				{
					if (managedCache.ContainsKey(binding.targetType))
					{
						continue;
					}
					managedCache.Add(binding.targetType, type);
				}
			}
		}
	}
}