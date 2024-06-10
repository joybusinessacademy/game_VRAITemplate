using System;
using System.Collections.Generic;
using System.Linq;

namespace SkillsVR.TimelineTool
{
    public static class TypeExtensions
    {
        public static IEnumerable<Type> GetAllSubClassTypesFromBaseTypes(this Type mainType, bool includeMaintype, Type[] typeGroup, params Type[] baseTypes)
        {
            List<Type> baseTypeList = new List<Type>();
            if (includeMaintype && null != mainType)
            {
                baseTypeList.Add(mainType);
            }
            if (null != typeGroup)
            {
                baseTypeList.AddRange(typeGroup.Where(t => null != t));
            }
            if (null != baseTypes)
            {
                baseTypeList.AddRange(baseTypes.Where(t => null != t));
            }
            var list = baseTypeList.Distinct();
            return GetAllSubClassTypesFromBaseTypes(list.ToArray());
        }


        private static IEnumerable<Type> GetAllSubClassTypesFromBaseTypes(params Type[] baseTypes)
        {
            var allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract);

            List<Type> outputTypes = new List<Type>();
            foreach(var baseType in baseTypes)
            {
                if (null == baseType)
                {
                    continue;
                }
                outputTypes.AddRange(allTypes.Where(t => t == baseType || baseType.IsAssignableFrom(t)));
            }
            return outputTypes;
        }

        public static bool IsOrSubTypeOf(this Type testType, Type baseType)
        {
            if (null == testType || null == baseType)
            {
                return true;
            }

            return testType == baseType || baseType.IsAssignableFrom(testType);
        }

        public static string ToSerializableTypeString(this Type type)
        {
            return null == type ? null : type.AssemblyQualifiedName;
        }

        public static Type ParseToType(this string typeInfo)
        {
            if (string.IsNullOrWhiteSpace(typeInfo))
            {
                return null;
            }

            try
            {
                return Type.GetType(typeInfo);
            }
            catch
            {
                return null;
            }
        }

        public static Type ParseToType(this string typeInfo, Type baseType)
        {
            Type t = ParseToType(typeInfo);
            if (null == baseType)
            {
                return t;
            }
            return t.IsOrSubTypeOf(baseType) ? t : null;
        }

        public static Type ParseToType<T>(this string typeInfo)
        {
            return ParseToType(typeInfo, typeof(T));
        }
    }
}