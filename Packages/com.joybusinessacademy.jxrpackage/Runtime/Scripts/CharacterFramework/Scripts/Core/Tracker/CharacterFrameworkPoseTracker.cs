using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using System;

namespace JBA.CharacterFramework.Tracker
{
    public class CharacterFrameworkPoseTracker : MonoBehaviour
    {
        public const string MSGID_REQUEST_RESET_OFFSET_ZERO_POINT_WITH_TARGET_POS = "MSGID_REQUEST_RESET_OFFSET_ZERO_POINT_WITH_TARGET_POS";
        [SerializeField]
        private string initBindingFrameworkId;
        
        public string bindFrameId { get; private set; }

        public string fullBindFrameId => CharacterFrameworkModule.GetFrameId(bindFrameId);

        private Dictionary<string, CharacterFrameworkNodeTracker> trackingNodes = new Dictionary<string, CharacterFrameworkNodeTracker>();

        private Transform offsetZeroPoint;

        private XRRig xrRig;

        private bool canResetZeroPointByRecenter = true;

        private Vector3 lastRecenterTargetPos = Vector3.zero;

        private void Awake()
        {
            xrRig = GetComponent<XRRig>();
            if (null == xrRig)
            {
                Debug.LogError("CharacterFrameworkPoseTracker Need XRRig at same game object.");
            }
            ResetFrameNodesFromChildren();
            GameObject offsetZeroPointObj = new GameObject(nameof(offsetZeroPoint));
            offsetZeroPointObj.transform.parent = this.gameObject.transform;
            offsetZeroPoint = offsetZeroPointObj.transform;
            lastRecenterTargetPos = offsetZeroPoint.TransformPoint(Vector3.forward);
            SetOffsetZeroPointWithTargetPoint(lastRecenterTargetPos);

            RegisterRecenterEvents();
            InputDevices.deviceConnected += OnDeviceConnected;
            InputDevices.deviceDisconnected += OnDeviceDisconnected;


            //TODO: New Way to Message between Packages
            //GlobalMessenger.Instance?.AddListener<Vector3>(MSGID_REQUEST_RESET_OFFSET_ZERO_POINT_WITH_TARGET_POS, SetOffsetZeroPointWithTargetPoint);
        }

        private void OnDestroy()
        {
            InputDevices.deviceConnected -= OnDeviceConnected;
            InputDevices.deviceDisconnected -= OnDeviceDisconnected;

            UnRegisterRecenterEvents();
            //TODO: New Way to Message between Packages
            //GlobalMessenger.Instance?.RemoveListener<Vector3>(MSGID_REQUEST_RESET_OFFSET_ZERO_POINT_WITH_TARGET_POS, SetOffsetZeroPointWithTargetPoint);
        }

        private void Start()
        {
            SetBindingFramework(initBindingFrameworkId);
        }
        
        public void SetBindingFramework(string frameworkId)
        {
            bindFrameId = frameworkId;
            if (!string.IsNullOrWhiteSpace(fullBindFrameId))
            {
                CharacterFrameworkModule.inst.CreateFramework(fullBindFrameId);
                foreach (var node in trackingNodes.Values)
                {
                    if (null == node || string.IsNullOrWhiteSpace(node.id))
                    {
                        continue;
                    }
                    CharacterFrameworkModule.inst.CreateFrameworkNode(fullBindFrameId, node.id);
                }
            }
        }

        private void Update()
        {
            ResetFrameNodesFromChildren();
            WriteToFramework(fullBindFrameId);
            TrackingRecenter();
        }

        private void TrackingRecenter()
        {
#if UNITY_ANDROID
            // Note: 
            // inputDevice.subsystem.trackingOriginUpdated not fired with holding right menu button
            // inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.menuButton, out pressed) always false on right menu
            Vector3 camRigPos = xrRig.cameraInRigSpacePos;
            if (0.01f > Mathf.Abs(camRigPos.x) && 0.01f > Mathf.Abs(camRigPos.z))
            {
                if (canResetZeroPointByRecenter)
                {
                    canResetZeroPointByRecenter = false;
                    Debug.Log("Reset zero point by recenter. " + this.gameObject.name);
                    StartCoroutine(DelaySetOffsetZeroPointWithLastTargetPoint());
                }
            }
            else if (!canResetZeroPointByRecenter)
            {
                canResetZeroPointByRecenter = true;
            }
#endif
        }

        private IEnumerator DelaySetOffsetZeroPointWithLastTargetPoint()
        {
            yield return null;
            SetOffsetZeroPointWithTargetPoint(lastRecenterTargetPos);
            yield return null;
            SetOffsetZeroPointWithTargetPoint(lastRecenterTargetPos);
        }

        private void RegisterRecenterEvents()
        {
            List<InputDevice> devices = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Camera, devices);
            foreach (var device in devices)
            {
                device.subsystem.trackingOriginUpdated += OnRecenter;
            }
        }
        private void UnRegisterRecenterEvents()
        {
            List<InputDevice> devices = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Camera, devices);
            foreach (var device in devices)
            {
                device.subsystem.trackingOriginUpdated -= OnRecenter;
            }
        }

        private void OnRecenter(XRInputSubsystem obj)
        {
            SetOffsetZeroPointWithTargetPoint(lastRecenterTargetPos);
        }

        private void OnDeviceConnected(InputDevice obj)
        {
            if ((obj.characteristics & InputDeviceCharacteristics.Camera) == InputDeviceCharacteristics.Camera)
            {
                obj.subsystem.trackingOriginUpdated += OnRecenter;
            }
        }

        private void OnDeviceDisconnected(InputDevice obj)
        {
            if (null != obj && null != obj.subsystem)
            {
                obj.subsystem.trackingOriginUpdated -= OnRecenter;
            }
        }
        
        public void SetOffsetZeroPointWithTargetPoint(Vector3 targetPos)
        {
            ResetZeroPoint();

            string rootNodeId = CharacterFrameworkNodeTypeEnum.RootOnGround.ToString();
            Vector3 pos;
            Vector3 angle;
            if (!CharacterFrameworkModule.inst.TryGetNodeTransform(fullBindFrameId, rootNodeId, out pos, out angle))
            {
                Debug.LogErrorFormat("ResetZeroPoint fail: Cannot get root point from node {0}.{1}", fullBindFrameId, rootNodeId);
                return;
            }
            lastRecenterTargetPos = targetPos;
            SetOffsetZeroPoint(pos, angle);
            
            Vector3 dir = targetPos - this.transform.TransformPoint(pos);
            
            Quaternion rotation = Quaternion.LookRotation(dir, Vector3.up);
            Vector3 offsetEuler = offsetZeroPoint.eulerAngles;
            offsetEuler.y = rotation.eulerAngles.y;
            offsetZeroPoint.eulerAngles = offsetEuler;
            /*
            Debug.LogFormat("Root ground {0}  rot{1}", pos, angle);
            Debug.LogFormat("dir {0}\r\ntarget: {1}\r\ntracker pos: {2}\r\noffsetZPt {3}\r\nrot angle {4}\r\nlo to w pos {5}", 
                dir, 
                targetPos, 
                this.transform.position, 
                offsetZeroPoint.transform.position
                , rotation.eulerAngles,
                transform.TransformPoint(pos));
                */
            Update();
        }
        
        public void AutoSetOffsetZeroPoint()
        {
            ResetZeroPoint();

            string rootNodeId = CharacterFrameworkNodeTypeEnum.RootOnGround.ToString();
            Vector3 pos;
            Vector3 angle;
            if (!CharacterFrameworkModule.inst.TryGetNodeTransform(fullBindFrameId, rootNodeId, out pos, out angle))
            {
                Debug.LogErrorFormat("ResetZeroPoint fail: Cannot get root point from node {0}.{1}", fullBindFrameId, rootNodeId);
                return;
            }

            SetOffsetZeroPoint(pos, angle);
        }

        public void SetOffsetZeroPoint(Vector3 pos, Vector3 angle)
        {
            offsetZeroPoint.localPosition = pos;
            offsetZeroPoint.localRotation = Quaternion.Euler(angle);
            Update();
        }

        private void ResetZeroPoint()
        {
            if (null == offsetZeroPoint)
            {
                return;
            }
            offsetZeroPoint.localPosition = Vector3.zero;
            offsetZeroPoint.localRotation = new Quaternion();
            offsetZeroPoint.localScale = Vector3.one;
            Update();
        }

        private readonly string teleportAnchorEnumString = CharacterFrameworkNodeTypeEnum.TeleportAnchor.ToString();

        public void WriteToFramework(string frameworkId)
        {
            UnityEngine.XR.TrackingOriginModeFlags mode = UnityEngine.XR.TrackingOriginModeFlags.Floor;

            if (null != xrRig)
            {
                mode = xrRig.CurrentTrackingOriginMode;
            }
            Transform rootTransform = (null == offsetZeroPoint ? this.transform : offsetZeroPoint);
            string anchorId = teleportAnchorEnumString;
            foreach (var node in trackingNodes.Values)
            {
                if (null == node || string.IsNullOrWhiteSpace(node.id))
                {
                    continue;
                }

                if (node.id == anchorId)
                {
                    CharacterFrameworkModule.inst.SetNodeTransform(frameworkId, node.id, Vector3.zero, Vector3.zero);
                    continue;
                }

                var position = rootTransform.InverseTransformPoint(node.transform.position);
                switch (node.nodeType)
                {
                    case CharacterFrameworkNodeTypeEnum.RootOnGround:
                        break;
                    case CharacterFrameworkNodeTypeEnum.TeleportAnchor:
                        break;
                    default:
                        position.y += !Application.isEditor && mode == UnityEngine.XR.TrackingOriginModeFlags.Device ? xrRig.cameraYOffset : 0.0f;
                        break;
                }
                var rotation = (Quaternion.Inverse(rootTransform.rotation) * node.transform.rotation).eulerAngles;
                CharacterFrameworkModule.inst.SetNodeTransform(frameworkId, node.id, position, rotation);
            }
        }
        
        public void ResetFrameNodesFromChildren()
        {
            trackingNodes.Clear();
            var nodes = gameObject.GetComponentsInChildren<CharacterFrameworkNodeTracker>();
            foreach (var node in nodes)
            {
                if (null == node || string.IsNullOrWhiteSpace(node.id))
                {
                    continue;
                }
                if (this != (CharacterFrameworkPoseTracker)node.GetComponentInParent<CharacterFrameworkPoseTracker>())
                {
                    continue;
                }

                if (trackingNodes.ContainsKey(node.id))
                {
                    //Debug.LogErrorFormat("CollectFrameNodesInChildren Error: Node \"{0}\" already added. Frame Obj: {3}. Existing Obj: {1}. Conflict Obj: {2}", node.id, trackingNodes[node.id].gameObject.name, node.gameObject.name, this.gameObject.name);
                }
                else
                {
                    trackingNodes.Add(node.id, node);
                }
            }
        }
    }
}