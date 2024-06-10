using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace SkillsVR.TimelineTool.Editor
{
	public class AdvancedDropdownT<T> : AdvancedDropdown
	{
		public class ItemT : AdvancedDropdownItem
		{
			public T data;

			public ItemT(T rawData, string name = null) : base(name)
			{
				data = rawData;
			}
		}


		public event Action<T, AdvancedDropdownItem> onItemSelected;
		public Func<string> GetLabel;
		public Func<T, string> GetItemNameFromData;
		public Action<AdvancedDropdownItem, IEnumerable<T>> BuildCustomList;

		public int displayLineCount = 10;
		public bool simplizeGroup = true;
		public bool enableNullItem = true;

		protected List<T> rawDataList = new List<T>();

		public AdvancedDropdownT(AdvancedDropdownState state) : base(state)
		{
			SetupSize(displayLineCount);
		}

		public AdvancedDropdownT(IEnumerable<T> rawData) : base(new AdvancedDropdownState())
		{
			SetupSize(displayLineCount);
			if (null != rawData)
			{
				rawDataList = rawData.ToList();
			}
		}

		public AdvancedDropdownT(IEnumerable<T> rawData, AdvancedDropdownState state) : base(state)
		{
			SetupSize(displayLineCount);
			if (null != rawData)
			{
				rawDataList = rawData.ToList();
			}
		}

		protected virtual T GetItemData(AdvancedDropdownItem item, T defaultValue)
		{
			if (null == item)
			{
				return defaultValue;
			}

			var itemT = item as ItemT;
			if (null != itemT)
			{
				return itemT.data;
			}

			return defaultValue;
		}

		protected override AdvancedDropdownItem BuildRoot()
		{
			SetupSize(displayLineCount);
			var root = new AdvancedDropdownItem(null == GetLabel ? GetDefaultLabel() : GetLabel.Invoke());

			var buildFunc = null == BuildCustomList ? DefaultBuildList : BuildCustomList;
			buildFunc(root, rawDataList);

			return root;
		}

		protected void DefaultBuildList(AdvancedDropdownItem root, IEnumerable<T> dataSource)
		{
			if (enableNullItem)
			{
				root.AddChild(new ItemT(default(T), "null"));
			}
			if (null == dataSource)
			{
				return;
			}

			AdvancedDropdownItem dataNodeRoot = new AdvancedDropdownItem("");
			foreach (var data in dataSource)
			{
				string name = GetItemName(data).Replace("/", "\\");
				var path = name.Split("\\");
				var parentNode = dataNodeRoot;
				int nodeDepth = 1;
				foreach (var nodeName in path)
				{
					AdvancedDropdownItem node = null;
					if (null != parentNode.children)
					{
						node = parentNode.children.Where(x => x.name == nodeName).FirstOrDefault();
					}

					if (null == node)
					{
						node = nodeDepth == path.Length ? new ItemT(data, nodeName) : new AdvancedDropdownItem(nodeName);
						parentNode.AddChild(node);
					}
					parentNode = node;
					++nodeDepth;
				}
			}
			if (simplizeGroup && dataNodeRoot.children.Count() == 1)
			{
				foreach (var dataChildren in dataNodeRoot.children)
				{
					if (dataChildren.children.Count() == 0)
					{
						root.AddChild(dataChildren);
					}

					foreach (var item in dataChildren.children)
					{
						root.AddChild(item);
					}
				}
			}
			else
			{
				foreach (var item in dataNodeRoot.children)
				{
					root.AddChild(item);
				}
			}
		}


		protected void SetupSize(int lines)
		{
			var size = minimumSize;
			size.y = Mathf.Max(3, lines) * EditorGUIUtility.singleLineHeight;
			minimumSize = size;
		}

		protected override void ItemSelected(AdvancedDropdownItem item)
		{
			onItemSelected?.Invoke(GetItemData(item, default(T)), item);
		}

		protected string GetDefaultLabel()
		{
			return "Advanced Dropdown " + typeof(T).Name;
		}

		protected string GetItemName(T data)
		{
			string name = null == GetItemNameFromData ? null : GetItemNameFromData.Invoke(data);
			name = string.IsNullOrEmpty(name) ? (null == data ? "<null>" : data.ToString()) : name;
			return name;
		}
	}
}
