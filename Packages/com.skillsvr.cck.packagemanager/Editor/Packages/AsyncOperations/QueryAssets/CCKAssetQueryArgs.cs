using System.Collections.Generic;
using UnityEngine;

namespace SkillsVR.CCK.PackageManager.AsyncOperations
{
    public static class Ie
    {
        public static bool TryAddQueryValues(this IEnumerable<string> src, IEnumerable<string> values)
        {
            if (null == src || null== values)
            {
                return false;
            }
            foreach(var v in values)
            {
                src.TryAddQueryValue(v);
            }
            return true;
        }
        public static bool TryAddQueryValue(this IEnumerable<string> src, string value)
        {
            if (null == src || string.IsNullOrWhiteSpace(value))
            {
                return false;
            }
            HashSet<string> set = src as HashSet<string>;
            if (null == set)
            {
                return false;
            }
            return set.Add(value);
        }
    }
    public class CCKAssetQueryArgs
    {
        public string RawQueryText { get; protected set; }

        public IEnumerable<string> KeyWords { get; protected set; } = new HashSet<string>(0);

        // t:A;b;c;
        public IEnumerable<string> Types { get; protected set; } = new HashSet<string>(0);
        // f:a;b;c;
        public IEnumerable<string> Flags { get; protected set; } = new HashSet<string>(0);
        // c:a;b;c
        public IEnumerable<string> Categories { get; protected set; } = new HashSet<string>(0);
        // p:a;b;c;
        public IEnumerable<string> Packages { get; protected set; } = new HashSet<string>(0);

        public int size { get; protected set; }
        public int from { get; protected set; }

        public CCKAssetQueryArgs(int fromIndex = 0, int sizePerPage = 24)
        {
            SetRange(fromIndex, sizePerPage);
        }
        
        public void SetRange(int fromIndex, int sizePerPage)
        {
            from = Mathf.Max(0, fromIndex);
            size = Mathf.Max(1, sizePerPage);
        }

        public void SetFromPage(int pageIndexFrom0, int pageSize = 0)
        {
            pageSize = 0 < pageSize ? pageSize : size;
            SetRange(pageIndexFrom0 * pageSize, pageSize);
        }
        public void NextPage()
        {
            SetRange(from + size, size);
        }

        public void PrevPage()
        {
            SetRange(from - size, size);
        }

        public void Clear()
        {
            (KeyWords as HashSet<string>)?.Clear();
            (Types as HashSet<string>)?.Clear();
            (Flags as HashSet<string>)?.Clear();
            (Categories as HashSet<string>)?.Clear();
            (Packages as HashSet<string>)?.Clear();
        }

        public override string ToString()
        {
            return string.Join("\r\n",
                $"Text: {(string.IsNullOrWhiteSpace(RawQueryText) ? "null" : RawQueryText)}",
                $"Keywords: {string.Join("; ", KeyWords)}",
                $"Categories: {string.Join("; ", Categories)}",
                $"Types: {string.Join("; ", Types)}",
                $"Flags: {string.Join("; ", Flags)}",
                $"Packages: {string.Join("; ", Packages)}",
                $"From: {from}",
                $"Size: {size}"
                );
        }
    }
}