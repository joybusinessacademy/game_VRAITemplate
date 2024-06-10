using GraphProcessor;
using Scripts.VisualElements;
using SkillsVRNodes.Editor.NodeViews.Validation.Impl;
using System.Linq;
//using System.Runtime.Remoting.Messaging;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace SkillsVRNodes.Editor.NodeViews.Validation
{
	public static class GraphViewDataValidationExtensions
	{
		public static void SetAutoDataValidationEnable(this BaseGraphView graphView, bool enable)
		{
			if (enable)
			{
				graphView.StartAutoDataValidation();
			}
			else
			{
				graphView.StopAutoDataValidation();
			}
		}

		public static void StartAutoDataValidation(this BaseGraphView graphView)
		{
			if (null == graphView)
			{
				return;
			}

			var items = graphView.nodeViews;
			foreach (var view in items)
			{
				view.StartAutoDataValidation();
			}

			graphView.onNodeViewAdded -= graphView.StartAutoValidationForNewNodeView;
			graphView.onNodeViewAdded += graphView.StartAutoValidationForNewNodeView;

			graphView.SetIsInConnectionValidationProcess(false);
			graphView.ExecOnceOnRenderReady(()=> { 
                graphView.ClearValidationAlertsAll();
                graphView.ShowValidationAlertsConnection();
			});


			graphView.computeOrderUpdated -= graphView.ShowValidationAlertsConnection;
			graphView.computeOrderUpdated += graphView.ShowValidationAlertsConnection;
        }

		public static void StopAutoDataValidation(this BaseGraphView graphView)
        {
            if (null == graphView)
            {
                return;
            }
            graphView.onNodeViewAdded -= graphView.StartAutoValidationForNewNodeView;
			graphView.computeOrderUpdated -= graphView.ShowValidationAlertsConnection;
			var items = graphView.nodeViews;
            foreach (var view in items)
            {
                view.StopAutoDataValidation();
            }
        }

        private static void StartAutoValidationForNewNodeView(this BaseGraphView graphView,  BaseNodeView addedNodeView)
		{
			addedNodeView.ExecOnceOnRenderReady(addedNodeView.StartAutoDataValidation);

            // BaseGraphView.GraphViewChangedCallback() override return data and no valid data could be received after.
            // Other graph view changed delegate cannot working correctly unless future update.
            // So no auto connection check for graph view now.
            if (addedNodeView is StartNodeView || addedNodeView is EndNodeView)
			{
				graphView?.ShowValidationAlertsConnection();
			}
		}
       
        private static string GetInProcKey(this BaseGraphView graphView)
        {
            return graphView.GetHashCode() + "IsInPVAProc";
		}
        private static bool GetIsInConnectionValidationProcess(this BaseGraphView graphView)
        {
            if (null == graphView)
            {
                return false;
            }
            string inProcessKey = GetInProcKey(graphView);
			bool isInProc = SessionState.GetBool(inProcessKey, false);
            return isInProc;
		}

		private static void SetIsInConnectionValidationProcess(this BaseGraphView graphView, bool value)
		{
			if (null == graphView)
			{
				return ;
			}
			string inProcessKey = GetInProcKey(graphView);
			SessionState.SetBool(inProcessKey, value);
		}
		public static void ShowValidationAlertsConnection(this BaseGraphView graphView)
        {
            if (null == graphView)
            {
                return;
            }

			var binder = graphView.GetPathBinding();
			var visualTarget = binder.GetVisualSourceFromPath("");
            if (null == visualTarget)
            {
                return;
            }

            
            if (graphView.GetIsInConnectionValidationProcess())
            {
                return;
            }
            graphView.SetIsInConnectionValidationProcess(true);
			var icon = graphView.ShowAlertOnOther(visualTarget, new Vector2(20, 20), WarningLevelEnum.Normal, "Validating Graph Node Flow...", 40, "Icon/Map1");
            icon.style.opacity = 1.0f;
			icon.schedule.Execute(() => { icon.SetunityBackgroundImageTintColor(Color.white); }).ExecuteLater(20);

            // Delay start validation in case of some event callback may trigger multiple times in one frame.
            icon.schedule.Execute(() => {
                graphView.SetIsInConnectionValidationProcess(false);
                var validator = graphView.GetDataValidator();
                var results = validator.Validate();
                var result = results.Merge();

                if (null != result)
                {
                    // show errors
                    icon.SetBackgroundImage("Icon/Error");
                    icon.SetunityBackgroundImageTintColor(Color.red);
                    icon.tooltip = result.Message;
                }
                else
                {
                    // Show check ok anim
                    icon.schedule.Execute(() => { icon.SetBackgroundImage("Icon/Map2"); });
                    icon.schedule.Execute(() => { icon.SetBackgroundImage("Icon/Map3"); }).ExecuteLater(100);
                    icon.schedule.Execute(() => { icon.SetBackgroundImage("Icon/Map4"); }).ExecuteLater(200);
                    icon.schedule.Execute(() => { icon.SetBackgroundImage("Icon/Map5"); }).ExecuteLater(300);
                    icon.schedule.Execute(() => {
                        icon.SetBackgroundImage("Icon/Map");
                        icon.SetunityBackgroundImageTintColor(Color.green);
                        icon.tooltip = "Graph Flow Check: OK";
                        icon.Fade(1.0f, 0.0f, 2000);
                    }).ExecuteLater(400);
                }
            }).ExecuteLater(100);

            
        }

        public static void ShowValidationAlertsNodesOnly(this BaseGraphView graphView)
        {
            if (null == graphView)
            {
                return;
            }
            var items = graphView.nodeViews;
            foreach (var view in items)
            {
                view.ValidateDataAndShowAlerts();
            }
        }
        public static void ShowValidationAlertsFull(this BaseGraphView graphView)
		{
			graphView?.ShowValidationAlertsConnection();
			graphView?.ShowValidationAlertsNodesOnly();
		}

        public static void ClearValidationAlertsConnection(this BaseGraphView graphView)
        {
            graphView?.HideAllAlerts();
            graphView?.HideAllAlerts();
        }

        public static void ClearValidationAlertsNodesOnly(this BaseGraphView graphView)
        {
            if (null == graphView || null == graphView.nodeViews)
            {
                return;
            }
            var items = graphView.nodeViews;
            foreach (var view in items)
            {
                view.ClearValidationAlerts();
            }
        }

        public static void ClearValidationAlertsAll(this BaseGraphView graphView)
        {
            graphView?.ClearValidationAlertsConnection();
            graphView?.ClearValidationAlertsNodesOnly();
        }


        private static IDataValidator GetDataValidator(this BaseGraphView view)
        {
            var validation = ValidationManager.GetDataValidationByType(view.GetType());
            if (validation is InvalidNodeViewDataValidation)
            {
                validation = ValidationManager.GetDataValidationByType(typeof(BaseGraphView));
            }
            validation.SetValidateSourceObject(view);
            return validation;
        }

        private static IVisualElementPathBinding GetPathBinding(this BaseGraphView view)
        {
            return view.GetDataValidator() as IVisualElementPathBinding;
        }
    }
}