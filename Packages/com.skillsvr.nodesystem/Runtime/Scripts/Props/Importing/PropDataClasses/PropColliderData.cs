using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PropColliderData
{
    [SerializeField] private Vector3 boxCenter;
    [SerializeField] private Vector3 boxSize;

    private bool shouldLoad = false;

    public void CreateInteractableColliderData(GameObject targetPrefab, InteractableProp prop)
    {
        GameObject colliderParent = new GameObject("Collider");
        colliderParent.transform.parent = targetPrefab.transform;


        prop.collider = colliderParent.AddComponent<BoxCollider>();

        //Bounds meshBounds = targetPrefab.GetComponentsInChildren<MeshFilter>()[0].sharedMesh.bounds;
        MeshFilter[] allfilters = targetPrefab.GetComponentsInChildren<MeshFilter>();

        
       
        CombineInstance[] combine = new CombineInstance[allfilters.Length];

        for (int i = 0; i < allfilters.Length; i++)
        {
            combine[i].mesh = allfilters[i].sharedMesh;
            combine[i].transform = allfilters[i].transform.localToWorldMatrix;
        }

        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combine);
        Bounds meshBounds = mesh.bounds;



        BoxCollider boxCollider = prop.collider as BoxCollider;
        if (shouldLoad)
        {
            boxCollider.size = boxSize;
            boxCollider.center = boxCenter;
        }
        else
        {

            boxCollider.size = meshBounds.size;
            boxCollider.center = meshBounds.center;
            boxSize = meshBounds.size;
            boxCenter = meshBounds.center;

        }
        prop.collider = boxCollider;
        shouldLoad = true;
    }

    public void CreateSocketColliderData(GameObject targetPrefab, SocketProp prop)
    {
        prop.collider = targetPrefab.AddComponent<BoxCollider>();
        //Bounds meshBounds = targetPrefab.GetComponent<MeshFilter>().sharedMesh.bounds;

        MeshFilter[] allfilters = targetPrefab.GetComponentsInChildren<MeshFilter>();

        CombineInstance[] combine = new CombineInstance[allfilters.Length];

        for (int i = 0; i < allfilters.Length; i++)
        {
            combine[i].mesh = allfilters[i].sharedMesh;
            combine[i].transform = allfilters[i].transform.localToWorldMatrix;
        }

        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combine);
        Bounds meshBounds = mesh.bounds;

        BoxCollider boxCollider = prop.collider as BoxCollider;

        if (shouldLoad)
        {
            boxCollider.size = boxSize;
            boxCollider.center = boxCenter;
        }
        else
        {

            boxCollider.size = meshBounds.size;
            boxCollider.center = meshBounds.center;
            boxSize = meshBounds.size;
            boxCenter = meshBounds.center;

        }

        prop.collider = boxCollider;
        prop.collider.isTrigger = true;
        prop.socket.attachTransform.position = boxCollider.bounds.center;
        shouldLoad = true;
    }

    public void SetColliderData(Vector3 size, Vector3 center)
    {
        boxSize = size;
        boxCenter = center;
    }
}
