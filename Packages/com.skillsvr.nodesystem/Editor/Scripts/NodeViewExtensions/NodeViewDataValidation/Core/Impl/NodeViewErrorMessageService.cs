using GraphProcessor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
	public class NodeViewErrorMessageService
	{

		public void ShowMessageOnViewProp(VisualElement visual, WarningLevelEnum warningLevel, string message = null)
		{

		}

		public void HideMessageOnViewProp(VisualElement visual)
		{

		}


		
		
		public VisualElement GetIconElement(VisualElement parent)
		{
			string iconName = "FloatingAlertIcon";
			var iconUI = null == parent ? null : parent.Q(iconName);
			if (null == iconUI)
			{
				iconUI = new VisualElement();
				parent.Add(iconUI);
				iconUI.BringToFront();
			}

			return iconUI;
		}

		public void SetMessageOnUI(VisualElement visualElement, string message)
		{
			visualElement.tooltip = message;
		}
		public void SetColorOnUI(VisualElement visualElement, Color color)
		{
			visualElement.style.backgroundColor = color;
		}
		public void SetIconOnUI(VisualElement visualElement, Texture2D icon)
		{
			visualElement.style.backgroundImage = icon;
		}
		/*
		public VisualElement GetMessagePanelRoot()
		{
			var errorVisualRoot = TargetNodeView.Q("ErrorRoot");
			if (null == errorVisualRoot)
			{
				errorVisualRoot = new VisualElement();
				errorVisualRoot.name = "ErrorRoot";
				errorVisualRoot.style.position = Position.Absolute;
				errorVisualRoot.pickingMode = PickingMode.Ignore;
				errorVisualRoot.style.overflow = Overflow.Visible;
				TargetNodeView.Add(errorVisualRoot);
			}

			errorVisualRoot.CopyPosAndSizeFrom(TargetNodeView);
			errorVisualRoot.BringToFront();
			return errorVisualRoot;
		}


		public VisualElement GetMessageVisualByName(VisualElement root, string name)
		{
			var error = root.Q(name);
			if (null == error)
			{
				error = new VisualElement();
				error.name = name;
				error.style.backgroundColor = new Color(1.0f, 0.0f, 0.0f, 0.6f);
				error.style.position = Position.Absolute;
				error.pickingMode = PickingMode.Ignore;
				error.style.overflow = Overflow.Visible;

				var icon = new VisualElement();
				icon.style.backgroundImage = new StyleBackground(Resources.Load<Texture2D>("Icon/Home"));
				icon.style.width = 16;
				icon.style.height = 16;
				icon.style.top = -8;
				icon.style.right = -8;
				icon.tooltip = r.Message;
				icon.style.overflow = Overflow.Visible;
				icon.style.position = Position.Absolute;
				icon.BringToFront();

				error.Add(icon);
				errorVisualRoot.Add(error);

			}

			error.CopyOffsetPosition(errorVisualRoot, visual);
			error.CopySizeFrom(visual, true);
			error.BringToFront();
		}

		public void ShowErrorOnUI(VisualElement visualElement, string message, Texture2D icon = null, Color bgColor = default)
		{
			if (null == icon)
			{
				icon = Resources.Load<Texture2D>("Icon/Error");
			}
			if (default == bgColor)
			{
				bgColor = new Color(1.0f, 0.0f, 0.0f, 0.4f);
			}
			ShowMessageOnVisual(visualElement, message, icon, bgColor);
		}

		public void ShowWarningOnUI(VisualElement visualElement, string message, Texture2D icon = null, Color bgColor = default)
		{
			if (null == icon)
			{
				icon = Resources.Load<Texture2D>("Icon/Warning");
			}
			if (default == bgColor)
			{
				bgColor = new Color(1.0f, 0.9f, 0.0f, 0.4f);
			}
			ShowMessageOnVisual(visualElement, message, icon, bgColor);
		}
		*/
	}
}