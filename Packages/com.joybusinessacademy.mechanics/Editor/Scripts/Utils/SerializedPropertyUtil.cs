using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;
using System.Collections;
using UnityEditor;

namespace VRMechanics.Editor
{
	public static class SerializedPropertyUtil
    {
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
                path = path.Substring(0, path.Length - lastElementName.Length - (".Array.".Length));
            }
            else
            {
                path = path.Substring(0, path.Length - lastElementName.Length - 1);
            }
            return prop.serializedObject.FindProperty(path);
        }

        public static void SetValue(this SerializedProperty property, object val)
        {
            object obj = property.serializedObject.targetObject;

            List<KeyValuePair<FieldInfo, object>> list = new List<KeyValuePair<FieldInfo, object>>();

            FieldInfo field = null;
            foreach (var path in property.propertyPath.Split('.'))
            {
                var type = obj.GetType();
                field = type.GetField(path, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                // Generic type field will be not found in child class, so search base class and get the field.
                while (null == field && null != type)
                {
                    type = type.BaseType;
                    if (null != type)
                    {
                        field = type.GetField(path, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    }
                    if (null != field)
                    {
                        break;
                    }
                }
                list.Add(new KeyValuePair<FieldInfo, object>(field, obj));
                obj = field.GetValue(obj);
            }

            // Now set values of all objects, from child to parent
            for (int i = list.Count - 1; i >= 0; --i)
            {
                list[i].Key.SetValue(list[i].Value, val);
                // New 'val' object will be parent of current 'val' object
                val = list[i].Value;
            }
        }

        public static object GetValue(this SerializedProperty prop)
        {
            string path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            string[] elements = path.Split('.');

            foreach (string element in elements.Take(elements.Length))
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

            return obj;
        }

        private static object GetValue(object source, string name)
        {
            if (source == null)
            {
                return null;
            } 
            var type = source.GetType();
            var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (f != null)
            {
                return f.GetValue(source);
            }
            var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (p != null)
            {
                return p.GetValue(source, null);
            }
            // Generic type field/property will be not found in child class, so search base class and get them.
            type = type.BaseType;
            while (null != type)
            {
                f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                {
                    return f.GetValue(source);
                }
                p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (p != null)
                {
                    return p.GetValue(source, null);
                }
                type = type.BaseType;
            }
            return null;
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