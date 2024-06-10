using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using UnityEngine.UIElements;
using System;

[System.Serializable]
public class PropDetailData 
{
    [SerializeField]   public string name;
    [SerializeField]   public string description;

    private Action<bool> allValuesSetConfirmed;

    public PropDetailData(Action<bool> _allValuesSetConfirmed)
    {
        this.allValuesSetConfirmed = _allValuesSetConfirmed;
    }
    public void Copy ( PropDetailData toCopy)
    {
        name = toCopy.name;
        description = toCopy.description;
        CheckForTrigger(true);
    }

    public VisualElement GetVisual()
    {

        var nameinput = new TextField("Prop Name: ");
        {
            nameinput.value = name;
        }
        nameinput.RegisterValueChangedCallback((evt) =>
        {
            name = evt.newValue;
            CheckForTrigger(true);
        });

        var descriptionInput = new TextField("Prop Description: ");
        {
            descriptionInput.value = description;
        }
        descriptionInput.RegisterValueChangedCallback((evt) =>
        {
            description = evt.newValue;
            CheckForTrigger(true);
        });

        VisualElement root = new VisualElement();

        root.Add(nameinput);
        root.Add(descriptionInput);
        return root;
    }

    private void CheckForTrigger(bool val)
    {
        if (!name.IsNullOrEmpty() && !description.IsNullOrEmpty())
        {
            allValuesSetConfirmed.Invoke(val);
        }
    }
}
