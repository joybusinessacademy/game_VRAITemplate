using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SkillsVR.CCK.PackageManager.Startup
{
    public static class TypeExtensions
    {
        public static void InvokeAllOnLoadMethodsWithThisAttribute(this Type attributeType, IEnumerable<Type> types)
        {
            var methods = types
                .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(m => m.IsDefined(attributeType, false)
                                && 0 == m.GetParameters().Length)
                );

            foreach (var method in methods)
            {
                method?.Invoke(null, null);
            }
        }
    }
}