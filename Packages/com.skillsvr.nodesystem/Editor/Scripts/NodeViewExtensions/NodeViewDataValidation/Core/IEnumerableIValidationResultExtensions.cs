using SkillsVRNodes.Editor.NodeViews.Validation.Impl;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SkillsVRNodes.Editor.NodeViews.Validation
{
	public static class IEnumerableIValidationResultExtensions
	{
		public static WarningLevelEnum GetMaxWarningLevel(this IEnumerable<IValidationResult> source)
		{
			return source.Max(x => x.WarningLevel);
		}

		public static IValidationResult Merge(this IEnumerable<IValidationResult> source, string msgFormat = "{lv}: {msg}")
		{
			if (null == source)
			{
				return null;
			}

			var validSource = source.Where(x => null != x);
			if (0 == validSource.Count())
			{
				return null;
			}

			ValidationResult result = new ValidationResult();
			result.WarningLevel = validSource.GetMaxWarningLevel();
			result.Name = validSource.First().Name;
			result.Id = string.Join("|", validSource.Select(x => string.IsNullOrWhiteSpace(x.Id) ? "null" : x.Id));

			msgFormat = string.IsNullOrWhiteSpace(msgFormat) ? "{lv}: {msg}" : msgFormat;
			result.Message = string.Join("\r\n\r\n", validSource.Select(x => x.ToFormatString(msgFormat)).Distinct());
			return result;
		}


		public static IEnumerable<IValidationResult> GroupResults<TKEY>(this IEnumerable<IValidationResult> source, Func<IValidationResult, TKEY> keySelector)
		{
			if (null == source)
			{
				return new List<IValidationResult>(0);
			}

			if (null == keySelector)
			{
				return source;
			}

			var groups = source.GroupBy(keySelector);
			List<IValidationResult> outputs = new List<IValidationResult>();
			foreach(var group in groups)
			{
				var item = group.Merge();
				if (null != item)
				{
					outputs.Add(item);
				}
			}
			return outputs;
		}
	}
}