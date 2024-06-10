using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JBA.CharacterFramework
{
    public enum CharacterFrameworkNodeTypeEnum
    {
        None = 0,
        TeleportAnchor = 1,
        RootOnGround = 2,
        Root = 3,
        Head = 4,
        LeftHand = 5,
        RightHand = 6,
        LeftEye = 7,
        RightEye = 8,
    }

    [System.Serializable]
    public class CharacterFrameworkNode
    {
        public string id;
        public Vector3 position = Vector3.zero;
        public Vector3 rotation = Vector3.zero;

        public CharacterFrameworkNode(string xID)
        {
            id = xID;
        }
        public CharacterFrameworkNode(string xID, Vector3 xPos, Vector3 xAngle)
        {
            id = xID;
            position = xPos;
            rotation = xAngle;
        }

        public CharacterFrameworkNode Clone()
        {
            CharacterFrameworkNode clone = new CharacterFrameworkNode(this.id);
            clone.position = this.position;
            clone.rotation = this.rotation;
            return clone;
        }
    }

    [System.Serializable]
    public class CharacterFrameworkRecognitionStep
    {
        public enum DetectionTypeEnum
        {
            NONE,
            LOCAL_POSITION,
            LOCAL_ROTATION,
            RELATIVE_POSITION_TO,
            RELATIVE_ROTATION_TO,
        }

        public string id;
        public string name;
        public DetectionTypeEnum detectionType = DetectionTypeEnum.NONE;
        public string mainNodeId;
        public string additionNodeId;
        public float range = 0.0f;

        public CharacterFrameworkRecognitionStep(string xId, string xName, DetectionTypeEnum xType, float xRange, string xMainNodeId, string xAdditionNodeId = null)
        {
            id = xId;
            name = xName;
            detectionType = xType;
            mainNodeId = xMainNodeId;
            additionNodeId = xAdditionNodeId;
            range = xRange;
        }

        public CharacterFrameworkRecognitionStep Clone()
        {
            CharacterFrameworkRecognitionStep clone = new CharacterFrameworkRecognitionStep(id, name, detectionType, range, mainNodeId, additionNodeId);
            return clone;
        }
    }

    [System.Serializable]
    public class CharacterFramework
    {
        public string id;

        public List<CharacterFrameworkNode> nodes = new List<CharacterFrameworkNode>();

        public List<CharacterFrameworkRecognitionStep> steps = new List<CharacterFrameworkRecognitionStep>();

        public void SetDefaultRecognizeSteps()
        {
            steps.Clear();
            steps.Add(new CharacterFrameworkRecognitionStep("HeadPosDir", "HeadDir", CharacterFrameworkRecognitionStep.DetectionTypeEnum.RELATIVE_ROTATION_TO, 45.0f,
                CharacterFrameworkNodeTypeEnum.Head.ToString(), CharacterFrameworkNodeTypeEnum.TeleportAnchor.ToString()));
            steps.Add(new CharacterFrameworkRecognitionStep("LeftHandPosDir", "LeftHandDir", CharacterFrameworkRecognitionStep.DetectionTypeEnum.RELATIVE_ROTATION_TO, 30.0f,
                CharacterFrameworkNodeTypeEnum.LeftHand.ToString(), CharacterFrameworkNodeTypeEnum.Head.ToString()));
            steps.Add(new CharacterFrameworkRecognitionStep("RightHandPosDir", "RightHandDir", CharacterFrameworkRecognitionStep.DetectionTypeEnum.RELATIVE_ROTATION_TO, 30.0f,
                CharacterFrameworkNodeTypeEnum.RightHand.ToString(), CharacterFrameworkNodeTypeEnum.Head.ToString()));

            steps.Add(new CharacterFrameworkRecognitionStep("HeadRotation", "HeadRotation", CharacterFrameworkRecognitionStep.DetectionTypeEnum.LOCAL_ROTATION, 60.0f,
                CharacterFrameworkNodeTypeEnum.Head.ToString()));
            steps.Add(new CharacterFrameworkRecognitionStep("LeftHandRotation", "LeftHandRotation", CharacterFrameworkRecognitionStep.DetectionTypeEnum.LOCAL_ROTATION, 45.0f,
                CharacterFrameworkNodeTypeEnum.LeftHand.ToString()));
            steps.Add(new CharacterFrameworkRecognitionStep("RightHandRotation", "RightHandRotation", CharacterFrameworkRecognitionStep.DetectionTypeEnum.LOCAL_ROTATION, 45.0f,
                CharacterFrameworkNodeTypeEnum.RightHand.ToString()));
        }

        public CharacterFrameworkRecognitionStep GetStep(string id)
        {
            return steps.Find(x => null != x && !string.IsNullOrEmpty(id) && id == x.id);
        }

        public List<string> GetAllStepIds()
        {
            List<string> nodeIds = new List<string>();

            foreach (var node in steps)
            {
                if (null == node || string.IsNullOrWhiteSpace(node.id))
                {
                    continue;
                }
                nodeIds.Add(node.id);
            }
            return nodeIds;
        }

        public CharacterFramework(string xID)
        {
            id = xID;
            if (0 == steps.Count)
            {
                SetDefaultRecognizeSteps();
            }
        }

        public CharacterFrameworkNode GetNode(string id)
        {
            foreach (var node in nodes)
            {
                if (null != node && node.id == id)
                {
                    return node;
                }
            }
            return null;
        }

        public List<string> GetAllNodeIds()
        {
            List<string> nodeIds = new List<string>();

            foreach (var node in nodes)
            {
                if (null == node || string.IsNullOrWhiteSpace(node.id))
                {
                    continue;
                }
                nodeIds.Add(node.id);
            }
            return nodeIds;
        }

        public bool AddNode(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }
            if (null != GetNode(id))
            {
                return true;
            }
            nodes.Add(new CharacterFrameworkNode(id));
            return true;
        }

        public bool RemoveNode(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }
            var node = GetNode(id);
            if (null == node)
            {
                return true;
            }
            return nodes.Remove(node);
        }

        public bool CopyFrom(CharacterFramework source)
        {
            if (null == source)
            {
                return false;
            }
            nodes.Clear();
            nodes.Capacity = source.nodes.Count;
            foreach (var node in source.nodes)
            {
                if (null == node)
                {
                    continue;
                }
                nodes.Add(node.Clone());
            }

            steps.Clear();
            steps.Capacity = source.steps.Count;
            foreach (var step in source.steps)
            {
                if (null == step)
                {
                    continue;
                }
                steps.Add(step.Clone());
            }
            return true;
        }
    }
}
