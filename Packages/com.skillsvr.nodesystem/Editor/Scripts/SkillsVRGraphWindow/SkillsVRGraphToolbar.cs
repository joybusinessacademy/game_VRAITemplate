using System;
using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
//using NUnit.Framework.Internal;
using SceneNavigation;
using Scripts.VisualElements;
using SkillsVR.UnityExtenstion;
using SkillsVR.VisualElements;
using SkillsVRNodes.Editor.NodeViews;
using SkillsVRNodes.Managers.GraphNavigator;
using SkillsVRNodes.Managers.Setup;
using SkillsVRNodes.Managers.Utility;
using SkillsVRNodes.Scripts;
using SkillsVRNodes.Scripts.Nodes;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VisualElements;

namespace SkillsVRNodes.Editor.Graph
{
	public class SkillsVRGraphToolbar : VisualElement, IDisposable
	{
		private readonly VisualElement leftContainer;
		private readonly VisualElement rightContainer;
		private readonly VisualElement tooltipContainer;

		public VisualElement fullScreenButton;
		public BaseGraphView GraphView { get; private set; }

		private Button jumpToOptions;
		public Breadcrumbs Breadcrumbs;

		private Label tooltipLabel;

		public SkillsVRGraphToolbar(BaseGraphView graphView)
		{
			GraphView = graphView;
			name = "toolbar";
			styleSheets.Add(Resources.Load<StyleSheet>("SkillsStyles/toolbar"));

			VisualElement parentBar = new VisualElement()
			{
				name = "barContainer"
			};

			leftContainer = new VisualElement
			{
				name = "leftContainer"
			};
			rightContainer = new VisualElement
			{
				name = "rightContainer"
			};
			tooltipContainer = new VisualElement
			{
				name = "tooltipContainer"
			};

			AddTooltipLabel("");
			
			SetToolTipText(GraphTooltipManager.GetRandomTip());

			parentBar.Add(leftContainer);
			parentBar.Add(rightContainer);
			Add(parentBar);
			Add(tooltipContainer);
			Refresh();
		}

		public virtual void Dispose()
		{
			GraphView = null;
		}

		public void Refresh()
		{
			rightContainer.Clear();
			
			AddButtons();
		}

		public void CreateTitle()
		{
			Breadcrumbs = new Breadcrumbs();

			leftContainer.Add(Breadcrumbs);
		}

		protected void AddButtons()
		{
			jumpToOptions = new Button
			{
				name = "jumpto-project-button",
				style =
				{
					paddingLeft = 6,
				}
			};
			jumpToOptions.clicked += OpenJumpToMenu;
			jumpToOptions.Add(new Label("Jump to"));
			jumpToOptions.Add(new Image() { image = Resources.Load<Texture2D>("Icon/Expand Down") });
			rightContainer.Add(jumpToOptions);

			rightContainer.Add(new GraphWindowDataValidationUI());

			rightContainer.Add(AddIconButton("Refresh", () => GraphView.Refresh(), "Refresh"));
		}

		public void OpenJumpToMenu()
		{
			GenericMenu options = new();
			
			options.AddItem(new GUIContent("Start"), false, () =>
			{
				GraphView.ClearSelection();
				GraphView.SelectNodesOfType<StartNode>();
				GraphView.FrameSelection();
			});
			options.AddItem(new GUIContent("End"), false, () =>
			{
				GraphView.ClearSelection();
				GraphView.SelectNodesOfType<EndNode>();
				GraphView.FrameSelection();
			});

			options.AddItem(new GUIContent("Center"), false, () =>
			{
				GraphView.FrameAll();
			});
			options.AddItem(new GUIContent("Selected"), false, () =>
			{
				GraphView.FrameSelection();
			});
			
			options.ShowAsContext();

		}

		private void UpdateFullscreenButton()
		{
			if (SkillsVRGraphWindow.GetWindow.docked)
			{
				fullScreenButton.RemoveFromClassList("hide");
			}
			else
			{
				fullScreenButton.AddToClassList("hide");
			}

			fullScreenButton[0].visible = !SkillsVRGraphWindow.GetWindow.maximized;
			fullScreenButton[1].visible = SkillsVRGraphWindow.GetWindow.maximized;

			if (SkillsVRGraphWindow.GetWindow.maximized)
			{
				fullScreenButton[0].AddToClassList("hide");
				fullScreenButton[1].RemoveFromClassList("hide");
			}
			else
			{
				fullScreenButton[0].RemoveFromClassList("hide");
				fullScreenButton[1].AddToClassList("hide");
			}
		}

		// public void GenerateGraphStackButtons(Stack<BaseGraph> baseGraphs)
		// {
		// 	VisualElement container = new VisualElement
		// 	{
		// 		name = "graphsPath"
		// 	};
		// 	
		// 	if (baseGraphs == null || baseGraphs.Count == 0 || baseGraphs.ToArray()[0] == null )
		// 	{
		// 		container.Add(AddTextButton("No Graph Open", () => { }));
		// 		return;
		// 	}
		// 	CreateStackButtons(baseGraphs, container);
		// 	leftContainer.Add(container);
		// }

		// private void CreateStackButtons(Stack<BaseGraph> baseGraphs, VisualElement container)
		// {
		// 	for (int i = 0; i < baseGraphs.Count; i++)
		// 	{
		// 		BaseGraph graph = baseGraphs.ToArray()[i];
		//
		// 		int popAmount = i;
		// 		VisualElement button = AddTextButton(graph.name, () =>
		// 		{
		// 			for (int j = 0; j < popAmount; j++)
		// 			{
		// 				SkillsVRGraphWindow.GetWindow.graphFolderStack.Pop();
		// 			}
		//
		// 			SkillsVRGraphWindow.OpenGraphInFolder(graph);
		// 		});
		// 		
		// 		if (i != 0)
		// 		{
		// 			Image chevron = new()
		// 			{
		// 				name = "chevron",
		// 				image = Resources.Load<Texture2D>("Icon/Right")
		// 			};
		// 			container.Add(chevron);
		// 		}
		// 		container.Add(button);
		// 	}
		// }

		public VisualElement AddTextButton(string text, Action onClick)
		{
			Button button = new Button()
			{
				text = text,
				name = "textButton"
			};

			button.clicked += onClick;
			return button;
		}

		public void AddTooltipLabel(string tooltipText)
		{
			tooltipLabel = new Label(tooltipText);

			Image image = new()
			{
				image = Resources.Load<Texture2D>("Icon/Info"),
				style =
				{
					width = 16, 
					height = 16,
					marginBottom = 4,
					marginTop = 4,
					marginRight = 4,
					marginLeft = 4,
				}
				
			};
			
			image.tintColor = new Color(0.5f, 0.84f, 0.99f);

			if (!tooltipContainer.Contains(image))
				tooltipContainer.Add(image);
			if(!tooltipContainer.Contains(tooltipLabel))
				tooltipContainer.Add(tooltipLabel);
		}
		
		public void SetTooltipVisbility(bool state)
		{
			tooltipContainer.SetDisplay(state);
		}

		public void SetToolTipText(string text, bool onTimer = true)
		{
			tooltipLabel.text = text;

			if (onTimer)
				EditorCoroutineUtility.StartCoroutineOwnerless(TooltipRotation());
			else
				SetTooltipVisbility(true);
		}

		private IEnumerator TooltipRotation()
		{
			SetTooltipVisbility(true);
			yield return new EditorWaitForSeconds(5);
			SetTooltipVisbility(false);
		}

		public VisualElement AddIconButton(string iconName, Action onClick, string tooltip)
		{
			IconButton iconButton = new IconButton(iconName, 16, 4)
			{
				tooltip = tooltip
			};

			iconButton.clicked += onClick;
			return iconButton;
		}
		
		public VisualElement AddGap(int width)
		{
			VisualElement visualElement = new VisualElement()
			{
				name = "gap",
				style = { width = width },
			};
			return visualElement;
		}
	}
}