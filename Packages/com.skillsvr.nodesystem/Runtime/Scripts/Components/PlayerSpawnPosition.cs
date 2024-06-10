using Props;
using SkillsVR.UnityExtenstion;
using UnityEngine;
using UnityEngine.Serialization;

namespace SkillsVRNodes
{
	public class PlayerSpawnPosition : MonoBehaviour
	{
		public bool setAsParent = false;
		private string uid;

		public string Uid => uid;

		private void Awake()
		{
			uid = SystemInfo.deviceUniqueIdentifier;
			
			PlayerDistributer.RegisterSpawnPosition(this);
			
			GameObject player = PlayerDistributer.GetPlayer(SystemInfo.deviceUniqueIdentifier);
			TeleportTo(transform, Uid);
			
			if (setAsParent)
			{
				player.transform.parent = transform;
			}
		}

		public void DirectlySetPlayerPos(Transform setTo)
		{
			TeleportTo(setTo, Uid);
		}

		public static void TeleportTo(Transform teleportPosition, string uid)
		{
			PlayerSpawnPosition spawnPosition = PlayerDistributer.TryGetSpawnPosition(uid);
			if (spawnPosition != null && teleportPosition != spawnPosition.transform)
			{
				spawnPosition.transform.position = teleportPosition.position;
				spawnPosition.transform.rotation = teleportPosition.rotation;
			}
			PlayerDistributer.GetPlayer(uid).SendMessage("TeleportTo", teleportPosition, SendMessageOptions.DontRequireReceiver);
		}

		private void OnDisable()
		{
			if (setAsParent)
			{
				GameObject player = PlayerDistributer.GetPlayer(SystemInfo.deviceUniqueIdentifier);
				player.transform.parent = null;
				DontDestroyOnLoad(player.gameObject);
			}

			PlayerDistributer.UnregisterSpawnPosition(this);
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = new Color(0.18f, 0.96f, 0.04f, 0.35f);
			Gizmos.matrix = this.transform.localToWorldMatrix;
			Gizmos.DrawCube(new Vector3(0, 0.8f, 0), new Vector3(0.4f, 1.6f, 0.4f));
		}

		private void Reset()
		{
			CreatePanelIfNull("Front Left", transform, new Vector3(-1.25f, 1.5f, 1.5f), Quaternion.Euler(0, -30, 0));
			CreatePanelIfNull("Front Center", transform, new Vector3(0, 1.5f, 2f));
			CreatePanelIfNull("Front Right", transform, new Vector3(1.25f, 1.5f, 1.5f), Quaternion.Euler(0, 30, 0));

			if (!transform.GetChild("Head Look At"))
			{
				GameObject newGameObject = new("Head Look At")
				{
					transform = { parent = transform, }
				};
				newGameObject.AddComponent<PlayerHeadLookAt>();
			}

#if UNITY_EDITOR
			PropManager localPropManager = FindObjectOfType<PropManager>();
				localPropManager.FindAndAddAllProps();
#endif
		}

		private void CreatePanelIfNull(string panelName, Transform parent, Vector3 position, Quaternion rotation = new())
		{
			if (parent.GetChild(panelName))
			{
				return;
			}

			GameObject newGameObject = new(panelName)
			{
				transform =
				{
					parent = parent,
					position = position,
					rotation = rotation
				}
			};
			PropComponent propComponent = newGameObject.AddComponent<PropComponent>();
			propComponent.propType = new PanelProp(propComponent);
			propComponent.name = newGameObject.name;
			propComponent.PropName = "Relative To Player/" + newGameObject.name;
		}
	}
}