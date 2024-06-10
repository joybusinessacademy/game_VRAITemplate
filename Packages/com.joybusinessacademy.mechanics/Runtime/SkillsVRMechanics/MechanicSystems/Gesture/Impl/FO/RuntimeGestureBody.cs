using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VRMechanics.Mechanics.GestureDetection
{
    [Serializable]
	public class RuntimeGestureBody : IGestureBody
    {
        public string id => null == normalizedGestureBody ? "" : normalizedGestureBody.id;
        public IGestureBodyPart anchor => GetBodyPartRuntime(GestureBodyPartType.Anchor);

        [SerializeField]
        protected List<RuntimeGestureBodyPart> managedSkeletonNodes = new List<RuntimeGestureBodyPart>();

        [SerializeField]
        protected GestureBody normalizedGestureBody = new GestureBody();

        public void ClearRuntimeBodyParts()
        {
            managedSkeletonNodes.Clear();
        }

        public void AddRuntimeBodyPart(string newPartId, Transform newPartTransform)
        {
            if (null == newPartTransform || string.IsNullOrWhiteSpace(newPartId))
            {
                return;
            }
            managedSkeletonNodes.Add(new RuntimeGestureBodyPart() { id = newPartId, transform = newPartTransform });
        }

        public void UpdateNormalizedGestureBody()
        {
            InitBodyParts();
            WorldToAnchorLocalSpace();
            NormalizeHeight(1.7f);
        }

        protected void NormalizeHeight(float height)
        {
            var head = normalizedGestureBody.GetBodyPart(nameof(GestureBodyPartType.Head));
            if (null == head)
            {
                return;
            }
            var realHeight = Mathf.Abs(head.GetPosition().y);
            var scale = 0.0f == realHeight ? 0.0f : height / realHeight;
            foreach(var part in normalizedGestureBody.bodyParts)
            {
                part.position *= scale;
            }
        }

        protected void InitBodyParts()
        {
            normalizedGestureBody.bodyParts.Clear();
            managedSkeletonNodes.Where(x => Enum.GetNames(typeof(GestureBodyPartType)).Any(t => t == x.id)).ToList().ForEach(x =>
             normalizedGestureBody.bodyParts.Add(new GestureBodyPart(x)));
        }
        protected void WorldToAnchorLocalSpace()
        {
            var anchor = GetBodyPartRuntime(GestureBodyPartType.Anchor);
            if (null == anchor || null == anchor.transform)
            {
                return;
            }
            foreach (var part in managedSkeletonNodes)
            {
                if (part.GetId() == nameof(GestureBodyPartType.Anchor))
                {
                    continue;
                }
                var position = part.GetPosition();
                var rotation = part.GetRotation();
                position = anchor.transform.InverseTransformPoint(position);
                rotation = Quaternion.Inverse(anchor.transform.rotation) * rotation;

                normalizedGestureBody.SetBodyPartPosition(part.GetId(), position);
                normalizedGestureBody.SetBodyPartRotation(part.GetId(), rotation);
            }
        }

        public RuntimeGestureBodyPart GetBodyPartRuntime(GestureBodyPartType typeEnum)
        {
           return  managedSkeletonNodes.Find(x => null != x && x.id == typeEnum.ToString());
        }

        public int BodyPartsCount()
        {
            return normalizedGestureBody.BodyPartsCount();
        }

        public bool Contains(string id)
        {
            return normalizedGestureBody.Contains(id);
        }

        public IGestureBodyPart GetBodyPart(string id)
        {
            return normalizedGestureBody.GetBodyPart(id);
        }

        public Vector3 GetBodyPartPosition(string id)
        {
            return normalizedGestureBody.GetBodyPartPosition(id);
        }

        public Quaternion GetBodyPartRotation(string id)
        {
            return normalizedGestureBody.GetBodyPartRotation(id);
        }

        public IEnumerable<IGestureBodyPart> GetBodyParts()
        {
            return normalizedGestureBody.GetBodyParts();
        }

        public string GetId()
        {
            return normalizedGestureBody.GetId();
        }

        public IGestureBodyPart GetRawBodyPart(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }
            return managedSkeletonNodes.Find(x => null != x && x.id == id);
        }
    }
}