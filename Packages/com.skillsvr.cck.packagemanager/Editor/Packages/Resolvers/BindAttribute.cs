using System;

namespace SkillsVR.CCK.PackageManager.Ioc
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class BindAttribute : System.Attribute
    {
        public Type TypeKey { get; set; }
        public string[] StringKeys { get; set; } 

        public BindAttribute(params string[] keys)
        {
            StringKeys = keys;
        }

        public BindAttribute(Type keyType, params string[] keys)
        {
            TypeKey = keyType;
            StringKeys = keys;
        }
    }
}