using Scripts.VisualElements;
using SkillsVRNodes.Editor.NodeViews.Validation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VisualElements;

public class GraphWindowDataValidationUI : VisualElement
{
	public VisualElement showAlertsButton { get; private set; }
	public Button autoDataValidationButton { get; private set; }
	public Button dropdownButton { get; private set; }

	private Color defaultColor = new Color(0.35f, 0.35f, 0.35f, 1);
	private Color selectedColor = new Color(0.27f, 0.37f,0.49f , 1);

	public GraphWindowDataValidationUI()
	{
		styleSheets.Add(Resources.Load<StyleSheet>("SkillsStyles/toolbar"));

		CreateUI();
	}

	private void CreateUI()
	{
		//this.SetSize(30);
		this.SetPadding(0);
		this.SetMargin(4,0,0,0);
		this.style.flexDirection = FlexDirection.Row;

		showAlertsButton = AddIconButton("alert", () => ToggleVisibilityForAlerts(), "Show Alerts");
		showAlertsButton.style.marginRight = 7;

		RefreshShowAlertsButtonByState(GraphWindowDataValidationExtensions.IsShowAlertsEnabled());
		this.Add(showAlertsButton);

		SetupAutoValidationButton();
		Button buildDropdown = new Button
		{
			name = "debug-dropdown",
			tooltip = "Debug Settings",
			style =
			{
				borderLeftWidth = 0,
			}
		};
		buildDropdown.Add(new Image() { image = Resources.Load<Texture2D>("Icon/Expand Down") });
		buildDropdown.clicked += BuildDropdown;
		this.Add(buildDropdown);
	}

	public IconButton AddIconButton(string iconName, Action onClick, string tooltip)
	{
		IconButton iconButton = new IconButton(iconName, 16, 4)
		{
			tooltip = tooltip
		};
		
		iconButton.clicked += onClick;
		return iconButton;
	}

	private void BuildDropdown()
	{
		bool autoValidateOn = GraphWindowDataValidationExtensions.IsAutoDataValidationEnable();

		GenericMenu menu = new GenericMenu();
		menu.AddItem(new GUIContent("Auto validate nodes"), autoValidateOn, () =>
		{
			ToggleAutoDataValidation();
		});

		menu.ShowAsContext();
	}

	private void ValidationCurrentGraphConnections()
	{
		if (null == BaseGraphWindow.Instance || null == BaseGraphWindow.Instance.graphView)
		{
			return;
		}
		BaseGraphWindow.Instance.graphView.ShowValidationAlertsConnection();
	}

	private void SetupAutoValidationButton()
	{
		autoDataValidationButton = AddIconButton("Bug", BaseGraphWindow.Instance.graphView.ShowValidationAlertsFull, "Validate current graph ");
		Add(autoDataValidationButton);
		autoDataValidationButton.style.marginRight = 0;
		autoDataValidationButton.style.borderBottomRightRadius = 0;
		autoDataValidationButton.style.borderTopRightRadius = 0;
		autoDataValidationButton.style.paddingLeft = 8;
		autoDataValidationButton.style.paddingRight = 8;
		RefreshAutoValidationButtonByState(GraphWindowDataValidationExtensions.IsAutoDataValidationEnable());
	}

	private void ToggleAutoDataValidation()
	{
		bool newValue = !GraphWindowDataValidationExtensions.IsAutoDataValidationEnable();
		GraphWindowDataValidationExtensions.SetAutoDataValidationEnable(BaseGraphWindow.Instance, newValue);
		RefreshAutoValidationButtonByState(newValue);
	}

	private void ToggleVisibilityForAlerts()
	{
		bool newValue = !GraphWindowDataValidationExtensions.IsShowAlertsEnabled();
		GraphWindowDataValidationExtensions.SetShowAlertsEnable(BaseGraphWindow.Instance, newValue);

		if (newValue == false)
		{
			BaseGraphWindow.Instance.graphView.ClearValidationAlertsAll();
		}else
		{
			BaseGraphWindow.Instance.graphView.ShowValidationAlertsFull();
		}

		RefreshShowAlertsButtonByState(newValue);
	}

	private void RefreshAutoValidationButtonByState(bool isOn)
	{
		autoDataValidationButton.style.backgroundColor = (isOn ? selectedColor : defaultColor);
		autoDataValidationButton.tooltip = "Validate current graph ";
	}

	private void RefreshShowAlertsButtonByState(bool isOn)
	{																													
		showAlertsButton.SetBackgroundColor(isOn ? selectedColor : defaultColor);
	}
}
