using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class ImageComponentPalette : PaletteComponent
{
    [SerializeField] public Color color;

    public ImageComponentPalette(string id) : base(id)
    {
        color = Color.white;
    }

}
