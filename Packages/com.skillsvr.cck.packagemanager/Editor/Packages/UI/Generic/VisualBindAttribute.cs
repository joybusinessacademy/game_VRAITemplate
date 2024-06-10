using System;

namespace SkillsVR.CCK.PackageManager.UI
{
    /// <summary>
    /// Mark a property or field bind to a visual element with name. 
    /// If no special name allocated, the binding will try follow names in order:
    /// - uss style name from member name, which replace _ to -, and any upper letter to - and lower letter. ie.e _some_ABC to some-a-b-c
    /// - original member name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class VisualBindAttribute :  Attribute
    {
        public string Name { get; set; }

        public VisualBindAttribute(string elementName = null)
        {
            Name = elementName;
        }
    }
}