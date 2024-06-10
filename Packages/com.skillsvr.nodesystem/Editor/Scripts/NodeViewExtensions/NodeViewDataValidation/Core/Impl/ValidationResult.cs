using GraphProcessor;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
	public class ValidationResult : IValidationResult
	{
		public string Name { get; set; }

		public WarningLevelEnum WarningLevel { get; set; }

		public string Message { get; set; }

		public string Id { get; set; }
	}

	public static class NodeViewValidationResultExtension
	{
		public static IValidationResult Add(this IList<IValidationResult> list, string property,  WarningLevelEnum warningLevel, string message, string nodeGuid)
		{
			ValidationResult result = new ValidationResult();
			result.WarningLevel = warningLevel;
			result.Name = property;
			result.Message = message;
			result.Id = nodeGuid;
			list.Add(result);
			return result;
		}

		public static IValidationResult AddWarning(this IList<IValidationResult> list, string property, string message, string baseNode)
		{
			return Add(list, property, WarningLevelEnum.Warning, message, baseNode);
		}

		public static IValidationResult AddError(this IList<IValidationResult> list, string property, string message, string baseNode)
		{
			return Add(list, property, WarningLevelEnum.Error, message, baseNode);
		}

		public static IValidationResult AddNormal(this IList<IValidationResult> list, string property, string message, string baseNode)
		{
			return Add(list, property, WarningLevelEnum.Normal, message, baseNode);
		}
	}
}