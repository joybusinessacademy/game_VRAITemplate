using Props;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropDataScriptable : ScriptableObject
{

    public SocketPropComponentData socketProp;

    public InteractablePropComponentData intertactableProp;

    public PropDetailData propDetails;
    public PropTypeData propType;
    public PropColliderData colliderData;

    public string assetDatabaseObjectPath;
    public GameObject propPrefab;
   
    public void HardCopy(PropDataScriptable toCopy)
    {
        this.socketProp = toCopy.socketProp;
        this.intertactableProp = toCopy.intertactableProp;
        this.propDetails = toCopy.propDetails;
        this.propType = toCopy.propType;
        this.colliderData = toCopy.colliderData;
        this.assetDatabaseObjectPath = toCopy.assetDatabaseObjectPath;
        this.propPrefab = toCopy.propPrefab;
    }
}
