using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEditor;

[System.Serializable]
public class PaletteListenerPair
{
    public string idKey;
    public UIBehaviour graphicComponent;
}


public class VisualPaletteListener : MonoBehaviour
{
    [SerializeField] private string mechanicName;
    [SerializeField] List<PaletteListenerPair> mechanicGraphics = new List<PaletteListenerPair>();

    private void Awake()
    {
        ApplyPallete();
    }

    public void ApplyPallete()
    {

        if (!VerifyMechanic(out PaletteComponentCollection componentPaletteCollection))
            return;

        for (int i = 0; i < mechanicGraphics.Count; i++)
        {

            if (mechanicGraphics[i].graphicComponent as Button)
            {
                ApplyPaletteToButton(componentPaletteCollection, mechanicGraphics[i].graphicComponent as Button, i);
            }

            if (mechanicGraphics[i].graphicComponent as Image)
            {
                ApplyPaletteToImage(componentPaletteCollection, mechanicGraphics[i].graphicComponent as Image, i);
            }
            else if (mechanicGraphics[i].graphicComponent as RawImage)
            {
                ApplyPaletteToRawImage(componentPaletteCollection, mechanicGraphics[i].graphicComponent as RawImage, i);
            }

            if (mechanicGraphics[i].graphicComponent as TextMeshProUGUI)
            {
                ApplyPaletteToText(componentPaletteCollection, mechanicGraphics[i].graphicComponent as TextMeshProUGUI, i);
            }
        }
    }

    private bool VerifyMechanic(out PaletteComponentCollection componentPaletteCollection)
    {
        componentPaletteCollection = null;

        if (MechanicsManager.Instance == null || MechanicsManager.Instance.visualPaletteScriptable == null)
            return false;

        componentPaletteCollection = MechanicsManager.Instance.visualPaletteScriptable.visualComponentsDetected.Find(element => element.name == mechanicName);

        if (componentPaletteCollection == null || !componentPaletteCollection.isActive)
            return false;

        return true;
    }


    private void ApplyPaletteToButton(PaletteComponentCollection componentPaletteCollection, Button component, int index)
    {
        ButtonComponentPalette paletteComponent =
            componentPaletteCollection.buttonComponents[componentPaletteCollection.buttonComponents.FindIndex(id => id.keyId == mechanicGraphics[index].idKey)] as ButtonComponentPalette;
        component.colors = new ColorBlock()
        {
            normalColor = paletteComponent.normalColor,
            highlightedColor = paletteComponent.highlightedColor,
            pressedColor = paletteComponent.pressedColor,
            selectedColor = paletteComponent.selectedColor,
            disabledColor = paletteComponent.disabledColor,
            colorMultiplier = 1
        };
    }

    private void ApplyPaletteToImage(PaletteComponentCollection componentPaletteCollection, Image component, int index)
    {
        ImageComponentPalette paletteComponent = componentPaletteCollection.imageComponents[componentPaletteCollection.imageComponents.FindIndex(id => id.keyId == mechanicGraphics[index].idKey)] as ImageComponentPalette;
        component.color = paletteComponent.color;
    }

    private void ApplyPaletteToRawImage(PaletteComponentCollection componentPaletteCollection, RawImage component, int index)
    {
        ImageComponentPalette paletteComponent = componentPaletteCollection.imageComponents[componentPaletteCollection.imageComponents.FindIndex(id => id.keyId == mechanicGraphics[index].idKey)] as ImageComponentPalette;
        component.color = paletteComponent.color;
    }

    private void ApplyPaletteToText(PaletteComponentCollection componentPaletteCollection, TextMeshProUGUI component, int index)
    {
        TextmeshComponentPalette paletteComponent = componentPaletteCollection.textmeshComponents[componentPaletteCollection.textmeshComponents.FindIndex(id => id.keyId == mechanicGraphics[index].idKey)] as TextmeshComponentPalette;
        component.color = paletteComponent.fontColor;
        component.font = paletteComponent.textsFont;
    }

#if UNITY_EDITOR
    private static string CONFIG_EXPORT_PATH = "Assets/VisualPaletteListenerAssets";
    public void GenerateScriptable()
    {

        VisualPaletteLinkerScriptable idLinker = ScriptableObject.CreateInstance<VisualPaletteLinkerScriptable>();
        EditorUtility.SetDirty(idLinker);
        if (AssetDatabase.IsValidFolder(CONFIG_EXPORT_PATH) == false)
            AssetDatabase.CreateFolder("Assets", CONFIG_EXPORT_PATH.Replace("Assets/", string.Empty));


        for (int i = 0; i < mechanicGraphics.Count; i++)
        {
            idLinker.Add(mechanicGraphics[i]);
        }

        idLinker.mechanicName = mechanicName;

        AssetDatabase.CreateAsset(idLinker, CONFIG_EXPORT_PATH + "/" + mechanicName + ".asset");
    }
#endif
}
