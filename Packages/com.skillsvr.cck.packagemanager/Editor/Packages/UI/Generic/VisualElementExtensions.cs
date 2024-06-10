using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.UIElements.Cursor;

namespace SkillsVR.CCK.PackageManager.UI
{
    public static class VisualElementExtensions
    {
        public static void ReloadVisualTreeAssetByType(this VisualElement visual)
        {
            if (null == visual)
            {
                return;
            }

            visual.ReloadVisualTreeAssetResource(visual.GetType().Name);
        }

        public static void ReloadVisualTreeAssetResource(this VisualElement visual, string resourcePath)
        {
            if (null == visual)
            {
                return;
            }
            visual.Clear();

            var asset = Resources.Load<VisualTreeAsset>(resourcePath);
            asset.CloneTree(visual);
            visual.EjectVisualElementsToProperty();
        }

        static string ToUSSName(this string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "unknown";
            }

            string ussName = name.Replace("_", "-");
            ussName = Regex.Replace(ussName, "([A-Z])", "-$1");
            ussName = Regex.Replace(ussName, "-+", "-");
            return ussName.TrimStart('-').ToLower();
        }

        static VisualElement QueryByName(this VisualElement visual, params string[] names)
        {
            if (null == names)
            {
                return null;
            }

            foreach (var name in names)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                if (name.Contains("_")
                    || name.Any(c => char.IsUpper(c)))
                {
                    var ussName = name.ToUSSName();
                    var v = visual.Q(ussName);
                    if (null != v)
                    {
                        return v;
                    }
                }

                var r = visual.Q(name);
                if (null != r)
                {
                    return r;
                }
            }

            return null;
        }

        public static void SetCursor(this VisualElement element, MouseCursor cursor)
        {
            object objCursor = new Cursor();
            PropertyInfo fields = typeof(Cursor).GetProperty("defaultCursorId", BindingFlags.NonPublic | BindingFlags.Instance);
            fields.SetValue(objCursor, (int)cursor);
            element.style.cursor = new StyleCursor((Cursor)objCursor);
        }

        public static void EjectVisualElementsToProperty(this VisualElement visual)
        {
            var ps = visual.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(p => p.IsDefined(typeof(VisualBindAttribute),  true));

            foreach(var p in ps)
            {
                if (null == p.SetMethod)
                {
                    continue;
                }
                var attr = p.GetCustomAttribute<VisualBindAttribute>(true);
                if (null == attr)
                {
                    continue;
                }

                var item = visual.QueryByName(attr.Name, p.Name);
                if (null == item)
                {
                    string[] names = { attr.Name, p.Name };
                    var xNames = names.Select(n => n.ToUSSName()).Where(n => "unknown" != n);
                    Debug.LogError($"Eject element by name fail: Cannot find any of {string.Join(", ", xNames)}");
                }
                p.SetValue(visual, item);
            }


            var fs = visual.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(p => p.IsDefined(typeof(VisualBindAttribute), true));

            foreach (var f in fs)
            {
                var attr = f.GetCustomAttribute<VisualBindAttribute>(true);
                if (null == attr)
                {
                    continue;
                }
                string name = string.IsNullOrWhiteSpace(attr.Name) ? f.Name : attr.Name;
                var item = visual.QueryByName(attr.Name, f.Name);
                if (null == item)
                {
                    string[] names = { attr.Name, f.Name };
                    var xNames = names.Select(n => n.ToUSSName()).Where(n => "unknown" != n);
                    Debug.LogError($"Eject element by name fail: Cannot find any of {string.Join(", ", xNames)}");
                }
                f.SetValue(visual, item);
            }
        }
        public static void SetBackgroundImage(this VisualElement visual, Texture2D texture2D)
        {
            visual.style.backgroundImage = new StyleBackground(texture2D);
        }

        public static void LerpBackgroundColor(this VisualElement visual, Color from, Color to, float t)
        {
            if (null == visual)
            {
                return;
            }
            var c = Color.Lerp(from, to, t);
            visual.style.backgroundColor = new StyleColor(c);
        }

        public static void SetDisplay(this VisualElement visual, bool display)
        {
            if (null == visual)
            {
                return;
            }

            visual.style.display = display ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}