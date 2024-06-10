using System.Collections.Generic;
using System.Linq;

namespace SkillsVR.CCK.PackageManager
{
    public static class ObjectToTextExtensions
    {
        public static string ToText(this object obj)
        {
            if (null == obj)
            {
                return "null";
            }
            return obj.ToString();
        }

        public static string ToText(this object obj, string header)
        {
            if (string.IsNullOrWhiteSpace(header))
            {
                return obj.ToText();
            }
            return $"{header}: {obj.ToText()}";
        }

        
    }
}