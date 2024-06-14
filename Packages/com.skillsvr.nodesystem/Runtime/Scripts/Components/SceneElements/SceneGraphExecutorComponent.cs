using System;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using Props;
using SkillsVRNodes.Managers;
using SkillsVRNodes.Scripts.Nodes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace SkillsVRNodes
{
	public class SceneGraphExecutorComponent : MonoBehaviour
	{
		[Header("Graph to Run on Start")]
		public SceneGraph graph;

		public NodeExecutor nodeExecutor;

		public UnityEvent onGraphComplete;
		
		public void Awake()
		{
			SetUpGraph(graph);

			//string assistantId = (graph as SceneGraph).npcId;
		}
		public void SetUpGraph(SceneGraph newGraph)
		{
			graph = newGraph;
			nodeExecutor = new NodeExecutor(graph);
			
			nodeExecutor.InitializeGraph();

			nodeExecutor.onEndAction += () => onGraphComplete.Invoke();
		}

		private void OnDestroy()
		{
			nodeExecutor.onEndAction -= () => onGraphComplete.Invoke();

			nodeExecutor.isStopped = true;
		}
		
		private void Start()
		{
			DisableMultipEventSystem();
			if (PropManager.Instance == null)
			{
				GameObject sceneElementsGO = new GameObject
				{
					name = "Prop Manager"
				};
				sceneElementsGO.AddComponent<PropManager>();
				SceneManager.MoveGameObjectToScene(sceneElementsGO, gameObject.scene);
			}
			nodeExecutor.Start();
		}

		public void AddListener(Action action)
		{
			nodeExecutor.onEndAction += action;
		}
		
		public void RemoveListener(Action action)
		{
			nodeExecutor.onEndAction += action;
		}

		private void DisableMultipEventSystem()
		{
			var items = GameObject.FindObjectsOfType<EventSystem>();
			if (items.Length > 1)
			{
				var gos = items.Where(x => x.gameObject.scene == this.gameObject.scene).Select(x => x.gameObject);
				foreach(var go in gos)
				{
					go.SetActive(false);
				}
			}
		}

#if UNITY_EDITOR

		private void Update()
		{
			if (nodeExecutor == null)
			{
				return;
			}

			//Skip Scene
			if (Input.GetKeyDown(KeyCode.J))
			{
				nodeExecutor.onEndAction?.Invoke();
			}
			
			//Skip Node
			if(Input.GetKeyDown(KeyCode.LeftAlt))
			{
				IEnumerable<ExecutableNode> exeNodes = graph.nodes.OfType<ExecutableNode>();

				foreach (ExecutableNode item in exeNodes)
				{
					if (!item.NodeActive)
					{
						continue;
					}
					
					item.SkipNode();
					item.CompleteNode();
					break;
				}
			}
		}

#endif
	}
}