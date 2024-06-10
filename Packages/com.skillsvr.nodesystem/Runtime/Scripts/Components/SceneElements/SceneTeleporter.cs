using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace SkillsVRNodes
{
    public class SceneTeleporter : SceneElement
    {      
        public override void Reset()
        {
            base.Reset();
            GetComponent<BoxCollider>().isTrigger = true;
            teleporterGameObject = gameObject;
		}

        public Action onComplete;

        public GameObject teleporterGameObject;

		private Camera targetCamera;
		private Canvas targetCanvas;

		private GameObject visualGameobject;

		public void Awake()
        {
            if (teleporterGameObject)
            {
                SetUpInteractionDetection();

				return;
            }
            
            GameObject prefabToClone =  Resources.Load<GameObject>("Custom Assets/teleporter");

            if (!prefabToClone)
            {
                prefabToClone =  Resources.Load<GameObject>("Default Assets/teleporter");
            }

            teleporterGameObject = Instantiate(prefabToClone, transform);
            teleporterGameObject.SetActive(false);
        }

        public void SendSetupMessage()
        {
            teleporterGameObject.SendMessage("Setup", SendMessageOptions.DontRequireReceiver);
        }
        
        public void SendTeleportMessage()
        {
			PlayerDistributer.GetPlayer(SystemInfo.deviceUniqueIdentifier).SendMessage("TeleportTo", teleporterGameObject.transform.position, SendMessageOptions.DontRequireReceiver);
			teleporterGameObject.SendMessage("Teleport", SendMessageOptions.DontRequireReceiver);
        }

        public void SendEnableTeleportMessage()
        {
            teleporterGameObject.SendMessage("EnableTeleport", SendMessageOptions.DontRequireReceiver);
			visualGameobject.SetActive(true);
		}

        public void SendDisableTeleportMessage()
        {
            teleporterGameObject.SendMessage("DisableTeleport", SendMessageOptions.DontRequireReceiver);
			visualGameobject.SetActive(false);
			teleporterGameObject.SetActive(false);
		}

        public void AttachListener(Action action)
        {
            onComplete += action;
        }
        
        public void Complete()
        {
            onComplete.Invoke();
        }

        public void SetUpInteractionDetection()
        {
			GameObject canvasObject = new GameObject("WorldSpaceCanvas");

			visualGameobject = canvasObject;

			canvasObject.transform.parent = teleporterGameObject.transform;
			canvasObject.transform.localPosition = Vector3.zero;
			canvasObject.transform.rotation = Quaternion.identity;

			Canvas canvas = canvasObject.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.WorldSpace;

            canvasObject.AddComponent<GraphicRaycaster>();
            canvasObject.AddComponent<TrackedDeviceGraphicRaycaster>();

			RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();
			canvasRectTransform.sizeDelta = new Vector2(1, 2);

			GameObject newButtonObject = new GameObject("NewButton");
			newButtonObject.transform.SetParent(canvas.gameObject.transform, false);

			Button newButton = newButtonObject.AddComponent<Button>();
			newButtonObject.transform.localPosition = new Vector3(0f, 0f, 0f);
            newButton.onClick.AddListener(OnClickAction);

			Image image = newButtonObject.AddComponent<Image>();
			image.color = Color.clear;

			RectTransform imageRectTransform = image.GetComponent<RectTransform>();
			imageRectTransform.anchorMin = Vector2.zero;
			imageRectTransform.anchorMax = Vector2.one;
			imageRectTransform.sizeDelta = Vector2.zero;

            targetCanvas = canvas;

			visualGameobject.SetActive(false);
		}

		private void OnClickAction()
		{
            SendTeleportMessage();
            SendDisableTeleportMessage();

			visualGameobject.SetActive(false);
		}

		private void Start()
		{
            SetupCamera();
		}

		public void SetupCamera()
		{
			//if (null == targetCamera)
			//{
			//	var xrRig = GameObject.FindObjectOfType<XROrigin>();
			//	targetCamera = null != xrRig ? (null != xrRig.Camera ? xrRig.Camera : Camera.main) : Camera.main;

			//	targetCanvas.worldCamera = targetCamera;
			//}
		}

		private void Update()
		{
			FaceCamera();
		}

		private void FaceCamera()
		{
			if (targetCamera == null)
				return;

			Vector3 directionToCamera = targetCamera.transform.position - transform.position;
			transform.forward = directionToCamera.normalized;
		}
	}
}
