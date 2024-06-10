using SkillsVR.CCK.PackageManager.UI.Views;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVR.CCK.PackageManager.UI
{
    public static class VisualElementButtonBindExtensions
    {
        public static void BindButtons(this VisualElement visual)
        {
            var btns = visual.Query<Button>().Where(btn => !string.IsNullOrWhiteSpace(btn.bindingPath));
            foreach (var btn in btns.ToList())
            {
                btn.RegisterCallback<ClickEvent>(visual.OnButtonClickBind);
            }
        }

        public static void UnBindButtons(this VisualElement visual)
        {
            var btns = visual.Query<Button>().Where(btn => !string.IsNullOrWhiteSpace(btn.bindingPath));
            foreach (var btn in btns.ToList())
            {
                btn.UnregisterCallback<ClickEvent>(visual.OnButtonClickBind);
            }
        }

        private static void OnButtonClickBind(this VisualElement visual, ClickEvent clickEvent)
        {
            Button btn = clickEvent.target as Button;
            string invokerPath = GetInvokerPath(btn.bindingPath);

            string methodName = GetMethodName(btn.bindingPath);
            object invoker = visual.GetInvoker(invokerPath);

            if (null == invoker)
            {
                return;
            }

            var method = invoker.GetType().FindMethodByName(methodName);
            if (null != method)
            {
                method.Invoke(invoker, null);
            }
            if (null == method && null != invoker)
            {
                method = invoker.GetType().FindExtensionMethodByName(methodName);
                if (null != method)
                {
                    method.Invoke(null, new object[] { invoker });
                }
            }

            if (null == method)
            {
                Debug.LogError("No method found by " + btn.bindingPath);
                return;
            }

        }


        private static string GetInvokerPath(string bindingPath)
        {
            var index = bindingPath.LastIndexOf('.');
            if (0 > index)
            {
                return null;
            }

            string path = bindingPath.Substring(0, index);
            return path;
        }

        private static string GetMethodName(string bindingPath)
        {
            var index = bindingPath.LastIndexOf('.');
            if (0 > index)
            {
                return bindingPath;
            }


            string path = bindingPath.Substring(index).TrimStart('.');
            return path;
        }





        private static object GetInvoker(this VisualElement visual, string bindingPath)
        {
            if (visual is IBindingVisualElement bindingVisual && null != bindingVisual)
            {
                if (null == bindingVisual.BindingSerializer)
                {
                    return visual;
                }
                var p = bindingVisual.BindingSerializer.FindProperty(bindingPath);
                if (null == p)
                {
                    return visual;
                }

                return GetTargetObjectOfProperty(p);
            }
            return visual;
        }

        public static object GetTargetObjectOfProperty(SerializedProperty prop)
        {
            if (prop == null) return null;

            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }
            return obj;
        }

        public static object GetTargetObjectOfProperty(SerializedProperty prop, object targetObj)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    targetObj = GetValue_Imp(targetObj, elementName, index);
                }
                else
                {
                    targetObj = GetValue_Imp(targetObj, element);
                }
            }
            return targetObj;
        }

        private static object GetValue_Imp(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                    return f.GetValue(source);

                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                    return p.GetValue(source, null);

                type = type.BaseType;
            }
            return null;
        }

        private static object GetValue_Imp(object source, string name, int index)
        {
            var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
            if (enumerable == null) return null;
            var enm = enumerable.GetEnumerator();
            //while (index-- >= 0)
            //    enm.MoveNext();
            //return enm.Current;

            for (int i = 0; i <= index; i++)
            {
                if (!enm.MoveNext()) return null;
            }
            return enm.Current;
        }
    }
}