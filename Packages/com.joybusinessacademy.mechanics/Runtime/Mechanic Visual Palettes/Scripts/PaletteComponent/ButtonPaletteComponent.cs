using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class ButtonComponentPalette : PaletteComponent
{
    [SerializeField] public Color normalColor;
    [SerializeField] public Color highlightedColor;
    [SerializeField] public Color pressedColor;
    [SerializeField] public Color selectedColor;
    [SerializeField] public Color disabledColor;

    public ButtonComponentPalette(string id) : base(id)
    {
        normalColor = Color.black;
        highlightedColor = Color.grey;
        pressedColor = Color.white;
        selectedColor = Color.white;
        disabledColor = Color.grey;
    }
}
