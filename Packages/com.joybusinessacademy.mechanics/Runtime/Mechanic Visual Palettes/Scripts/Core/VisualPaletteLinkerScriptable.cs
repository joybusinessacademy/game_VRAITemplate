using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;



public class VisualPaletteLinkerScriptable : ScriptableObject
{
    [SVRReadOnly]
    public string mechanicName;
    public List<ImageComponentPalette> imageComponents = new List<ImageComponentPalette>();
    public List<TextmeshComponentPalette> textmeshComponents = new List<TextmeshComponentPalette>();
    public List<ButtonComponentPalette> buttonComponents = new List<ButtonComponentPalette>();

    public void Add(PaletteListenerPair graphicIdPair)
    {
        if (graphicIdPair.graphicComponent as Button)
        {
            buttonComponents.Add(new ButtonComponentPalette( graphicIdPair.idKey) );
        }
        else if (graphicIdPair.graphicComponent as Image || graphicIdPair.graphicComponent as RawImage)
        {
            imageComponents.Add(new ImageComponentPalette(graphicIdPair.idKey) );
        }
        else if (graphicIdPair.graphicComponent as TextMeshProUGUI)
        {
            textmeshComponents.Add(new TextmeshComponentPalette(graphicIdPair.idKey ));
        }
    }
}

