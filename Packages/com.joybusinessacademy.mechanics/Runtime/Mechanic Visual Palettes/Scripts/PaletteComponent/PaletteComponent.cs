using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class PaletteComponent
{
    [SVRReadOnly] [SerializeField] public string keyId;

    public PaletteComponent(string id)
    {
        keyId = id;
    }
}
