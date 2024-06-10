using UnityEditor;
using UnityEngine;

namespace SkillsVR.CCK.PackageManager.UI.Views
{
    public class ScriptableObject<T> : ScriptableObject
    {
        public T data;
    }

    public static class CustomSerializedObjectExtensions
    {
        public static SerializedObject CreateSerializedObject<WrapperType, DataType>(this DataType data) where WrapperType : ScriptableObject<DataType>
        {
            var rp = ScriptableObject.CreateInstance<WrapperType>();
            rp.data = data;
            var serializedObject = new SerializedObject(rp);
            return serializedObject;
        }
    }
}