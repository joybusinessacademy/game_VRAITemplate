using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SocketableTag : MonoBehaviour
{
    public TagSO tag;
    private void Awake()
    {
        tag = null;
    }
}
