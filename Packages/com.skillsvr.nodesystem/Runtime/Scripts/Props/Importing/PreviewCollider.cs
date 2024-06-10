using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
public class PreviewCollider : MonoBehaviour 
{
    public Material mat;
    BoxCollider col;
    GameObject cube;

    private Vector3 previewMeshScale = new Vector3(1, 1, 1);
    public float PreviewMeshScaleX { get => previewMeshScale.x; set => previewMeshScale.x = value; }
    public float PreviewMeshScaleY { get => previewMeshScale.y; set => previewMeshScale.y = value; }
    public float PreviewMeshScaleZ { get => previewMeshScale.z; set => previewMeshScale.z = value; }

    private Vector3 previewBoxCenter = Vector3.zero;
    public float PreviewBoxCenterX { get => previewBoxCenter.x; set => previewBoxCenter.x = value; }
    public float PreviewBoxCenterY { get => previewBoxCenter.y; set => previewBoxCenter.y = value; }
    public float PreviewBoxCenterZ { get => previewBoxCenter.z; set => previewBoxCenter.z = value; }

    private Vector3 originalSize, originalCenter;

    private void OnEnable()
    {
        col = GetComponentInChildren<BoxCollider>();
        if (col)
        {
            cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

            previewMeshScale = col.size;
            originalSize = col.size;
            previewBoxCenter = transform.position + col.center;
            originalCenter = transform.position + col.center;

            var guid = UnityEditor.AssetDatabase.FindAssets("t:Material ColliderGreen").FirstOrDefault();
            mat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
            cube.GetComponent<MeshRenderer>().material = mat;
            cube.transform.parent = transform.parent;
            cube.layer = LayerMask.NameToLayer("ImportRender");
            cube.transform.position = previewBoxCenter;
            cube.transform.localScale = previewMeshScale;
        }
    }

    public void CleanSelf()
    {
        if (cube)
            DestroyImmediate(cube);
        DestroyImmediate(this);
    }

    private void Update()
    {
        if (col != null && cube != null)
        {
            cube.transform.position = previewBoxCenter;
            cube.transform.localRotation = transform.rotation;
            cube.transform.localScale = previewMeshScale;

            col.center = col.transform.InverseTransformPoint(cube.transform.position);
            col.size = cube.transform.localScale;
            col.transform.rotation = cube.transform.rotation;
        }

    }

    public void ResetBox()
    {
        col.size = originalSize;
        col.center = originalCenter;

        previewMeshScale = col.size;
        previewBoxCenter = transform.position + col.center;
    }
}
#endif