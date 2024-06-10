using UnityEngine;

namespace VRMechanics.Mechanics.GestureDetection
{
	public interface IGestureBodyPart
    {
        string GetId();
        Vector3 GetPosition();
        Quaternion GetRotation();
        IGestureBodyPart Clone();
    }

    public static class IIGestureBodyPartExtensionMethods
    {
        public static Vector3 LocalToWorldPosition(this IGestureBodyPart root, Vector3 localPos)
        {
            return root.GetRotation() * localPos + root.GetPosition();
        }

        public static Vector3 WorldToLocalPosition(this IGestureBodyPart root, Vector3 worldPos)
        {
            return Quaternion.Inverse(root.GetRotation()) * (worldPos - root.GetPosition());
        }

        public static Vector3 LocalToWorldRotation(this IGestureBodyPart root, Vector3 localRotation)
        {
            return (root.GetRotation() * Quaternion.Euler(localRotation)).eulerAngles;
        }

        public static Vector3 WorldToLocalRotation(this IGestureBodyPart root, Vector3 worldRotation)
        {
            return (Quaternion.Inverse(root.GetRotation()) * Quaternion.Euler(worldRotation)).eulerAngles;
        }
    }
}