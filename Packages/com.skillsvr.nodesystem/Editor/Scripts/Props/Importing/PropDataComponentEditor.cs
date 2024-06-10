using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(PropGeneratorComponent))]
public class PropDataComponentEditor : Editor
{
    private PropGeneratorComponent component;

    private void OnEnable()
    {
        component = (PropGeneratorComponent)target;

        if (component.source != null)
            component.Initialise(component.source);
    }
    public override VisualElement CreateInspectorGUI()
    {
        var rootCreateGui = new VisualElement();

        rootCreateGui.Add(component.PropDetails.GetVisual());
        rootCreateGui.Add(component.PropType.GetVisual());

        return rootCreateGui;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }
}