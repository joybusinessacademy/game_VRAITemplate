using System;

namespace SkillsVR.OdinPlaceholder
{
	public class FoldoutGroupAttribute : Attribute
	{
		public FoldoutGroupAttribute(string groupName, float order = 0) { }
		public FoldoutGroupAttribute(string groupName, bool expanded, float order = 0) { }
	}


	public class ShowInInspectorAttribute : Attribute
	{
	}

	public class ReadOnlyAttribute : System.Attribute
	{
	}

	public class BoxGroupAttribute :Attribute
	{
		public BoxGroupAttribute() { }
		public BoxGroupAttribute(string group, bool showLabel = true, bool centerLabel = false, float order = 0) { }
	}

	public class ButtonAttribute : Attribute
	{
		public ButtonAttribute() { }
		public ButtonAttribute(string name) { }
	}

	public class ShowIfGroupAttribute : Attribute
	{
		public ShowIfGroupAttribute(string path, bool animate = true) { }
		public ShowIfGroupAttribute(string path, object value, bool animate = true) {}
	}
}