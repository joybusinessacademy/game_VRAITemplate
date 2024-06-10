using System.Collections.Generic;
using System.Linq;

namespace SkillsVR
{
	public static class ObjectExtension
    {

        public static T ToType<T>(this object item, T defaultValue)
        {
            if (null == item)
            {
                return defaultValue;
            }
            if (item is T)
            {
                return (T)item;
            }
            return defaultValue;
        }
        /// <summary>
        /// Get object string with format.
        /// </summary>
        /// <param name="item">object to print, could be null</param>
        /// <param name="format">output string format, {t} - type, {o} - object</param>
        /// <returns></returns>
        public static string GetString(this object item, string format = "({t}) {o}")
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                return item.GetString("({t}) {o}");
            }
            return format.Replace("{t}", item.GetTypeName()).Replace("{o}", item.GetObjectString());
        }

        public static string GetObjectString(this object item)
        {
            if (null == item)
            {
                return "null";
            }
            return item.ToString();
        }

        public static string GetTypeName(this object item)
        {
            if (null == item)
            {
                return "null";
            }
            return item.GetType().Name;
        }

        /// <summary>
        ///  Get object string with format from IEnumerable<object>.
        /// </summary>
        /// <param name="objList">source to print</param>
        /// <param name="objectFormat">print format for each object, {t} - type, {o} - object</param>
        /// <param name="surfix">string at end for each object, format {i} - index</param>
        /// /// <param name="prefix">string at start for each object, format {i} - index</param>
        /// <returns></returns>
        public static string GetObjectListString(this IEnumerable<object> objList, string objectFormat = "({t}) {o}", string surfix = ", ", string prefix = "")
        {
            if (null == objList)
            {
                return null;
            }
            surfix = null == surfix ? ", " : surfix;
            prefix = null == prefix ? "" : prefix;

            int index = 0;
            string info = "";
            foreach (var obj in objList)
            {
                ++index;
                info += prefix.Replace("{i}", index.ToString()) + obj.GetString(objectFormat) + surfix.Replace("{i}", index.ToString());
            }
            return info;
        }
    }
}
