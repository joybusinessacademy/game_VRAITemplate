using System.Linq;

namespace SkillsVR.CCK.PackageManager.Data
{
    public static class CCKInfoExtensions
    {
        public static bool HasFlag(this CCKInfo package, string flag)
        {
            if (null == package
                || string.IsNullOrWhiteSpace(flag)
                || null == package.flags)
            {
                return false;
            }

            string head = flag.Trim(' ') + "=";
            return package.flags.Any(f => f == flag || f.StartsWith(head));
        }

        public static int GetValueInt(this CCKInfo package, string key, int defaultValue = 0)
        {
            string valueStr = GetValueString(package, key);
            if (string.IsNullOrWhiteSpace(valueStr))
            {
                return defaultValue;
            }
            int output = defaultValue;
            if (int.TryParse(valueStr, out output))
            {
                return output;
            }
            return defaultValue;
        }


        public static string GetValueString(this CCKInfo package, string key)
        {
            if (null == package
               || string.IsNullOrWhiteSpace(key)
               || null == package.flags)
            {
                return string.Empty;
            }

            var head = key.Trim(' ') + "=";
            var flagValueStr = package.flags.FirstOrDefault(f => f.StartsWith(head));

            if (string.IsNullOrWhiteSpace(flagValueStr))
            {
                return string.Empty;
            }

            return flagValueStr.Substring(head.Length);
        }

        public static string GetInhertValueString(this CCKInfo info, string key)
        {
            var curr = info;

            while(null != curr)
            {
                var value = curr.GetValueString(key);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
                else
                {
                    curr = curr.Parent;
                }
            }
            return string.Empty;
        }

        public static int GetInhertValueInt(this CCKInfo info, string key, int defaultValue = 0)
        {
            var curr = info;

            while (null != curr)
            {
                var value = curr.GetValueString(key);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    int v = defaultValue;
                    if (int.TryParse(value, out v))
                    {
                        return v;
                    }
                }
                curr = curr.Parent;
            }
            return defaultValue;
        }
    }
}