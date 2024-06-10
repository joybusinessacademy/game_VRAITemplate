using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace JBA.CharacterFramework.Tracker
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class RootPoseUpdater : MonoBehaviour
    {
        public Transform anchorTramsform;
        public Transform headTramsform;

        public bool rotateWithHead = false;

        // Update is called once per frame
        void Update()
        {
            if (null != anchorTramsform && null != headTramsform)
            {
                var pos = anchorTramsform.InverseTransformPoint(headTramsform.position);
                pos.y = 0.0f;
                transform.position = anchorTramsform.TransformPoint(pos);


                if (rotateWithHead)
                {
                    transform.rotation = anchorTramsform.rotation;
                    var localRotation = Quaternion.Inverse(anchorTramsform.rotation) * headTramsform.rotation;
                    transform.Rotate(0.0f, localRotation.eulerAngles.y, 0.0f, Space.Self);
                }

            }
        }
    }
}