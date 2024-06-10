using System;
using System.Collections.Generic;
using UnityEngine;

namespace VRMechanics.Mechanics.GestureDetection
{

    [Serializable]
    public class GestureBody : IGestureBody
    {
        public string id;

        [SerializeField]
        public List<GestureBodyPart> bodyParts = new List<GestureBodyPart>();

        public int BodyPartsCount()
        {
            return bodyParts.Count;
        }

        public bool Contains(string id)
        {
            return null != GetBodyPart(id);
        }

        public IGestureBodyPart GetBodyPart(string id)
        {
            return bodyParts.Find(x => null != x && !string.IsNullOrWhiteSpace(x.id) && x.id == id);
        }

        public Vector3 GetBodyPartPosition(string id)
        {
            var bodyPart = GetBodyPart(id);
            return null == bodyPart ? Vector3.zero : bodyPart.GetPosition();
        }

        public Quaternion GetBodyPartRotation(string id)
        {
            var bodyPart = GetBodyPart(id);
            return null == bodyPart ? Quaternion.identity : bodyPart.GetRotation();
        }

        public IEnumerable<IGestureBodyPart> GetBodyParts()
        {
            return bodyParts;
        }

        public string GetId()
        {
            return string.IsNullOrWhiteSpace(id) ? "" : id;
        }

        public IGestureBodyPart GetRawBodyPart(string id)
        {
            return GetBodyPart(id);
        }

        public void SetBodyPartPosition(string id, Vector3 pos)
        {
            var bodyPart = GetBodyPart(id) as GestureBodyPart;
            if (null == bodyPart)
            {
                return;
            }
            bodyPart.position = pos;
        }

        public void SetBodyPartRotation(string id, Quaternion rotation)
        {
            var bodyPart = GetBodyPart(id) as GestureBodyPart;
            if (null == bodyPart)
            {
                return;
            }
            bodyPart.rotation = rotation.eulerAngles;
        }
    }
}