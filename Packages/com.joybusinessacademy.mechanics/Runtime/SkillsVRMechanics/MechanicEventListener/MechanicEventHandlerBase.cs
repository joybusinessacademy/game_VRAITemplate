using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VRMechanics;
using System.Linq;
using SkillsVR.Mechanic.Core;
using SkillsVR.Serialization;
using SkillsVR.Mechanic.Events;
using SkillsVR.OdinPlaceholder;

namespace SkillsVR.Mechanic.Events
{

	[Serializable]
	public abstract class MechanicEventHandlerBase
	{
		[ValueDropdown(nameof(GetMechanicTypes))]
		public SerializableType mechanicType;
		
		[NonSerialized]
		[ValueDropdown(nameof(GetKeys))]

		[ShowInInspector]
		[FoldoutGroup("Add")]
		public object keyToAdd;

		[NonSerialized]
		[ValueDropdown(nameof(GetArgu))]

		[ShowInInspector]
		[FoldoutGroup("Add")]
		public SerializableType arguType;

		protected virtual List<IMechanicEventListBase> allLists { get; }

		public void Invoke(IMechanicSystemEvent sysEvent)
		{
			if (null == sysEvent || null == sysEvent.eventKey)
			{
				return;
			}
			allLists.ForEach(x => x.Invoke(sysEvent));
		}


		protected void Add()
		{
			if (null == keyToAdd)
			{
				return;
			}
			object eventKey = keyToAdd;
			if (null != eventKey as IValueDropdownItem)
			{
				eventKey = ((IValueDropdownItem)(eventKey)).GetValue();
			}

			if (null == eventKey)
			{
				return;
			}

			if (null == arguType)
			{
				return;
			}

			var list = allLists.Where(x => null != x && x.ArguType == arguType && !x.Contains(eventKey)).FirstOrDefault();
			if (null != list)
			{
				list.Add(eventKey);
			}

		}

		protected void OnKeyChanged()
		{
			arguType = GetArgu().FirstOrDefault().Value;
		}

		protected ValueDropdownList<object> GetKeys()
		{
			ValueDropdownList<object> list = new ValueDropdownList<object>();

			var events = MechanicEventManager.GetMechanicEvents(mechanicType);
			foreach (var item in events)
			{
				list.Add(new ValueDropdownItem<object>(item.GetType().Name + "/" + item.ToString(), item));
			}

			return list;
		}

		protected ValueDropdownList<SerializableType> GetArgu()
		{
			ValueDropdownList<SerializableType> list = new ValueDropdownList<SerializableType>();

			object key = keyToAdd;
			if (null != key as IValueDropdownItem)
			{
				key = ((IValueDropdownItem)(keyToAdd)).GetValue();
			}
			var t = MechanicEventManager.GetEventParameterType(key);

			if (null != t)
			{
				list.Add(new ValueDropdownItem<SerializableType>(t.Name, new SerializableType(t)));
			}
			if (typeof(IMechanicSystemEvent) != t)
			{
				list.Add(new ValueDropdownItem<SerializableType>(nameof(IMechanicSystemEvent), new SerializableType(typeof(IMechanicSystemEvent))));
			}


			return list;
		}

		protected ValueDropdownList<SerializableType> GetMechanicTypes()
		{
			ValueDropdownList<SerializableType> list = new ValueDropdownList<SerializableType>();

			var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
				.Where(t => typeof(IMechanicSystem).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);

			foreach (var type in types)
			{
				string prefix = "MechanicSystem/";
				if (typeof(IMechanicSpawner).IsAssignableFrom(type))
				{
					prefix = "Spawners/";
				}
				list.Add(new ValueDropdownItem<SerializableType>(prefix + type.Name, new SerializableType(type)));
			}

			return list;
		}
	}
}