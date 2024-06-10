using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace SkillsVR.TimelineTool.Editor
{
    public static class SerializedPropertyUtil
    {
        public static Type GetFieldType(this SerializedProperty prop)
        {
            var targetObject = prop.serializedObject.targetObject;

            object value = null;
            var info = prop.GetMemberInfo(out value);
            if (null == info || null == value)
            {
                return null;
            }
            FieldInfo fieldInfo = value.GetType().GetField(info.Name);
            if (fieldInfo == null)
            {
                PropertyInfo propInfo = value.GetType().GetProperty(info.Name);
                if (propInfo != null)
                    return propInfo.PropertyType;
            }
            else
                return fieldInfo.FieldType;
            return null;
        }
        public static object Invoke(this SerializedProperty prop, string methodName, object[] args)
        {
            var targetObject = prop.GetValue();
            var method = targetObject.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return method.Invoke(targetObject, args);
        }

        public static object TryInvoke(this SerializedProperty prop, string methodName, object[] args)
        {
            if (string.IsNullOrWhiteSpace(methodName))
            {
                return null;
            }
            var targetObject = prop.GetValue();
            if (null == targetObject)
            {
                return null;
            }
            var method = targetObject.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (null == method)
            {
                return null;
            }
            try
            {
                return method.Invoke(targetObject, args);
            }
            catch
            {
                return null;
            }
        }

        public static SerializedProperty GetParent(this SerializedProperty prop)
        {
            string path = prop.propertyPath;
            string[] elements = path.Split('.');
            if (null == elements || 2 > elements.Length)
            {
                return null;
            }
            string lastElementName = elements[elements.Length - 1];
            if (lastElementName.StartsWith("data[") && lastElementName.EndsWith("]"))
            {
                path = path.Substring(0, path.Length - lastElementName.Length - ".Array.".Length);
            }
            else
            {
                path = path.Substring(0, path.Length - lastElementName.Length - 1);
            }
            return prop.serializedObject.FindProperty(path);
        }

        public static void SetValue(this SerializedProperty property, object val)
        {
            object target = null;
            var mem = property.GetMemberInfo(out target);
            mem.SetValue(target, val);
        }

        public static object GetValue(this SerializedProperty property)
        {
            object target = null;
            var mem = property.GetMemberInfo(out target);
            return mem.GetValue(target);
        }

        private static MemberInfo GetMemberInfo(this SerializedProperty prop, out object targetObj)
        {
            string path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            string[] elements = path.Split('.');

            foreach (string element in elements.Take(elements.Length - 1))
            {
                if (element.Contains("["))
                {
                    string elementName = element.Substring(0, element.IndexOf("["));
                    int index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue(obj, elementName, index);
                }
                else
                {
                    obj = GetValue(obj, element);
                }
            }

            targetObj = obj;
            return GetMemberInfo(obj, elements.Last());
        }



        private static MemberInfo GetMemberInfo(object source, string name)
        {
            if (source == null)
            {
                return null;
            }
            var type = source.GetType();
            var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (f != null)
            {
                return f;
            }
            var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (p != null)
            {
                return p;
            }
            // Generic type field/property will be not found in child class, so search base class and get them.
            type = type.BaseType;
            while (null != type)
            {
                f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                {
                    return f;
                }
                p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (p != null)
                {
                    return p;
                }
                type = type.BaseType;
            }
            return null;
        }


        public static void SetValue(this MemberInfo memberInfo, object target, object value)
        {
            if (null == memberInfo)
            {
                return;
            }
            FieldInfo f = memberInfo as FieldInfo;
            if (null != f)
            {
                try
                {
                    value = Convert.ChangeType(value, f.FieldType);
                }
                catch { }
                f.SetValue(target, value);
                return;
            }

            PropertyInfo p = memberInfo as PropertyInfo;
            if (null != p)
            {
                try
                {
                    value = Convert.ChangeType(value, p.PropertyType);
                }
                catch { }
                p.SetValue(target, value);
                return;
            }
        }

        public static object GetValue(this MemberInfo memberInfo, object target)
        {
            if (null == memberInfo)
            {
                return null;
            }
            FieldInfo f = memberInfo as FieldInfo;
            if (null != f)
            {
                return f.GetValue(target);
            }

            PropertyInfo p = memberInfo as PropertyInfo;
            if (null != p)
            {
                return p.GetValue(target);
            }
            return null;
        }

        private static object GetValue(object source, string name)
        {
            var member = GetMemberInfo(source, name);
            return member.GetValue(source);
        }


        private static object GetValue(object source, string name, int index)
        {
            var enumerable = GetValue(source, name) as IEnumerable;
            var enm = enumerable.GetEnumerator();
            while (index-- >= 0)
                enm.MoveNext();
            return enm.Current;
        }
    }
}