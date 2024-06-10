using System.Collections.Generic;
using UnityEngine;
using System;
using SkillsVR.Mechanic.Core;
using System.Linq;
using SkillsVR.Mechanic.Core.Impl;
using SkillsVR.Mechanic.MechanicSystems.DeepBreath;
using SkillsVR.Mechanic.MechanicSystems.Gesture;
using SkillsVR.Mechanic.MechanicSystems.DeepBreath.Impl;

public class MechanicEventManager
{
    private static Dictionary<object, Type> eventParamTypeData = new Dictionary<object, Type>();

#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
#endif
    private static void InitGlobalCache()
    {
        CacheEventParams();
    }

    private static void CacheEventParams()
    {
        AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .SelectMany(t => t.GetCustomAttributes(typeof(RegisterMechanicEventParameterTypeAttribute), false))
            .Select(attr => (RegisterMechanicEventParameterTypeAttribute)attr)
            .Where(attr => null != attr.eventValue && null != attr.parameterType)
            .ToList()
            .ForEach(attr => eventParamTypeData.Add(attr.eventValue, attr.parameterType));
    }

    public static Type GetEventParameterType(object eventObject)
    {
        if (null == eventObject)
        {
            return typeof(IMechanicSystemEvent);
        }

        Type t = null;
        if (eventParamTypeData.TryGetValue(eventObject, out t))
        {
            return t;
        }
        else
        {
            return typeof(IMechanicSystemEvent);
        }
    }

    

    /// <summary>
    /// Get all events bind on this mechanic type and all its parent type/interfaces
    /// </summary>
    /// <param name="mechanicType">Target mechanic type</param>
    /// <returns>All event keys as IEnumerable object.</returns>
    public static IEnumerable<object> GetMechanicEvents(Type mechanicType)
    {
        List<object> events = new List<object>();
        if (null == mechanicType)
        {
            return events;
        }

        var interfaces = mechanicType.GetInterfaces();

        foreach(var interfaceType in interfaces)
        {
            events.AddRange(GetMechanicEventsFromType(interfaceType));
        }
        events.AddRange(GetMechanicEventsFromType(mechanicType));

        events = events.Distinct().ToList();
        return events;
    }

    public static void LogAllEventsForMechanic(Type mechanicType)
    {
        if (null == mechanicType)
        {
            return;
        }
        var items = GetMechanicEvents(mechanicType);
        string info = mechanicType.Name + " Events:\r\n";
        foreach (var item in items)
        {
            info += "  " + item.GetType().Name + "/" + item.ToString() + "  " + GetEventParameterType(item).Name + "\r\n";

        }
        Debug.Log(info);
    }

    // Only Get events bind on target type
    private static IEnumerable<object> GetMechanicEventsFromType(Type type)
    {
        List<object> events = new List<object>();
        if (null == type)
        {
            return events;
        }

        var attrList = type.GetCustomAttributes(typeof(RegisterMechanicEventAttribute), true);

        if (null == attrList)
        {
            return events;
        }

        foreach (var attrObj in attrList)
        {
            RegisterMechanicEventAttribute attr = attrObj as RegisterMechanicEventAttribute;
            if (null == attr)
            {
                continue;
            }

            List<object> attrEvents = new List<object>();

            if (null != attr.eventValue)
            {
                attrEvents.Add(attr.eventValue);
            }
            if (null != attr.enumEventType)
            {
                foreach (var item in Enum.GetValues(attr.enumEventType))
                {
                    attrEvents.Add(item);
                }
            }

            foreach (var item in attrEvents)
            {
                if (events.Contains(item))
                {
                    continue;
                }
                events.Add(item);
            }
        }

        return events;
    }

}