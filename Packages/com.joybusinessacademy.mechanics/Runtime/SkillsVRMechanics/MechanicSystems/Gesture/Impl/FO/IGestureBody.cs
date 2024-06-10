using System.Collections.Generic;
using UnityEngine;

namespace VRMechanics.Mechanics.GestureDetection
{
    public enum GestureBodyPartType
    {
        None = 0,
        Anchor = 1,
        Head = 4,
        LeftHand = 5,
        RightHand = 6,
    }

    public enum GestureBodySpace
    {
        None = 0,
        Anchor = 1,
        Head = 4,
        LeftHand = 5,
        RightHand = 6,
        Body = 999,
    }

    public interface IGestureBody
    {
        string GetId();
        int BodyPartsCount();
        bool Contains(string id);
        IGestureBodyPart GetBodyPart(string id);
        Vector3 GetBodyPartPosition(string id);
        Quaternion GetBodyPartRotation(string id);
        IEnumerable<IGestureBodyPart> GetBodyParts();

        IGestureBodyPart GetRawBodyPart(string id);
    }
}