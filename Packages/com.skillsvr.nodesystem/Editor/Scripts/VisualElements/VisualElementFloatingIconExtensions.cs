using SkillsVRNodes.Editor.NodeViews;
using SkillsVRNodes.Editor.NodeViews.Validation;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace Scripts.VisualElements
{
	public static class VisualElementFloatingIconExtensions
	{
		public static VisualElement ShowAlert(this VisualElement visual, WarningLevelEnum warningLevel, string message = null, int size = 40, Texture2D customIcon = null)
		{
			return visual.ShowAlertAtWorldPos(visual.GetBindingAlertName(), visual.worldBound.min, warningLevel, message, size, customIcon);
		}

		public static VisualElement ShowAlertOnOther(this VisualElement visual, VisualElement other, WarningLevelEnum warningLevel, string message = null, int size = 20, Texture2D customIcon = null)
		{
			var pos = other.GetWorldBoundTopRight();
			return visual.ShowAlertAtWorldPos(other.GetBindingAlertName(), pos, warningLevel, message, size, customIcon);
		}

		public static VisualElement ShowAlertOnOther(this VisualElement visual, VisualElement other, WarningLevelEnum warningLevel, string message, int size, string customIconPath)
		{
			var pos = other.GetWorldBoundTopRight();
			return visual.ShowAlertAtWorldPos(other.GetBindingAlertName(), pos, warningLevel, message, size, string.IsNullOrWhiteSpace(customIconPath) ? null : Resources.Load<Texture2D>(customIconPath));
		}

		public static VisualElement ShowAlertOnOther(this VisualElement visual, VisualElement other, Vector2 offsetPos, WarningLevelEnum warningLevel, string message, int size, string customIconPath)
		{
			var pos = other.GetWorldBoundTopRight() + offsetPos;
			return visual.ShowAlertAtWorldPos(other.GetBindingAlertName(), pos, warningLevel, message, size, string.IsNullOrWhiteSpace(customIconPath) ? null : Resources.Load<Texture2D>(customIconPath));
		}

		public static VisualElement HideAlertOnOther(this VisualElement visual, VisualElement other)
		{
			var alertUI = visual.GetAlertRoot().FindAlertByChild(other);
			alertUI.Hide();
			return alertUI;
		}

		public static ValueAnimation<float> FadeAlertOnOther(this VisualElement visual, VisualElement other, float fromAlpha, float toAlpha, int durationMS = 1000)
		{
			var alertUI = visual.GetAlertRoot().FindAlertByChild(other);
			return alertUI.experimental.animation
				.Start(fromAlpha, toAlpha, Mathf.Max(0, durationMS), (visual, v) => {
					visual.style.opacity = v; });
		}

		public static ValueAnimation<float> Fade(this VisualElement visual, float fromAlpha, float toAlpha, int durationMS = 1000)
		{
			var alertUI = visual;
			return alertUI.experimental.animation
				.Start(fromAlpha, toAlpha, Mathf.Max(0, durationMS), (visual, v) => {
					visual.style.opacity = v;
				});
		}

		public static void HideAllAlerts(this VisualElement visual)
		{
			visual.GetAlertRoot().Children().ForEach(x => x.Hide());
		}

		public static string GetBindingAlertName(this VisualElement visual)
		{
			string name= string.IsNullOrWhiteSpace(visual.name) ? "no-name" : visual.name;
			return name + "-alert" + visual.GetHashCode().ToString();
		}

		public static VisualElement FindAlertByName(this VisualElement root, string name)
		{
			return root.GetOrCreatChildByName(name);
		}

		public static VisualElement FindAlertByChild(this VisualElement root, VisualElement child)
		{
			return root.GetOrCreatChildByName(child.GetBindingAlertName());
		}

		private static VisualElement GetAlertRoot(this VisualElement root)
		{
			string alertRootName = "AlertCoverRoot";
			return root.GetOrCreatChildByName(alertRootName);
		}

		public static VisualElement ShowAlertAtWorldPos(this VisualElement nodeView, string name, Vector2 worldPos, WarningLevelEnum warningLevel, string message = null, int size = 20, Texture2D customIcon = null)
		{
			var alertRoot = nodeView.GetAlertRoot();
			alertRoot.style.position = Position.Absolute;
			alertRoot.pickingMode = PickingMode.Ignore;

			alertRoot.ExecOnceOnRenderReady(() =>{
				alertRoot.CopyPosAndSizeFrom(nodeView);
				alertRoot.BringToFront();
			});
			
			var alertUI = alertRoot.FindAlertByName(name);
			alertUI.Show();
			switch (warningLevel)
			{
				
				case WarningLevelEnum.Warning:
					alertUI.SetAsWarningIcon(message, customIcon, size);
					break;
				case WarningLevelEnum.Error:
					alertUI.SetAsErrorIcon(message, customIcon, size);
					break;
				case WarningLevelEnum.None:
				case WarningLevelEnum.Normal:
				default:
					alertUI.SetAsWarningIcon(message, customIcon, size);
					break;
			}

			alertUI.ExecOnceOnRenderReady(() => {
				alertUI.MoveToWorldPosition(worldPos - alertUI.GetworldTransformSize() * 0.5f);
			});
			
			return alertUI;
		}


		public static void SetAsErrorIcon(this VisualElement visual, string message = null, Texture2D customIcon = null, int size = 20)
		{
			visual.SetAsFloatingIcon(message, null == customIcon ? Resources.Load<Texture2D>("Icon/Error") : customIcon, size);
		}

		public static void SetAsWarningIcon(this VisualElement visual, string message = null, Texture2D customIcon = null, int size = 20)
		{
			visual.SetAsFloatingIcon(message, null == customIcon ? Resources.Load<Texture2D>("Icon/Warning") : customIcon, size);
		}

		public static void SetAsFloatingIcon(this VisualElement visual, string message = null, Texture2D icon = null, int size = 20)
		{
			visual.SetBackgroundImage(icon);
			visual.tooltip = message;
			visual.SetSize(size);
			visual.style.position = Position.Absolute;
			visual.pickingMode = PickingMode.Position;
			visual.BringToFront();
		}
	}
}