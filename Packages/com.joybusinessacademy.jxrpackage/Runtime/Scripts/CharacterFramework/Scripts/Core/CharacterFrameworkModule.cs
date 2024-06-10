using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace JBA.CharacterFramework
{ 
    public class CharacterFrameworkModule
    {
        public static string frameworkDir
        {
            get
            {
                return Application.persistentDataPath + "/CharacterFrameworks";
            }
        }
        public static string frameworkFileExtention
        {
            get
            {
                return "frm";
            }
        }
        public static string frameworkPreviewImageFileExtention
        {
            get
            {
                return "png";
            }
        }

        

        public static string GetFrameworkFilePath(string frameworkId)
        {
            return frameworkDir + "/" + frameworkId + "." + frameworkFileExtention;
        }


        public delegate string GetCustomFrameId(string inputFrameId);

        private static GetCustomFrameId CustomFrameIdOverride;

        public static void SetCustomFrameIdCallback(GetCustomFrameId callback)
        {
            CustomFrameIdOverride = callback;
        }

        public static string GetFrameId(string inputFrame)
        {
            if (null != CustomFrameIdOverride)
            {
                return CustomFrameIdOverride.Invoke(inputFrame);
            }
            return inputFrame;
        }

        public static CharacterFrameworkModule inst = new CharacterFrameworkModule();

        private Dictionary<string, CharacterFramework> data = new Dictionary<string, CharacterFramework>();

        public int Count => data.Count;

        public bool ContainsFramework(string frameworkId)
        {
            if (string.IsNullOrWhiteSpace(frameworkId))
            {
                return false;
            }
            return data.ContainsKey(frameworkId);
        }

        public bool ContainsFrameworkNode(string frameworkId, string nodeId)
        {
            CharacterFramework framework = null;
            if (data.TryGetValue(frameworkId, out framework))
            {
                return null != framework.nodes.Find(x => null != x && nodeId == x.id);
            }
            else
            {
                return false;
            }
        }

        public bool CreateFrameworkNode(string frameworkId, string nodeId)
        {
            if (CreateFramework(frameworkId))
            {
                var frame = GetFramework(frameworkId);
                return frame.AddNode(nodeId);
            }
            else
            {
                return false;
            }
        }

        public bool CreateFramework(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            if (data.ContainsKey(id))
            {
                return true;
            }
            CharacterFramework framework = new CharacterFramework(id);
            data.Add(id, framework);
            return true;
        }

        public bool CopyFramework(string sourceFrameworkId, string targetFrameworkId)
        {
            CharacterFramework source = null;
            if (!TryGetFramework(sourceFrameworkId, out source))
            {
                return false;
            }

            CharacterFramework target = null;
            if (!TryGetFramework(targetFrameworkId, out target))
            {
                return false;
            }

            return target.CopyFrom(source);
        }

        public bool RemoveFramework(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }
            return data.Remove(id);
        }

        public bool RemoveFrameworkNode(string frameworkId, string nodeId)
        {
            CharacterFramework framework = null;
            if (TryGetFramework(frameworkId, out framework))
            {
                return framework.RemoveNode(nodeId);
            }
            return true;
        }

        public bool SetStepRange(string frameworkId, string stepId, float aNewRange)
        {
            var step = GetFrameworkRecognizeStep(frameworkId, stepId);
            if (null == step)
            {
                return false;
            }
            step.range = aNewRange;
            return true;
        }
        public bool GetStepRange(string frameworkId, string stepId, out float xRange)
        {
            xRange = 0.0f;
            var step = GetFrameworkRecognizeStep(frameworkId, stepId);
            if (null == step)
            {
                return false;
            }
            xRange = step.range;
            return true;
        }

        public bool SetStepType(string frameworkId, string stepId, CharacterFrameworkRecognitionStep.DetectionTypeEnum aNewType)
        {
            var step = GetFrameworkRecognizeStep(frameworkId, stepId);
            if (null == step)
            {
                return false;
            }
            step.detectionType = aNewType;
            return true;
        }
        public bool GetStepType(string frameworkId, string stepId, out CharacterFrameworkRecognitionStep.DetectionTypeEnum xType)
        {
            xType = CharacterFrameworkRecognitionStep.DetectionTypeEnum.RELATIVE_ROTATION_TO;
            var step = GetFrameworkRecognizeStep(frameworkId, stepId);
            if (null == step)
            {
                return false;
            }
            xType = step.detectionType;
            return true;
        }

        public bool SetNodePosition(string frameworkId, string nodeId, Vector3 aNewPosition)
        {
            var node = GetFrameworkNode(frameworkId, nodeId);
            if (null == node)
            {
                return false;
            }
            node.position = aNewPosition;
            return true;
        }

        public bool SetNodeRotation(string frameworkId, string nodeId, Vector3 aNewRotationAngle)
        {
            var node = GetFrameworkNode(frameworkId, nodeId);
            if (null == node)
            {
                return false;
            }
            node.rotation = aNewRotationAngle;
            return true;
        }

        public bool SetNodeTransform(string frameworkId, string nodeId, Vector3 aNewPosition, Vector3 aNewRotationAngle)
        {
            var node = GetFrameworkNode(frameworkId, nodeId);
            if (null == node)
            {
                return false;
            }
            node.position = aNewPosition;
            node.rotation = aNewRotationAngle;
            return true;
        }

        public bool TryGetNodePosition(string frameworkId, string nodeId, out Vector3 aNewPosition)
        {
            var node = GetFrameworkNode(frameworkId, nodeId);
            if (null == node)
            {
                aNewPosition = Vector3.zero;
                return false;
            }
            aNewPosition = node.position;
            return true;
        }

        public bool TryGetNodeRotation(string frameworkId, string nodeId, out Vector3 aNewRotationAngle)
        {
            var node = GetFrameworkNode(frameworkId, nodeId);
            if (null == node)
            {
                aNewRotationAngle = Vector3.zero;
                return false;
            }
            aNewRotationAngle = node.rotation;
            return true;
        }

        public bool TryGetNodeTransform(string frameworkId, string nodeId, out Vector3 aNewPosition, out Vector3 aNewRotationAngle)
        {
            var node = GetFrameworkNode(frameworkId, nodeId);
            if (null == node)
            {
                aNewRotationAngle = Vector3.zero;
                aNewPosition = Vector3.zero;
                return false;
            }
            aNewRotationAngle = node.rotation;
            aNewPosition = node.position;
            return true;
        }

        public float GetHeight(string frameworkId)
        {
            Vector3 pos = Vector3.zero;
            TryGetNodePosition(frameworkId, CharacterFrameworkNodeTypeEnum.Head.ToString(), out pos);
            return pos.y;
        }

        public IEnumerable<string> GetFrameworkNodeIds(string frameworkId)
        {
            CharacterFramework framework = null;
            if (TryGetFramework(frameworkId, out framework))
            {
                return framework.GetAllNodeIds();
            }
            return new List<string>(); ;
        }

        public IEnumerable<string> GetFrameworkRecognizeStepIds(string frameworkId)
        {
            CharacterFramework framework = null;
            if (TryGetFramework(frameworkId, out framework))
            {
                return framework.GetAllStepIds();
            }
            return new List<string>(); ;
        }

        public List<string> ListFrameworkFilesInFolder(string dir, string pattern, SearchOption option)
        {
            List<string> output = new List<string>();
            if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
            {
                return output;
            }
            if (string.IsNullOrWhiteSpace(pattern))
            {
                pattern = "*.*";
            }
            output.AddRange(Directory.GetFiles(dir, pattern, option));
            return output;
        }

        public bool SaveFrameworkToFile(string frameworkId, Texture2D previewImg = null)
        {
            var frame = GetFramework(frameworkId);
            if(null == frame)
            {
                return false;
            }

            Directory.CreateDirectory(frameworkDir);
            string path = GetFrameworkFilePath(frame.id);
            try
            {
                string json = JsonUtility.ToJson(frame);
                File.WriteAllText(path, json);
            }
            catch(System.Exception e)
            {
                Debug.LogException(e);
                return false;
            }

            if (null != previewImg)
            {
                try
                {
                    string imgPath = Path.ChangeExtension(path, frameworkPreviewImageFileExtention);
                    var data = previewImg.EncodeToPNG();
                    File.WriteAllBytes(imgPath, data);
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }
            }

            return true;
        }

        public string LoadFromText(string id, string jsonText)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(jsonText))
            {
                return null;
            }
            try
            {
                var frame = JsonUtility.FromJson<CharacterFramework>(jsonText);
                if (null != frame)
                {
                    CreateFramework(id);
                    var managedFrame = GetFramework(id);
                    managedFrame.CopyFrom(frame);
                    return id;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
            return null;
        }

        public string LoadFrameworkFromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return null;
            }

            if (!File.Exists(filePath))
            {
                return null;
            }

            string id = Path.GetFileNameWithoutExtension(filePath);
            string data = File.ReadAllText(filePath);
            return LoadFromText(id, data);
        }

        private CharacterFramework GetFramework(string frameworkId)
        {
            CharacterFramework output = null;
            TryGetFramework(frameworkId, out output);
            return output;
        }

        private CharacterFrameworkNode GetFrameworkNode(string frameworkId, string nodeId)
        {
            CharacterFramework framework = null;
            if (TryGetFramework(frameworkId, out framework))
            {
                return framework.GetNode(nodeId);
            }
            else
            {
                return null;
            }
        }

        public CharacterFrameworkRecognitionStep GetFrameworkRecognizeStep(string frameworkId, string stepId)
        {
            CharacterFramework framework = null;
            if (TryGetFramework(frameworkId, out framework))
            {
                return framework.GetStep(stepId);
            }
            else
            {
                return null;
            }
        }

        private bool TryGetFramework(string id, out CharacterFramework value)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                value = null;
                return false;
            }
            return data.TryGetValue(id, out value);
        }
    }
}