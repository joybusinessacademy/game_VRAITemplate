using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using UnityEngine.UIElements;
using static Props.PropComponent;
using System;

[System.Serializable]
public class PropTypeData
{
    public int dropPositionChoice;
    public int typeChoice;
    private Action<int> changedPropType;

    public PropTypeData(Action<int> changedPropType)
    {
        this.changedPropType = changedPropType;
    }

    public void Copy(PropTypeData toCopy)
    {
        dropPositionChoice = toCopy.dropPositionChoice;
        typeChoice = toCopy.typeChoice;
    }

    public VisualElement GetVisual()
    {
        Dictionary<string, int> DropLocationChoices = new Dictionary<string, int>()
        {
            { "Floor" ,0 },
            { "Wall",1 },
            { "Ceiling" , 2 },
            { "GroundOnly" , 3 },
            { "TableOnly" , 4 },
            { "Any" , 5 }
        };

        Dictionary<string, int> PropTypeChoices = new Dictionary<string, int>()
        {
            {"Static", 0 },
            {"Interactable", 1 },
            {"Socketable", 2 }
        };

        DropdownField dropposDropdown = new DropdownField("Drop Where: ", DropLocationChoices.Keys.ToList(), dropPositionChoice);
        dropposDropdown.RegisterValueChangedCallback((evt) =>
        {
            dropPositionChoice = DropLocationChoices[evt.newValue];
        });

        DropdownField propTypeDropdown = new DropdownField("PropType: ", PropTypeChoices.Keys.ToList(), typeChoice);
        propTypeDropdown.RegisterValueChangedCallback((evt) =>
        {
            typeChoice = PropTypeChoices[evt.newValue];

            if (changedPropType != null)
                changedPropType.Invoke(typeChoice);
        });


        VisualElement root = new VisualElement();

        root.Add(propTypeDropdown);
        root.Add(dropposDropdown);

        return root;
    }
}
