using UnityEngine;
using System;

namespace JBA.CharacterFramework.Display
{
    public class CharacterFrameworkViewController : MonoBehaviour
    {
        [SerializeField]
        private string initBindingFrameworkId;

        [SerializeField]
        private bool initBindingAtEnable = false;
        
        public string bindFrameId { get; private set; }

        [Serializable]
        public class UnityEvent_String : UnityEngine.Events.UnityEvent<string> { }

        public UnityEvent_String OnBindFramework = new UnityEvent_String();

        public string fullBindFrameId => CharacterFrameworkModule.GetFrameId(bindFrameId);
        public enum ScaleTypeEnum
        {
            ORIGINAL,
            SCALE,
            SCALE_TO_HEIGHT,
        }

        public ScaleTypeEnum scaleType = ScaleTypeEnum.ORIGINAL;
        public float scale = 1.0f;
        public float scaleToHeight = 1.7f;



        public delegate void NodeUpdateDelegate(string xNodeId, Vector3 xPosition, Quaternion xRotation);

        public NodeUpdateDelegate OnNodeUpdate;

        private void Awake()
        {
            SetBindingFramework(initBindingFrameworkId);
        }

        private void OnEnable()
        {
            if (initBindingAtEnable)
            {
                SetBindingFramework(initBindingFrameworkId);
            }
        }
        
        public void SetBindingFramework(string frameworkId)
        {
            bindFrameId = frameworkId;
            OnBindFramework?.Invoke(fullBindFrameId);
        }
        
        private void Update()
        {
            ReadFromFrameworkData(fullBindFrameId);
        }

        private void ReadFromFrameworkData(string frameworkId)
        {
            if (!CharacterFrameworkModule.inst.ContainsFramework(frameworkId))
            {
                return;
            }
            float currScale = 1.0f;
            switch (scaleType)
            {
                case ScaleTypeEnum.ORIGINAL:
                    currScale = 1.0f;
                    break;
                case ScaleTypeEnum.SCALE:
                    currScale = scale;
                    break;
                case ScaleTypeEnum.SCALE_TO_HEIGHT:
                    {
                        if (CharacterFrameworkModule.inst.TryGetNodePosition(frameworkId, CharacterFrameworkNodeTypeEnum.Head.ToString(), out Vector3 headPos))
                        {
                            if (Math.Abs(headPos.y) > 0.001f)
                            {
                                currScale = scaleToHeight / headPos.y;
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
            foreach (string nodeId in CharacterFrameworkModule.inst.GetFrameworkNodeIds(frameworkId))
            {
                if (CharacterFrameworkModule.inst.TryGetNodeTransform(frameworkId, nodeId, out Vector3 pos, out Vector3 rot))
                {
                    Vector3 nodePos = transform.TransformPoint(pos * currScale);
                    Quaternion nodeRot = Quaternion.Euler(transform.rotation.eulerAngles + rot);
                    OnNodeUpdate?.Invoke(nodeId, nodePos, nodeRot);
                }
            }
        }
    }
}