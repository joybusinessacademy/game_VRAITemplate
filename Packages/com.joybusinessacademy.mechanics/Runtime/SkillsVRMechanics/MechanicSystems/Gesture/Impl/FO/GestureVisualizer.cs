using UnityEngine;

namespace VRMechanics.Mechanics.GestureDetection
{
	public class GestureVisualizer
    {
        public void UpdateGestureToGameObject(IGestureBody gestureBody, GameObject rootObject)
        {
            if (null == gestureBody || null == rootObject)
            {
                return;
            }

            GameObject anchorObj = FindObjectInChildren(rootObject, nameof(GestureBodyPartType.Anchor));
            if (null != anchorObj)
            {
                anchorObj.transform.localPosition = Vector3.zero;
                anchorObj.transform.localRotation = Quaternion.identity;
            }
            foreach (var part in gestureBody.GetBodyParts())
            {
                string id = part.GetId();
                if (id == nameof(GestureBodyPartType.Anchor))
                {
                    continue;
                }

                GameObject target = FindObjectInChildren(rootObject, id);
                if (null == target)
                {
                    continue;
                }
                target.transform.localPosition = part.GetPosition();
                target.transform.localRotation = part.GetRotation();
            }
        }

        protected GameObject FindObjectInChildren(GameObject root, string name)
        {
            if (null == root)
            {
                return null;
            }
            if (root.name == name)
            {
                return root;
            }
            foreach(Transform child in root.transform)
            {
                var output = FindObjectInChildren(child.gameObject, name);
                if (null != output)
                {
                    return output;
                }
            }
            return null;
        }
    }
}