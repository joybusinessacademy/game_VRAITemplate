using System;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using SceneNavigation;
using SkillsVRNodes.Managers.Utility;
using UnityEditor;
using UnityEngine.UIElements;
using System.Linq;
using SkillsVRNodes.Managers.GraphNavigator;

namespace SkillsVRNodes.Editor.Graph
{
	public class SkillsVRGraphWindow : BaseGraphWindow
	{
		public new static SkillsVRGraphWindow GetWindow => GetWindow<SkillsVRGraphWindow>();
		
		public Stack<BaseGraph> graphFolderStack = new Stack<BaseGraph>();
		public Stack<BaseGraph> graphBackStack = new Stack<BaseGraph>();

		public static void RefreshGraph()
		{
			GetWindow.graphView?.Refresh();
		}

		public static void FrameAll()
		{
			GetWindow.graphView?.FrameAll();
		}

		protected override void OnEnable()
		{
			base.OnEnable();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			GC.Collect();
		}

		//public GraphNavigator GraphNavigator;
		protected override void OnInitializeWindow(BaseGraph graph)
		{
			minSize = new Vector2(830, 430);

			titleContent = new GUIContent("Graph Editor", Resources.Load<Texture2D>("Icon/SkillsVRCCK"));

			graphView = CreateNewGraphViewInstance();
			
			rootView.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);

			rootView.Add(graphView);

			OnBackingScaleFactorChanged();
		}

		protected virtual BaseGraphView CreateNewGraphViewInstance()
		{
			return new SkillsVRGraphView(this);
		}

		public Action tabDetached = () => { };
		public Action tabAdded = () => { };
		private void OnAddedAsTab()
		{
			tabAdded.Invoke();
		}
		private void OnTabDetached()
		{
			tabDetached.Invoke();
		}

		[MenuItem("SkillsVR CCK/Open Graph View", priority = -10)]
		public static void OpenGraph()
		{
			CCKDebug.Log("Used Menu Item: Opening Graph View");

			OpenGraph(GraphFinder.CurrentGraph);
		}
		
		public static void OpenHomeGraph()
		{
			OpenGraph(GraphFinder.HomeGraph);
		}
		
		public static void OpenGraph(BaseGraph graph)
		{
			GetWindow.LoadGraph(graph);
		}
	}
}
