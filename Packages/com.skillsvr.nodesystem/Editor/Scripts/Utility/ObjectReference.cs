using System.Collections.Generic;
using SkillsVRNodes.Managers.Utility;

namespace Scripts.Utility
{
    public class ObjectReference
    {
        public string GUID;
        public string Path;
        public string Name;
        /// <summary>
        /// Null or empty is package
        /// </summary>
        public string attachedProject;
        public List<string> labels = new();
    
        public bool IsPackage => attachedProject.IsNullOrWhitespace();
        public bool IsProject => GraphFinder.CurrentActiveProject?.GetProjectName == attachedProject;
    }
}
