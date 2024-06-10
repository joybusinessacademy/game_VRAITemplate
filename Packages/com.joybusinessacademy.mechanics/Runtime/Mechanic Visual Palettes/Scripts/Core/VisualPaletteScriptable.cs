using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Mechanics/Visual Palette Config", fileName ="Visual Palette")]
public class VisualPaletteScriptable : ScriptableObject
{ 
    public List<PaletteComponentCollection> visualComponentsDetected = new List<PaletteComponentCollection>();

    private bool hasCheckedAssetDatabase;
    private void OnEnable()
    {
        if (hasCheckedAssetDatabase || visualComponentsDetected.Count!=0)
            return;

        DetectVisualsOnMechanics();
        hasCheckedAssetDatabase = true;
    }

    private void DetectVisualsOnMechanics()
    {
#if UNITY_EDITOR
        string[] assetguids = AssetDatabase.FindAssets("t:VisualPaletteLinkerScriptable");
        for (int i = 0; i < assetguids.Length; i++)
        {
            VisualPaletteLinkerScriptable next = (VisualPaletteLinkerScriptable)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assetguids[i]), typeof(VisualPaletteLinkerScriptable));
            visualComponentsDetected.Add(new PaletteComponentCollection(next.mechanicName, next.imageComponents, next.textmeshComponents, next.buttonComponents));
        }
#endif
    }
}


