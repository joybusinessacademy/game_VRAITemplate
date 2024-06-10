using GraphProcessor;
using Scripts.VisualElements;
using SkillsVRNodes.Editor.NodeViews.Validation.Impl;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation
{
	public static class NodeViewDataValidationExtensions
    {
		public static IEnumerable<IValidationResult> ValidateData(this BaseNodeView nodeView)
		{
			if (null == nodeView)
			{
				return new List<IValidationResult>(0);
			}

			var validation = nodeView.GetDataValidator();
			return validation.Validate();
		}

		public static void ValidateDataAndShowAlerts(this BaseNodeView view)
		{
			if (null == view)
			{
				return;
			}

			var validation = view.GetDataValidator();
			var results = validation.Validate();
			view.ClearValidationAlerts();
			view.ShowValidationAlerts(results);
			view.ShowOverviewValidationAlertsOnNodeView(results);
		}

		public static void ClearValidationAlerts(this BaseNodeView view)
		{
			view.HideAllAlerts();
		}

		public static void ShowValidationAlert(this BaseNodeView nodeView, IValidationResult result, IVisualElementPathBinding pathBinding = null)
		{
			if (null == nodeView || null == result )
			{
				return;
			}
			if (null == pathBinding)
			{
				pathBinding = nodeView.GetPathBinding();
			}
			var child = null == pathBinding ? nodeView.Q(result.Name) : pathBinding.GetVisualSourceFromPath(result.Name);
			if (null == child)
			{
				return;
			}
			nodeView.ShowAlertOnOther(child, result.WarningLevel, result.Message);
		}

		public static void ShowValidationAlerts(this BaseNodeView nodeView, IEnumerable<IValidationResult> results)
		{
			if (null == nodeView || null == results)
			{
				return;
			}
			
			var validation = nodeView.GetDataValidator();
			var pathBinding = validation as IVisualElementPathBinding;

			var groupResults = results.GroupBy(x => x.Name);
			foreach(var group in groupResults)
			{
				var orderedGroupItems = group.OrderByDescending(x => x.WarningLevel);
				var result = orderedGroupItems.Merge();
				if (null == result)
				{
					continue;
				}
				nodeView.ShowValidationAlert(result, pathBinding);
			}
		}

		public static void ShowOverviewValidationAlertsOnNodeView(this BaseNodeView nodeView, IEnumerable<IValidationResult> results)
		{
			if (null == nodeView || null == results)
			{
				return;
			}
			var validation = nodeView.GetDataValidator();
			var pathBinding = validation as IVisualElementPathBinding;

			var groupResults = results.GroupBy(x => x.Name);
			var elementResults = new List<IValidationResult>();
			foreach (var group in groupResults)
			{
				var orderedGroupItems = group.OrderByDescending(x => x.WarningLevel);
				var result = orderedGroupItems.Merge();
				if (null == result)
				{
					continue;
				}
				elementResults.Add(result);
			}

			var overviewResult = elementResults.Merge("{name}:\r\n{msg}");
			if (null != overviewResult)
			{
				nodeView.ShowAlertOnOther(nodeView, overviewResult.WarningLevel, overviewResult.Message, 30);
			}
		}


		private static IDataValidator GetDataValidator(this BaseNodeView view)
		{
			var validation = ValidationManager.GetDataValidationByType(view.GetType());
			if (validation is InvalidNodeViewDataValidation)
			{
				validation = ValidationManager.GetDataValidationByType(view.nodeTarget.GetType());
			}
			validation.SetValidateSourceObject(view);
			return validation;
		}

		private static IVisualElementPathBinding GetPathBinding(this BaseNodeView view)
		{
			return view.GetDataValidator() as IVisualElementPathBinding;
		}
	}
}