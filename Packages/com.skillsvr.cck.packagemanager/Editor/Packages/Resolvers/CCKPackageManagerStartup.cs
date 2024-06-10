using SkillsVR.CCK.PackageManager.Ioc;
using System;
using System.Linq;
using UnityEditor;

namespace SkillsVR.CCK.PackageManager.Startup
{
    class CCKPackageManagerStartup
    {
        [InitializeOnLoadMethod]
        static void OnCCKPackageManagerStartup()
        {
            var binder = new CCKBinder();
            IOCContext.Default = binder;

            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes());
            typeof(InitializeIocMethodAttribute).InvokeAllOnLoadMethodsWithThisAttribute(types);
        }

        
    }
}