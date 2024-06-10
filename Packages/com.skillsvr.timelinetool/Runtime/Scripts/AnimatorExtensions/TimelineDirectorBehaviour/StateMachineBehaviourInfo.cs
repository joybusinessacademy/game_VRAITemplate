
using System;
using UnityEngine;

[Serializable]
public class StateMachineBehaviourInfo
{
    public string stateName;
    public int stateHash;
    public string layerName;
    public int layerIndex;
    public string animationControllerName;
    public string animationControllerPath;

    public string ToString(string splitor)
    {
        splitor = null == splitor ? " " : splitor;
        return string.Join(splitor,
            (nameof(stateName) + ": " + stateName),
            (nameof(stateHash) + ": " + stateHash),
            (nameof(layerName) + ": " + layerName),
            (nameof(layerIndex) + ": " + layerIndex),
            (nameof(animationControllerName) + ": " + animationControllerName),
            (nameof(animationControllerPath) + ": " + animationControllerPath)
            );
    }

    public override string ToString()
    {
        return string.Join(" ", "State", stateName, "Layer", layerIndex, layerName, "AnimationController", animationControllerName, animationControllerPath);
    }
}
