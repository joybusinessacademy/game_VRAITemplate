using GraphProcessor;
using System;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation
{
	public interface IValidationResult
    {
        string Name { get; }
		WarningLevelEnum WarningLevel { get; }
        string Message { get; }
        string Id { get; }
	}

    public static class INodeViewValidationResultExtension
    {
        public static string Print(this IValidationResult result)
        {
            return string.Join(" ", result.WarningLevel, result.Name, result.Message, result.Id);
        }

		/// <summary>
		/// Print result with custom pattern. Use pattern with any of options: {id}, {name}, {lv} or {msg}.
		/// </summary>
		/// <param name="result">The result to print.</param>
		/// <param name="pattern">The format pattern with any of options: {id}, {name}, {lv} or {msg}.</param>
		/// <returns></returns>
		public static string ToFormatString(this IValidationResult result, string pattern)
		{
			if (null == result || string.IsNullOrEmpty(pattern))
			{
				return "";
			}
			return pattern.Replace("{id}", result.Id).Replace("{name}", result.Name).Replace("{msg}", result.Message).Replace("{lv}", result.WarningLevel.ToString());
		}
	}
}