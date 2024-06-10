using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SkillsVR.CCK.PackageManager.UI.Views
{
    public static class TypeExtensions
    {
        private static IEnumerable<MethodInfo> allExtensionMethods;

        public static MethodInfo FindMethodByName(this Type targetType, string methodName)
        {
            if (null == targetType)
            {
                return null;
            }
            if (string.IsNullOrWhiteSpace(methodName))
            {
                return null;
            }

            var method = targetType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(x => x.Name == methodName);

            return method;
        }

        public static MethodInfo FindExtensionMethodByName(this Type targetType, string methodName)
        {
            if (null == targetType)
            {
                return null;
            }
            if (string.IsNullOrWhiteSpace(methodName))
            {
                return null;
            }
            var extenAttrType = typeof(ExtensionAttribute);

            if (null == allExtensionMethods)
            {
                allExtensionMethods = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsSealed && !t.IsNested && t.IsAbstract)
                .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public).Where(m => m.IsDefined(extenAttrType)));
            }
                
            var method = allExtensionMethods.FirstOrDefault(m =>
                          m.Name == methodName
                          && m.GetParameters().First().ParameterType == targetType
                          );

            return method;
        }
    }
}