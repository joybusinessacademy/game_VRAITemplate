using System;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using GraphProcessor;
using Props;
using Props.PropInterfaces;
using SkillsVR.Mechanic.Core;
using SkillsVR.UnityExtenstion;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SkillsVRNodes.Scripts.Nodes
{
	public interface ISpawnerNode
	{
		string GetSpawnPosition();
		GameObject GetSpawnGameObject();
	}

	public class SpawnerNode<SPAWNER_TYPE, INTERFACE_TYPE, DATA_TYPE> : ExecutableNode, ISpawnerNode
		where SPAWNER_TYPE : AbstractMechanicSpawner<INTERFACE_TYPE, DATA_TYPE>
		where INTERFACE_TYPE : IMechanicSystem<DATA_TYPE>
		where DATA_TYPE : new()
	{
		public enum Size
		{
			Small,
			Medium,
			Large,
		}

		[HideInInspector] public PropGUID<IPropTransform> spawnPosition = new PropGUID<IPropTransform>(true);
		[HideInInspector] public bool panoramaSpawnPosition = false;
		[HideInInspector] public SPAWNER_TYPE mechanicSpawner;

		public Size SpawnSize = Size.Medium;
		public override Color color => NodeColours.Learning;

		public virtual DATA_TYPE MechanicData { get => mechanicData; set => mechanicData = value; }
		[SerializeField] public DATA_TYPE mechanicData;

		public int number;
		
		[HideInInspector] public GameObject mechanicObject;

		protected const string UnregisterMediatorId = "UnregisterMediator";
		
		protected virtual void CreateData()
        {
			MechanicData = new DATA_TYPE();
        }

        public override void OnNodeCreated()
        {
	        if (MechanicData == null)
	        {
				CreateData();
	        }
	        
            base.OnNodeCreated();
        }

        public virtual void SpawnObject()
		{
			if (MechanicData == null)
			{
				Debug.LogError("ERROR: No data in node");
				return;
			}
			mechanicSpawner = (SPAWNER_TYPE)MechanicProvider.Current.GetMechanic<DATA_TYPE>(typeof(SPAWNER_TYPE));
			mechanicSpawner?.Reset();
			mechanicObject = mechanicSpawner.gameObject;
			

			mechanicObject.AddComponent<Canvas>();
			mechanicObject.AddComponent<CanvasGroup>().alpha = 0;
			IPropTransform spawnParent = spawnPosition.GetProp();
			if (spawnParent != null)
			{
				if (null != mechanicObject.transform.parent)
				{
					Transform nullTransform = null;
					mechanicObject.transform.SetParent(nullTransform);
				}
				SceneManager.MoveGameObjectToScene(mechanicObject, spawnParent.GetTransform().gameObject.scene);
				mechanicObject.SetParent(spawnParent.GetTransform().gameObject);
				mechanicObject.transform.localPosition = Vector3.zero;
				mechanicObject.transform.localRotation = Quaternion.identity;
				CustomObjectMovement customObjectMovement = spawnParent.GetTransform().GetOrAddComponent<CustomObjectMovement>();

				customObjectMovement.enabled = panoramaSpawnPosition;
			}
			else if (PropManager.Instance != null)
			{
				SceneManager.MoveGameObjectToScene(mechanicObject, PropManager.Instance.gameObject.scene);
			}

			mechanicObject.transform.localScale = SpawnSize switch
			{
				Size.Small => Vector3.one * 0.5f,
				Size.Medium => Vector3.one * 1.0f,
				Size.Large => Vector3.one * 2.0f,
				_ => throw new ArgumentOutOfRangeException()
			};
			mechanicSpawner.mechanicData = MechanicData;
		}
		
		protected void StartListener(IMechanicSystemEvent eventArgs)
		{
			if (mechanicSpawner == null)
			{
				Debug.LogError("ERROR: mechanic spawner");
				CompleteNode();
				return;
			}

			switch (eventArgs.eventKey)
			{
				case MechSysSpawnStateEvent.Ready:
					GameObject.DestroyImmediate(mechanicObject.GetComponent<CanvasGroup>());
					GameObject.DestroyImmediate(mechanicObject.GetComponent<Canvas>());	

					// this should be done via prefab, too risky to update the prefab today
					var canvases = mechanicObject.GetComponentsInChildren<Canvas>(true);
					canvases.ToList().ForEach(canvas => 
					{
						canvas.overrideSorting = false;
						canvas.sortingOrder = 1;
					});
					
					mechanicSpawner.StartMechanic();
					mechanicSpawner.RemoveListener(StartListener);
					break;
			}
		}

		protected override void OnInitialise()
		{
			base.OnInitialise();
			SpawnObject();
		}
		protected override void OnStart()
		{
			base.OnStart();

			if (null == mechanicSpawner)
			{
				SpawnObject();
			}
			mechanicSpawner.RemoveListener(MechanicListener);
			mechanicSpawner.AddListerner(MechanicListener);

			if (mechanicSpawner.ready)
			{
				GameObject.DestroyImmediate(mechanicObject.GetComponent<CanvasGroup>());
				GameObject.DestroyImmediate(mechanicObject.GetComponent<Canvas>());	

				// this should be done via prefab, too risky to update the prefab today
				var canvases = mechanicObject.GetComponentsInChildren<Canvas>(true);
				canvases.ToList().ForEach(canvas => 
				{
					canvas.overrideSorting = false;
					canvas.sortingOrder = 1;
				});
				mechanicSpawner.StartMechanic();
			}
			else
			{
				mechanicSpawner.AddListerner(StartListener);
			}
		}
		
		protected virtual void MechanicListener(IMechanicSystemEvent mechanicSystemEvent)
		{
			switch (mechanicSystemEvent.eventKey)
			{
				case MechSysEvent.AfterFullStop:
					CompleteNode();
					break;
				case MechSysMonoEvents.OnGameObjectActiveChanged:
					{
						if (mechanicSystemEvent.GetData<bool>())
						{
							break;
						}
						// If game object is disabled, release mechanicSpawner.
						CompleteNode();
						if (null != mechanicSpawner)
						{
							mechanicSpawner.RemoveListener(MechanicListener);
							mechanicSpawner.mechanicData = default;
						}
						mechanicSpawner = null;
						mechanicObject = null;
						break;
					}
			}
		}

		protected override void OnSkip()
		{
			if(mechanicSpawner != null)
			{
				mechanicSpawner.StopMechanic();
			}//mechanicSpawner.gameObject.SetActive(false);
		}

		public string GetSpawnPosition()
		{
			return spawnPosition;
		}

		public GameObject GetSpawnGameObject()
		{
			return mechanicObject;
		}
	}
}
