using System;
using UnityEngine;

namespace VRMechanics.Mechanics.GestureDetection
{
	[Serializable]
    public class RuntimeGestureBodyPart : IGestureBodyPart
    {
        [StringEnumValueDropdown(enableCustomValue = true, enumType = typeof(GestureBodyPartType))]
        public string id;
        public Transform transform;

        public IGestureBodyPart Clone()
        {
            return new RuntimeGestureBodyPart() { id = this.id, transform = this.transform };
        }

        public string GetId()
        {
            return id;
        }

        public Vector3 GetPosition()
        {
            return null == transform ? Vector3.zero : transform.position;
        }

        public Quaternion GetRotation()
        {
            return null == transform ? Quaternion.identity : transform.rotation;
        }
    }
}