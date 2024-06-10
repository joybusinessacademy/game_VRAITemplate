using GraphProcessor;
using Scripts.VisualElements;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation
{
	public static class NodeViewAutoDataValidationExtensions
	{
		public static void StartAutoDataValidation(this BaseNodeView nodeView)
		{
			nodeView.StopAutoDataValidation();
			nodeView.RegisterCallback<GeometryChangedEvent>(nodeView.ExecDataValidataionOnGeometryChangedEvent);
			nodeView.AddDataValidationToValueChangedEvents();
			nodeView.DelayCheckValidationAlert(10);
		}

		public static void StopAutoDataValidation(this BaseNodeView nodeView)
		{
			nodeView.UnregisterCallback<GeometryChangedEvent>(nodeView.ExecDataValidataionOnGeometryChangedEvent);
			nodeView.RemoveDataValidationFromValueChangedEvents();
			nodeView.CancelDelayCheck();
			//nodeView.ClearValidationAlerts();
		}

		private static void AddDataValidationToValueChangedEvents(this BaseNodeView nodeView)
		{
			nodeView.RegisterDataValidationOnChangedValueType<bool>();
			nodeView.RegisterDataValidationOnChangedValueType<Object>();
			nodeView.RegisterDataValidationOnChangedValueType<int>();
			nodeView.RegisterDataValidationOnChangedValueType<float>();
			nodeView.RegisterDataValidationOnChangedValueType<string>();
			nodeView.RegisterDataValidationOnChangedValueType<Color>();
		}

		private static void RemoveDataValidationFromValueChangedEvents(this BaseNodeView nodeView)
		{
			nodeView.UnregisterDataValidationOnChangedValueType<bool>();
			nodeView.UnregisterDataValidationOnChangedValueType<Object>();
			nodeView.UnregisterDataValidationOnChangedValueType<int>();
			nodeView.UnregisterDataValidationOnChangedValueType<float>();
			nodeView.UnregisterDataValidationOnChangedValueType<string>();
			nodeView.UnregisterDataValidationOnChangedValueType<Color>();
		}

		private static void RegisterDataValidationOnChangedValueType<VALUE_TYPE>(this BaseNodeView nodeView)
		{
			var allControls = nodeView.Query<VisualElement>().ToList().Where(x => x is INotifyValueChanged<VALUE_TYPE>)
				.Select(x => (INotifyValueChanged<VALUE_TYPE>)x);
			foreach (var control in allControls.ToList())
			{
				control.UnregisterValueChangedCallback<VALUE_TYPE>(nodeView.ExecDataValidationOnValueChangedCallback<VALUE_TYPE>);
				control.RegisterValueChangedCallback<VALUE_TYPE>(nodeView.ExecDataValidationOnValueChangedCallback<VALUE_TYPE>);
			}
		}

		private static void UnregisterDataValidationOnChangedValueType<VALUE_TYPE>(this BaseNodeView nodeView)
		{
			var allToggles = nodeView.Query<VisualElement>().ToList().Where(x => x is INotifyValueChanged<VALUE_TYPE>)
				.Select(x => (INotifyValueChanged<VALUE_TYPE>)x);
			foreach (var toggle in allToggles.ToList())
			{
				toggle.UnregisterValueChangedCallback<VALUE_TYPE>(nodeView.ExecDataValidationOnValueChangedCallback<VALUE_TYPE>);
			}
		}

		private static void ExecDataValidationOnValueChangedCallback<VALUE_TYPE>(this BaseNodeView nodeView, ChangeEvent<VALUE_TYPE> evt)
		{
			nodeView.DelayCheckValidationAlert(10);
		}

		private static void ExecDataValidataionOnGeometryChangedEvent (this BaseNodeView node, GeometryChangedEvent evt)
		{
			var nodeView = evt.target as BaseNodeView;
			if (null == nodeView)
			{
				return;
			}

			if (evt.oldRect.yMin != evt.newRect.yMin || evt.oldRect.xMin != evt.newRect.xMin)
			{
				return;
			}

			nodeView.AddDataValidationToValueChangedEvents();
			nodeView.DelayCheckValidationAlert(10);
		}

		private const string DELAY_VALUDATE_USER_DATA_KEY = "DelayValidationCheck";
		private static void DelayCheckValidationAlert(this BaseNodeView nodeView, long delayMs = 500)
		{
			var scheduledAction = nodeView.GetUserData<IVisualElementScheduledItem>(DELAY_VALUDATE_USER_DATA_KEY);
			if (null == scheduledAction)
			{
				scheduledAction = nodeView.schedule.Execute(
					() => {
						nodeView.ValidateDataAndShowAlerts();
					});
				nodeView.SetUserData<IVisualElementScheduledItem>(DELAY_VALUDATE_USER_DATA_KEY, scheduledAction);
			}

			delayMs = 0 >= delayMs ? 10 : delayMs;
			scheduledAction.ExecuteLater(delayMs);
		}

		private static void CancelDelayCheck(this BaseNodeView nodeView)
		{
			var scheduledAction = nodeView.GetUserData<IVisualElementScheduledItem>(DELAY_VALUDATE_USER_DATA_KEY);
			if (null == scheduledAction)
			{
				return;
			}
			scheduledAction.Pause();
			nodeView.RemoveUserData(DELAY_VALUDATE_USER_DATA_KEY);
		}
	}
}