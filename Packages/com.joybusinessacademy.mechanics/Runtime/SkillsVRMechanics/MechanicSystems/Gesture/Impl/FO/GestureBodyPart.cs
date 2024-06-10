using System;
using UnityEngine;

namespace VRMechanics.Mechanics.GestureDetection
{
    [Serializable]
    public class GestureBodyPart : IGestureBodyPart
    {
        [StringEnumValueDropdown(enableCustomValue = true, enumType = typeof(GestureBodyPartType))]
        public string id;
        public Vector3 position = Vector3.zero;

        public Vector3 rotation = Vector3.zero;

        public GestureBodyPart() { }
        public GestureBodyPart(string bodyPartId)
        {
            id = bodyPartId;
        }
        public GestureBodyPart(GestureBodyPartType partType)
        {
            id = partType.ToString();
        }
        public GestureBodyPart(IGestureBodyPart other)
        {
            if (null != other)
            {
                this.id = other.GetId();
                this.position = other.GetPosition();
                this.rotation = other.GetRotation().eulerAngles;
            }
        }

        public IGestureBodyPart Clone()
        {
            return new GestureBodyPart(this);
        }

        public string GetId()
        {
            return id;
        }

        public Vector3 GetPosition()
        {
            return position;
        }

        public Quaternion GetRotation()
        {
            return Quaternion.Euler(rotation);
        }
    }
}