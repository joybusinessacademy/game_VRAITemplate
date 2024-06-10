using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SkillsVR.EventExtenstion;
using SkillsVR.OdinPlaceholder; 

public class SelectionStyleEvents : MonoBehaviour
{

    public bool enableCanSelect = false;
    [ShowIfGroup(nameof(enableCanSelect))]
    [FoldoutGroup(nameof(enableCanSelect) + "/CanSelectEvents")]
    public UnityEventBool OnCanSelect = new UnityEventBool();
    [FoldoutGroup(nameof(enableCanSelect) + "/CanSelectEvents")]
    [ShowIfGroup(nameof(enableCanSelect))]
    [Button]
    public void TriggerCanSelectEvent(bool canSelect)
    {
        if (!enableCanSelect)
        {
            return;
        }
        OnCanSelect?.Invoke(canSelect);
    }


    public bool enableAlphaChange = false;
    [ShowIfGroup(nameof(enableAlphaChange))]
    [FoldoutGroup(nameof(enableAlphaChange) + "/AlphaEvents")]
    public UnityEventFloat OnAlphaChanged = new UnityEventFloat();
    [ShowIfGroup(nameof(enableAlphaChange))]
    [FoldoutGroup(nameof(enableAlphaChange) + "/AlphaEvents")]
    [Button]
    public void TriggerAlphaEvent(float alpha)
    {
        if (!enableAlphaChange)
        {
            return;
        }
        OnAlphaChanged?.Invoke(alpha);
    }


    [System.Serializable]
    public class UnityEventColor : UnityEngine.Events.UnityEvent<Color> { }

    public bool enableColorChange = false;
    [ShowIfGroup(nameof(enableColorChange))]
    [FoldoutGroup(nameof(enableColorChange) + "/ColorEvents")]
    public Color normalColor = Color.white;
    [ShowIfGroup(nameof(enableColorChange))]
    [FoldoutGroup(nameof(enableColorChange) + "/ColorEvents")]
    public Color selectColor = Color.white;
    [ShowIfGroup(nameof(enableColorChange))]
    [FoldoutGroup(nameof(enableColorChange) + "/ColorEvents")]
    public UnityEventColor OnColorChanged = new UnityEventColor();
    [ShowIfGroup(nameof(enableColorChange))]
    [FoldoutGroup(nameof(enableColorChange) + "/ColorEvents")]
    [Button]
    public void TriggerSelectColorEvent(bool select)
    {
        if (!enableColorChange)
        {
            return;
        }
        OnColorChanged?.Invoke(select ? selectColor : normalColor);
    }


    public bool enableBorderColorChange = false;
    [ShowIfGroup(nameof(enableBorderColorChange))]
    [FoldoutGroup(nameof(enableBorderColorChange) + "/BorderColorEvents")]
    public Color normalBorderColor = Color.white;
    [ShowIfGroup(nameof(enableBorderColorChange))]
    [FoldoutGroup(nameof(enableBorderColorChange) + "/BorderColorEvents")]
    public Color selectBorderColor = Color.white;
    [ShowIfGroup(nameof(enableBorderColorChange))]
    [FoldoutGroup(nameof(enableBorderColorChange) + "/BorderColorEvents")]
    public UnityEventColor OnBorderColorChanged = new UnityEventColor();
    [ShowIfGroup(nameof(enableBorderColorChange))]
    [FoldoutGroup(nameof(enableBorderColorChange) + "/BorderColorEvents")]
    [Button]
    public void TriggerSelectBorderColorEvent(bool select)
    {
        if (!enableBorderColorChange)
        {
            return;
        }
        OnBorderColorChanged?.Invoke(select ? selectBorderColor : normalBorderColor);
    }







}