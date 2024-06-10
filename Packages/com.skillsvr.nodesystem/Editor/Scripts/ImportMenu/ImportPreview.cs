using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.EventSystems;
using DG.DemiEditor;
using System.Net.Http;

public class ImportPreview 
{
    private PreviewCollider previewController;
    private GameObject prefabInstance;
    private Scene previewScene;

    private Image previewImageElement;
    private Texture2D previewTextureElement;
    private RenderTexture previewCameraRenderTexture;

    private VisualElement prefabViewElement = new VisualElement();
    private bool renderingRoutineAdded;

    private bool isActive;
    private Vector2 previewWindowSize = new Vector2(400, 400);
    private Vector3 objectPosition = new Vector3(0, 0, 0);
    private Vector3 cameraEuler = new Vector3();

    private float cameraZoom;
    private float boxMoveDampener = 0.005f;
    private float boxScaleDampener = 0.005f;
    private float rotationFlip = 1.0f;
    private bool rightClicking;
    private bool leftClicking;

    private bool showsCollider;

    Quaternion CameraRot
    {
        get
        {
            Quaternion q = new Quaternion();
            q.eulerAngles = cameraEuler;
            return q;
        }
    }

    Vector3 CameraLookAt
    {
        get
        {
            if (showsCollider)
                return new Vector3(previewController.PreviewBoxCenterX, previewController.PreviewBoxCenterY, previewController.PreviewBoxCenterZ);
            else
                return Vector3.zero;
        }
    }

    public void SetUp(GameObject setUpPrefab)
    {
        CheckIfLayerPresent("ImportRender");
        prefabInstance = setUpPrefab;
        previewScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

        prefabInstance.transform.position = objectPosition;
        prefabInstance.transform.rotation = Quaternion.identity;
        prefabInstance.transform.localScale = Vector3.one;

        EditorSceneManager.MoveGameObjectToScene(prefabInstance, previewScene);
        ResetCameraView();
    }

    private void ResetCameraView()
    {
        cameraEuler = Vector3.zero;
        cameraEuler.y = 45f;
        cameraZoom = 1.0f;


        if ((cameraEuler.y > 45 && cameraEuler.y < 225 && rotationFlip > 0) || ((cameraEuler.y < 45 || cameraEuler.y > 225) && rotationFlip < 0))
            rotationFlip *= -1f;

    }
    public void CleanUp()
    {
        if (previewController != null)
        {      
            previewController.CleanSelf();
        }
        prefabInstance = null;
        StopRendering();
        EditorSceneManager.CloseScene(previewScene, true);
        isActive = false;
    }

    public void RemovePreviewComponent()
    {
        if (previewController != null)
            previewController.CleanSelf();

        if(prefabInstance != null)
            SetLayerRecursively(prefabInstance.transform, LayerMask.NameToLayer("Default"));
  
    }

    public VisualElement StartRendering(bool showColliderPreview)
    {
        if (!renderingRoutineAdded)
        {
            EditorApplication.update += RenderUpdate;
        }

        return GetPrefabViewElement(showColliderPreview);
    }

    public void StopRendering()
    {
        if (renderingRoutineAdded)
        {
            EditorApplication.update -= RenderUpdate;
            renderingRoutineAdded = false;
        }
    }
    private VisualElement GetPrefabViewElement(bool showColliderPreview)
    {
        showsCollider = showColliderPreview;
        prefabViewElement = new VisualElement();

        previewCameraRenderTexture = RenderTexture.GetTemporary((int)previewWindowSize.x, (int)previewWindowSize.y);

        SceneView sv = GetSceneView();
        sv.Focus();
        SetUpCamera(sv.camera, showsCollider);

        previewTextureElement = new Texture2D(previewCameraRenderTexture.width, previewCameraRenderTexture.height);
        UpdatePreviewTexture();

        previewImageElement = new Image();
        previewImageElement.image = previewCameraRenderTexture;
        SetUpMouseControls(previewImageElement, sv);
        CommonSetUp(prefabViewElement, showsCollider, sv);

        isActive = true;
        return prefabViewElement;
    }

    private void RenderUpdate()
    {
        if (!isActive)
            return;

        Graphics.SetRenderTarget(previewCameraRenderTexture);
        GL.Clear(true, true, Color.clear);

        SceneView sv = GetSceneView();
        SetUpCamera(sv.camera, false); 

        UpdatePreviewTexture();

        previewImageElement.image = previewTextureElement;
        prefabViewElement.MarkDirtyRepaint();
    }

    private SceneView GetSceneView()
    {
        SceneView sv = SceneView.lastActiveSceneView;
        if (sv == null)
            sv = EditorWindow.GetWindow<SceneView>("Scene", true);

        
        return sv;
    }

    private void SetUpCamera(Camera camera, bool showsCollider)
    {
        int importRenderLayer = LayerMask.NameToLayer("ImportRender");
        if (importRenderLayer != -1)
        {
            int importRenderLayerMask = 1 << importRenderLayer;
            camera.cullingMask = importRenderLayerMask;
            //prefabInstance.layer = importRenderLayer;
            SetLayerRecursively(prefabInstance.transform, importRenderLayer);
        }

        camera.targetTexture = previewCameraRenderTexture;
        camera.Render();

        if (showsCollider)
        {
            previewController = prefabInstance.AddComponent<PreviewCollider>();
        }
    }

    private void UpdatePreviewTexture()
    {
        RenderTexture.active = previewCameraRenderTexture;
        previewTextureElement.ReadPixels(new Rect(0, 0, previewCameraRenderTexture.width, previewCameraRenderTexture.height), 0, 0);
        previewTextureElement.Apply();
    }


    private void CommonSetUp(VisualElement prefabViewElement, bool showsCollider, SceneView sv)
    {
       
        prefabViewElement.Add(previewImageElement);

        prefabViewElement.Add(new Button(() =>
        {
            ResetCameraView();
            sv.LookAt(CameraLookAt, CameraRot, cameraZoom);
        })
        {
            text = "RESET CAMERA"
        });

        if (showsCollider)
        {
            prefabViewElement.Add(new Button(() =>
            {
                previewController.ResetBox();
                ResetCameraView();
                sv.LookAt(CameraLookAt, CameraRot, cameraZoom);
            })
            {
                text = "RESET BOX"
            });
        }

        string s = "RIGHT CLICK - Move Camera\n";
        if (showsCollider)
        {
            s += "LEFT CLICK - Move Box\n" +
            "LEFT CLICK + CTRL - Scale Box\n";
        }

        prefabViewElement.Add(new Label()
        {
            text = s
        }) ;

        sv.LookAt(CameraLookAt, CameraRot, cameraZoom);
    }
    private void SetUpMouseControls(Image previewImageElement, SceneView sv)
    {
        previewImageElement.RegisterCallback<MouseDownEvent>((evt) =>
        {
            if (evt.button == 1)
            {
                rightClicking = true;
            }
            else if (evt.button == 0)
            {
                leftClicking = true;
            }
        });

        previewImageElement.RegisterCallback<MouseUpEvent>((evt) =>
        {

            if (evt.button == 1)
                rightClicking = false;
            else if (evt.button == 0)
            {
                leftClicking = false;

                if (showsCollider)
                {
                   // ResetCameraView();
                    sv.LookAt(CameraLookAt, CameraRot, cameraZoom);
                }
            }
        });



        previewImageElement.RegisterCallback<MouseMoveEvent>((evt) =>
        {
            
            if (rightClicking)
            {
                if ((cameraEuler.y > 45 && cameraEuler.y < 225 && rotationFlip > 0) || ((cameraEuler.y < 45 || cameraEuler.y > 225) && rotationFlip < 0))
                    rotationFlip *= -1f;

                if (!evt.actionKey)
                {
                    cameraEuler.y += evt.mouseDelta.x;
                    if (cameraEuler.y > 360f)
                        cameraEuler.y = 0.1f;

                    if (cameraEuler.y < 0)
                        cameraEuler.y = 359.9f;

                    cameraEuler.x += evt.mouseDelta.y;
                    if (cameraEuler.x > 85f)
                        cameraEuler.x = 85f;

                    if (cameraEuler.x < -20)
                        cameraEuler.x = -20f;

                    sv.LookAt(CameraLookAt, CameraRot, cameraZoom);
                }
            }
            if (showsCollider)
            {
                if (leftClicking)
                {
                    if (!evt.actionKey)
                    {
                        if (cameraEuler.y > 45 && cameraEuler.y < 135 || cameraEuler.y > 225 && cameraEuler.y < 315)
                        {
                            previewController.PreviewBoxCenterZ += ((evt.mouseDelta.x * boxMoveDampener) * sv.cameraDistance * rotationFlip);
                        }
                        else
                        {
                            previewController.PreviewBoxCenterX += ((evt.mouseDelta.x * boxMoveDampener) * sv.cameraDistance * rotationFlip);
                        }

                        previewController.PreviewBoxCenterY += ((evt.mouseDelta.y * -boxMoveDampener) * sv.cameraDistance);
                    }
                    else
                    {

                        if (cameraEuler.y > 45 && cameraEuler.y < 135 || cameraEuler.y > 225 && cameraEuler.y < 315)
                        {
                            previewController.PreviewMeshScaleZ += ((evt.mouseDelta.x * boxScaleDampener) * sv.cameraDistance);
                            previewController.PreviewMeshScaleZ = Mathf.Clamp(previewController.PreviewMeshScaleZ, 0, 10);
                        }
                        else
                        {
                            previewController.PreviewMeshScaleX += ((evt.mouseDelta.x * boxScaleDampener) * sv.cameraDistance);
                            previewController.PreviewMeshScaleX = Mathf.Clamp(previewController.PreviewMeshScaleX, 0, 10);
                        }

                        previewController.PreviewMeshScaleY += ((evt.mouseDelta.y * -boxScaleDampener) * sv.cameraDistance);
                        previewController.PreviewMeshScaleY = Mathf.Clamp(previewController.PreviewMeshScaleY, 0, 10);
                    }
                }
            }
        });



        previewImageElement.RegisterCallback<WheelEvent>((evt) =>
        {

            cameraZoom = (evt.delta.y > 0) ? cameraZoom + 0.1f : cameraZoom - 0.1f;

            sv.LookAt(CameraLookAt, CameraRot, cameraZoom);

            Mathf.Clamp(cameraZoom, 0.1f, 10);
        });

        previewImageElement.RegisterCallback<MouseLeaveEvent>((evt) =>
        {
            leftClicking = false;
            rightClicking = false;
        });
    }

    void SetLayerRecursively(Transform obj, int layer)
    {
        obj.gameObject.layer = layer; 

        foreach (Transform child in obj)
        {
            SetLayerRecursively(child, layer);
        }
    }

    void CheckIfLayerPresent(string layername)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");

        int firstEmptyIndex = -1;

        foreach (SerializedProperty layer in layers) {
            if(!string.IsNullOrEmpty(layer.stringValue) )
            {
                if(layer.stringValue == layername)
                {
                    //found the layer, return;
                    return;
                }
            }
            else if(firstEmptyIndex < 0)
            {
                //first usable index if not found
                firstEmptyIndex = layer.GetIndexInArray();
            }
        }

        //didn't find, need to add
        if(firstEmptyIndex > 0)
        {
            SerializedProperty nextLayer = layers.GetArrayElementAtIndex(firstEmptyIndex);
            nextLayer.stringValue = layername;
            tagManager.ApplyModifiedProperties();
        }
        else
        {
            Debug.LogWarning("Couldn't add a unity layer for the import preview");
        }
       
    }

}