using SkillsVR.OdinPlaceholder; 
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VRMechanics.Mechanics.GestureDetection
{
	[Serializable]
    public class GestureTemplate
    {
        public string id => null == gesture ? "" : gesture.id;
        public GestureBody gesture;
        public GestureDetector detector;

        private void Awake()
        {
            if (null == gesture)
            {
                gesture = new GestureBody();
            }
            if (null == detector)
            {
                detector = new GestureDetector();
            }
        }

        public string ToJson(bool prettyPrint = true)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public bool FromJsonOverwrite(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }
            try
            {
                JsonUtility.FromJsonOverwrite(json, this);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool FromFileOverwrite(TextAsset textAsset)
        {
            if (null == textAsset)
            {
                return false;
            }
            return FromJsonOverwrite(textAsset.text);
        }

#if UNITY_EDITOR
        [Button]
        public void SaveAsTextAsset()
        {
            string dirPath = UnityEditor.EditorUtility.OpenFolderPanel("Save to Dir", "", "");
            if (string.IsNullOrWhiteSpace(dirPath))
            {
                return;
            }
            string fileName = "GestureTemplate" + (string.IsNullOrWhiteSpace(id) ? DateTime.Now.Millisecond.ToString() : id) + ".json";
            string path = Path.Combine(dirPath, fileName);
            Debug.Log(path);
            File.WriteAllText(path, ToJson());
        }
#endif
    }
}