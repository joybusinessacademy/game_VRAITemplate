using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRMechanics
{
    public static class AddressableUtils
    {
        [Serializable]
        public class RuntimeAddressableEntryInfo
        {
            public string guid;
            public string assetPath;
            public string address;
            public HashSet<string> lables = new HashSet<string>();
        }

        [Serializable]
        public class RuntimeAddressableGroupInfo
        {
            public string name;
            public string guid;
            public List<RuntimeAddressableEntryInfo> entries = new List<RuntimeAddressableEntryInfo>();
        }

        [Serializable]
        public class RuntimeAddressabeSettingInfo
        {
            public readonly List<string> labels = new List<string>();
            public readonly List<RuntimeAddressableGroupInfo> groups = new List<RuntimeAddressableGroupInfo>();
        }
        public readonly static RuntimeAddressabeSettingInfo cachedEditorAddressableSettingInfo = new RuntimeAddressabeSettingInfo();
    }

}