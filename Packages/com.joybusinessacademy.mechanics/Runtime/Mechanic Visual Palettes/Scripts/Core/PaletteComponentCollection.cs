using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PaletteComponentCollection
{
    [SVRReadOnly] public string name;
    public bool isActive;
    public List<ImageComponentPalette> imageComponents = new List<ImageComponentPalette>();
    public List<TextmeshComponentPalette> textmeshComponents = new List<TextmeshComponentPalette>();
    public List<ButtonComponentPalette> buttonComponents = new List<ButtonComponentPalette>();

    public PaletteComponentCollection(string _name, List<ImageComponentPalette> _imageComponents, List<TextmeshComponentPalette> _textmeshComponents, List<ButtonComponentPalette> _buttonComponents)
    {
        name = _name;
        imageComponents = _imageComponents;
        textmeshComponents = _textmeshComponents;
        buttonComponents = _buttonComponents;
    }
}
