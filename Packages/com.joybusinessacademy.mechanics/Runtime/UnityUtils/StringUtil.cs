using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace SkillsVR.UnityExtenstion
{
    public static class StringUtil
    {
        private static System.Random random = new System.Random();

        public static List<string> ExtractTag(this string text, string tagStart, string tagEnd)
        {
            var splits = text.Split(new string[] { tagEnd }, StringSplitOptions.None);
            for (int i = 0; i < splits.Length; i++)
                splits[i] = splits[i].Contains(tagStart) ? splits[i] + tagEnd : splits[i];

            List<string> keywords = new List<string>();
            for (int i = 0; i < splits.Length; i++)
            {
                Regex regex = new Regex(string.Format("{0}(.*){1}", tagStart, tagEnd));
                var v = regex.Match(splits[i]);
                if (v.Groups[1].Length > 0)
                    keywords.Add(v.Groups[1].ToString());
            }

            return keywords;
        }
        /// <summary>
        /// Returns the collection of ToString() of multiple objects
        /// with a line-break separating them.
        /// </summary>
        /// <param name="objects"></param>
        /// <returns></returns>
        public static string ToStringOfObjectsWithLinebreaks(ICollection objects)
        {
            string objectsString = "";

            foreach (var obj in objects)
                objectsString += obj + "\n";

            return objectsString.TrimEnd();
        }

        /// <summary>
        /// Returns a string of Unity Object names with a separator (comma by default).
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string ListToNamesString<T>(ICollection<T> collection, string separator = ",")
            where T : UnityEngine.Object
        {
            string objectsString = "";

            for (int i = 0; i < collection.Count; i++)
            {
                var obj = collection.ElementAt(i);

                objectsString += obj.name;

                if (i < collection.Count - 1)
                    objectsString += separator;
            }

            return objectsString;
        }

        /// <summary>
        /// Returns a string of objects with a separator (comma by default).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string ListToString<T>(ICollection<T> collection, string separator = ",")
        {
            string objectsString = "";

            for (int i = 0; i < collection.Count; i++)
            {
                var obj = collection.ElementAt(i);

                objectsString += obj.ToString();

                if (i < collection.Count - 1)
                    objectsString += separator;
            }

            return objectsString;
        }

        /// <summary>
        /// Returns the supplied Objects properties as a string formatted {propertyName : value }
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToString(object obj)
        {
            var output = "[" + obj.GetType().Name + "] - ";
            var properties = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            var firstProp = true;

            // Best method: uses the properties discovered using reflection
            if (properties.Length > 0)
            {
                foreach (var prop in properties)
                {
                    if (!firstProp)
                    {
                        output += ", ";
                    }

                    output += prop.Name + ": " + prop.GetValue(obj);

                    firstProp = false;
                }
            }

            // fallback solution: some objects cannot discover properties by reflection
            else
            {
                var objTypes = ((IEnumerable)obj).Cast<object>().Select(x => x.GetType().ToString()).ToArray();
                var objValues = ((IEnumerable)obj).Cast<object>().Select(x => x.ToString()).ToArray();

                for (var i = 0; i < objTypes.Length; i++)
                {
                    if (!firstProp)
                    {
                        output += ", ";
                    }

                    output += objTypes[i] + ": " + objValues[i];

                    firstProp = false;
                }
            }

            return output;
        }

        /// <summary>
        /// Returns the supplied Dictionary as a string formatted {item.Key : value }, for each item in the Dictionary
        /// </summary>
        /// <param name="dictionary"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static string ToString<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        {
            var output = "[Dictionary] - ";
            var firstProp = true;

            foreach (var item in dictionary)
            {
                if (!firstProp)
                {
                    output += ", ";
                }

                output += "[ " + item.Key + ": " + item.Value + " ]";

                firstProp = false;
            }

            return output;
        }

        /// <summary>
        /// Returns a randomString of the supplied length
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static readonly string[] Vowels = { "a", "e", "i", "o", "u" };

        public static bool StartsWithVowel(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            var firstLetter = value.ToLower().Substring(0, 1);

            return Vowels.Contains(firstLetter);
        }

        /// <summary>
        /// Returns a string that is either "a" or "an" depending on the following word.
        /// </summary>
        /// <returns>The an.</returns>
        /// <param name="word">The following word.</param>
        /// <param name="capital">If set to <c>true</c> capital.</param>
        public static string AOrAn(string word, bool capital = false)
        {
            string s = capital ? "A" : "a";

            if (word.StartsWithVowel())
                s += "n";

            return s;
        }

        public static string ToShortenedString(this string value, int length = 12, bool addDotsIfShortened = true)
        {
            return StringUtil.GetShortenedString(value, length, addDotsIfShortened);
        }

        public static string GetShortenedString(string value, int length = 12, bool addDotsIfShortened = true)
        {
            if (value == null)
                return null;
            var shortened = length < value.Length;
            length = Mathf.Min(length, value.Length);

            string s = value.Substring(0, length);

            if (shortened && addDotsIfShortened)
                s += "...";

            return s;
        }

        /// <summary>
        /// Converts a Unity Color to a hex string.
        /// </summary>
        /// <returns>The color as a hex string.</returns>
        /// <param name="color">The Color object.</param>
        public static string ColorToHex(Color32 color)
        {
            string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");

            return hex;
        }

        /// <summary>
        /// Converts a hex string to a Unity Color.
        /// </summary>
        /// <returns>The Color object.</returns>
        /// <param name="color">The hex string.</param>
        public static Color HexToColor(string hex)
        {
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

            return new Color32(r, g, b, 255);
        }

        public static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static string ToStringFormatted(this TimeSpan t)
        {
            string answer = string.Format("{0:D4}h:{1:D2}m:{2:D2}s",//:{3:D3}",
                Mathf.FloorToInt((float)t.TotalHours),          // 0
                t.Minutes,              // 1
                t.Seconds);              // 2
                                         //t.Milliseconds);        // 3

            return answer;
        }

        public static float ConvertStringToSeconds(string str)
        {
            var n = str.Split(':');

            float h = float.Parse(n[0]) * 3600;
            float m = float.Parse(n[1]) * 60;
            float s = float.Parse(n[2]);

            return h + m + s;
        }

        public static string ConvertSecondsToMinutesAndSeconds(float secs)
        {
            TimeSpan t = TimeSpan.FromSeconds(secs);

            string answer = string.Format("{0:D2}:{1:D2}",
                t.Minutes,
                t.Seconds);

            return answer;
        }

        public static string ConvertSecondsToString(float secs)
        {
            TimeSpan t = TimeSpan.FromSeconds(secs);

            string answer = string.Format("{0:D2}:{1:D2}:{2:D3}",
                //t.Hours,
                t.Minutes,
                t.Seconds,
                t.Milliseconds);

            //		string answer = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
            //			t.Hours,
            //			t.Minutes,
            //			t.Seconds,
            //			t.Milliseconds);

            return answer;
        }

        public static string NumberFormat(int num)
        {
            return string.Format("{0:#,###0.#}", num);
        }

        public static string ToDp(this float num, int dp = 2)
        {
            return StringUtil.Dp(num, dp);
        }

        public static string ToDp(this Vector3 vec, int dp = 2)
        {
            return StringUtil.Dp(vec, dp);
        }

        public static string Dp(float num, int dp = 2)
        {
            return num.ToString("n" + dp);
        }

        public static string RemoveLineEndings(this string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return value;
            }
            string lineSeparator = ((char)0x2028).ToString();
            string paragraphSeparator = ((char)0x2029).ToString();

            return value.Replace("\r\n", string.Empty)
                        .Replace("\n", string.Empty)
                        .Replace("\r", string.Empty)
                        .Replace(lineSeparator, string.Empty)
                        .Replace(paragraphSeparator, string.Empty);
        }

        public static string Dp(Vector3 vec, int dp = 2)
        {
            var x = StringUtil.Dp(vec.x, dp);
            var y = StringUtil.Dp(vec.y, dp);
            var z = StringUtil.Dp(vec.z, dp);

            return string.Format("{0}, {1}, {2}", x, y, z);
        }

        public static string GetDateTimeForFiles(string suffix, string prefix = "", DateTime dt = default(DateTime))
        {
            if (dt == default(DateTime))
                dt = DateTime.Now;

            return string.Format(prefix + " {0:yyyy-MM-dd_hh-mm-ss-tt}" + suffix, dt);
        }

        public static string DateTimeToBinaryString(DateTime dateTime)
        {
            return dateTime.ToBinary().ToString();
        }

        public static DateTime BinaryStringToDateTime(string dateTimeString)
        {
            return DateTime.FromBinary(Convert.ToInt64(dateTimeString));
        }

        [Obsolete]
        //public static string ApplyCapitalisation(string text, UI.Styles.TextFontStyle.TextCapitalisationStyle capStyle)
        //{
        //    if (capStyle == UI.Styles.TextFontStyle.TextCapitalisationStyle.AllCaps)
        //        return text.ToUpper();
        //    else if (capStyle == UI.Styles.TextFontStyle.TextCapitalisationStyle.AllLowercase)
        //        return text.ToLower();
        //    else if (capStyle == UI.Styles.TextFontStyle.TextCapitalisationStyle.CamelCase)
        //        return UppercaseFirstEach(text);
        //    else if (capStyle == UI.Styles.TextFontStyle.TextCapitalisationStyle.CaramelCase)
        //        return UppercaseFirstEach(text).Replace(" ", string.Empty);

        //    return text;
        //}

        public static string UppercaseFirstEach(string s)
        {
            char[] a = s.ToLower().ToCharArray();

            for (int i = 0; i < a.Length; i++)
            {
                a[i] = i == 0 || a[i - 1] == ' ' ? char.ToUpper(a[i]) : a[i];

            }

            return new string(a);
        }

        private const long OneKb = 1024;
        private const long OneMb = OneKb * 1024;
        private const long OneGb = OneMb * 1024;
        private const long OneTb = OneGb * 1024;

        public static string ToFileSize(this ulong value, int decimalPlaces = 0)
        {
            return ToFileSize((long)value, decimalPlaces);
        }
        public static string ToFileSize(this long value, int decimalPlaces = 0)
        {
            var asTb = Math.Round((double)value / OneTb, decimalPlaces);
            var asGb = Math.Round((double)value / OneGb, decimalPlaces);
            var asMb = Math.Round((double)value / OneMb, decimalPlaces);
            var asKb = Math.Round((double)value / OneKb, decimalPlaces);
            string chosenValue = asTb > 1 ? string.Format("{0}Tb", asTb)
                : asGb > 1 ? string.Format("{0}Gb", asGb)
                : asMb > 1 ? string.Format("{0}Mb", asMb)
                : asKb > 1 ? string.Format("{0}Kb", asKb)
                : string.Format("{0}B", Math.Round((double)value, decimalPlaces));
            return chosenValue;
        }


    }
}
