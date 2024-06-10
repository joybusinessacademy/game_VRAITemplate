using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CustomObjectMovement))]
public class CustomObjectMovementGizmo : Editor
{
    private void OnSceneGUI()
    {
        Tools.hidden = true; 

        Transform transform = Selection.activeTransform;

        if (transform == null)
            return;

        EditorGUI.BeginChangeCheck();
        Vector3 newPosition = Handles.PositionHandle(transform.position, transform.rotation);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(transform, "Move Object");
            transform.position = newPosition;
        }
    }
}