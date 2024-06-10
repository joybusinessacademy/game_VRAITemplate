using System;
using System.Collections.Generic;
using System.Linq;

namespace SkillsVR.Mechanic.Core
{
    public static class IMechanicProviderExtensions
	{
        public static IEnumerable<Type> GetAllMechanicTypes<BASE_TYPE>(this IMechanicProvider provider) 
        {
            return GetAllMechanicTypes(provider, typeof(BASE_TYPE));
        }


        public static IEnumerable<Type> GetAllMechanicTypes(this IMechanicProvider provider, Type baseType)
        {
            if (null == baseType)
            {
                return new List<Type>();
            }
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
                .Where(t => baseType.IsAssignableFrom(t) && !t.IsAbstract && t.IsClass);
        }
    }
}
