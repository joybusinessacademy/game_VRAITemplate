using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRMechanics;
using System.Linq;

namespace VRMechanics.Mechanics.GestureDetection
{
    [Serializable]
    public class GestureDetectionData : AbstractMechanicData<GestureDetectionData>
    {
        public const string searchingLabel = "GestureTemplate";

        [Tooltip("Addressable key for gesture template asset.")]
        [ValueDropdown(nameof(GetGestureTemplateKeys))]
        public string gestureTemplateAssetKey;

        [Tooltip("Real gesture asset that downloaded from spawner.")]
        [HideInInspector]
        public TextAsset gestureTemplateAsset;

        // same as GestureBodyPartType
        public enum RuntimePoseType
        {
            None = 0,
            Anchor = 1,
            Head = 4,
            LeftHand = 5,
            RightHand = 6,
        }

        [Serializable]
        public class RuntimePoseData
        {
            [StringEnumValueDropdown(enumType = typeof(RuntimePoseType), enableCustomValue = true)]
            [Tooltip("Gesture body part id. At least include Anchor and Head, better to have all.")]
            public string type;
            public Transform transform;
        }

        [Serializable]
        public class RuntimePose
        {
            public List<RuntimePoseData> poseDataList = new List<RuntimePoseData>();
        }

        [Tooltip("The gesture reference (like XRPlayer) that trigger breath in runtime. Transform data could from vr headset or dynamic game objects.")]
        public RuntimePose runtimeGestureRef = new RuntimePose();

        [Tooltip("[Optional] The visualizer object for breath gesture template. Null for no visual effect.")]
        public GameObject gestureTemplateVisualObject;
        [Tooltip("[Optional]The visualizer object for runtime gesture (like XRPlayer). Null for no visual effect.")]
        public GameObject runtimeGestureVisualObject;


        private IEnumerable<string> GetGestureTemplateKeys()
        {
#if UNITY_EDITOR
            return AddressableUtils.cachedEditorAddressableSettingInfo.groups
                .SelectMany(x => x.entries)
                .Where(x => x.lables.Contains(searchingLabel))
                .Select(x => x.address);
#endif
            return new List<string>();
        }

    }
}
