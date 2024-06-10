using GraphProcessor;
using UnityEditor;
using UnityEngine;

namespace SkillsVRNodes.Editor.NodeViews.Validation
{
	public static class GraphWindowDataValidationExtensions
	{
		public static bool defaultAutoFalicationOn = false;

		[InitializeOnLoadMethod]
		private static void AutoStartDataValidation()
		{
			SetAutoDataValidationEnable(null, IsAutoDataValidationEnable());
		}

		private static string ALERTS_VISUAL_PROJECT_KEY { get; set; } = "ALERTS_VISUAL_" + Application.dataPath.Replace("/", "_");
		private static string AUTO_DATA_VALIDATION_PROJECT_KEY { get; set; } = "AUTO_DATA_VALIDATION_" + Application.dataPath.Replace("/", "_");
		public static bool IsAutoDataValidationEnable()
		{
			return EditorPrefs.GetBool(AUTO_DATA_VALIDATION_PROJECT_KEY, defaultAutoFalicationOn);
		}

		public static bool IsShowAlertsEnabled()
		{
			return EditorPrefs.GetBool(ALERTS_VISUAL_PROJECT_KEY, false);
		}

		public static void SetAutoDataValidationEnable(this BaseGraphWindow graphWindow, bool enable)
		{
			EditorPrefs.SetBool(AUTO_DATA_VALIDATION_PROJECT_KEY, enable);

			BaseGraphWindow.onGraphWindowActiveChanged -= ProcessAutoDataValidationOnGraphWindowActiveChangedEvent;
			if (enable)
			{
				BaseGraphWindow.onGraphWindowActiveChanged += ProcessAutoDataValidationOnGraphWindowActiveChangedEvent;
			}

			graphWindow?.EnableAutoDataValidation(enable);
		}

		public static void SetShowAlertsEnable(this BaseGraphWindow graphWindow, bool enable)
		{
			EditorPrefs.SetBool(ALERTS_VISUAL_PROJECT_KEY, enable);
		}

		private static void StartAutoDataValidationOnGraphLoaded( this BaseGraphWindow graphWindow, BaseGraph baseGraph)
		{
			graphWindow.graphView.schedule.Execute(graphWindow.graphView.StartAutoDataValidation).ExecuteLater(30);
		}

		private static void EnableAutoDataValidation(this BaseGraphWindow graphWindow, bool enable)
		{
			if (null == graphWindow)
			{
				return;
			}
			graphWindow.graphLoaded -= graphWindow.StartAutoDataValidationOnGraphLoaded;
			if (enable)
			{
				graphWindow.graphLoaded += graphWindow.StartAutoDataValidationOnGraphLoaded;
			}
			graphWindow.graphView?.SetAutoDataValidationEnable(enable);
		}

		private static void ProcessAutoDataValidationOnGraphWindowActiveChangedEvent(BaseGraphWindow graphWindow, bool isActive)
		{
			graphWindow?.EnableAutoDataValidation(isActive);
            graphWindow.graphLoaded -= graphWindow.ClearGraphViewAlerts;
            if (isActive)
			{
				graphWindow.graphLoaded += graphWindow.ClearGraphViewAlerts;
			}
		}

        private static void ClearGraphViewAlerts(this BaseGraphWindow graphWindow, BaseGraph baseGraph)
        {
            graphWindow.graphView?.ClearValidationAlertsConnection();
        }
    }
}