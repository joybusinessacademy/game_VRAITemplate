using SkillsVRNodes.ScriptableObjects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetNodeVariable : MonoBehaviour
{
    [SerializeField] VariableSO variable;
    public void SetInt(int value)
    {
        if (variable)
        {
            if (variable as IntSO)
            {
                (variable as IntSO).value = value;
            }
        }
    }

    public void SetFloat(float value)
    {
        if (variable)
        {
            if (variable as FloatSO)
            {
                (variable as FloatSO).value = value;
            }
        }
    }
}


