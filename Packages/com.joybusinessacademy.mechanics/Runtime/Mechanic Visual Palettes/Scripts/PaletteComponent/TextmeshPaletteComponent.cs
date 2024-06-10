using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class TextmeshComponentPalette : PaletteComponent
{
    [SerializeField] public TMP_FontAsset textsFont;
    [SerializeField] public Color fontColor;

    public TextmeshComponentPalette(string id) : base(id)
    {
        fontColor = Color.black;
    }
}