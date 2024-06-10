using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JBA.CharacterFramework.Display
{
    public class ModelScaler : MonoBehaviour
    {
        public Transform moduleRoot;
        public Transform moduleHead;

        public float offsetHeight;

        float height = 1.0f;
        Vector3 originModuleScale = Vector3.one;

        public Transform sourceRoot;
        public Transform sourceHead;

        private void Awake()
        {
            height = GetOffsetHeight(moduleRoot, moduleHead);
            if (null != moduleRoot)
            {
                originModuleScale = moduleRoot.localScale;
            }
        }

        float GetOffsetHeight(Transform root, Transform head)
        {
            if (null == root || null == head)
            {
                return 1.0f;
            }
            return root.InverseTransformPoint(head.position).y;
        }

        void Update()
        {
            if (null != sourceRoot && null != sourceHead && null != moduleRoot)
            {
                float sourceHeight = GetOffsetHeight(sourceRoot, sourceHead);
                moduleRoot.localScale = originModuleScale * sourceHeight / (height + offsetHeight);
            }
            else if (null != moduleRoot)
            {
                moduleRoot.localScale = originModuleScale;
            }
        }
    }
}