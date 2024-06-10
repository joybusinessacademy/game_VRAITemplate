using Props;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PropImportModule : VisualElement
{
    public string ScriptablePath => prefabPath + "Scriptables";
    public string scriptableName;
    public string PrefabPath => prefabPath + objectName + ".prefab";
    string prefabPath = "";
    string objectName = "";

    GameObject assetDatabaseObject;
    GameObject instatiatedInSceneObject;

    VisualElement specificTypeContent = new VisualElement();
    VisualElement previewContent;
    Button confirmButton;

    private ImportPreview previewClass;
    private Action OnConfirm;

    private PropDataScriptable dataScriptable;
    private PropGeneratorComponent generatorComponent;

    private bool completedImport;
    private bool createdVariant;

    public PropImportModule(string path, VisualElement previewParent, PropDataScriptable propAsset, Action _OnConfirm = null)
    {
        if (!Validate(path))
            return;

        var botSection = new VisualElement();
        var topSection = new VisualElement();

        previewContent = previewParent;

        if (_OnConfirm != null)
            OnConfirm = _OnConfirm;

        botSection.Add(confirmButton = new Button(() =>
        {
            ConfirmProp();
            if (OnConfirm != null)
                OnConfirm.Invoke();
        })
        {
            text = "Confirm"
        });

        SetConfirmButton(false);

        if (propAsset == null) // creating new
        {
            dataScriptable = SkillsVRNodes.Managers.ScriptableObjectManager.CreateScriptableObject<PropDataScriptable>(ScriptablePath, objectName);
            scriptableName = dataScriptable.name;

            dataScriptable.assetDatabaseObjectPath = path;

            PropDetailData details = new PropDetailData(SetConfirmButton);
            topSection.Add(details.GetVisual());
            dataScriptable.propDetails = details;

            PropTypeData typeOfProp = new PropTypeData(ChangedPropType);
            topSection.Add(typeOfProp.GetVisual());
            dataScriptable.propType = typeOfProp;

            dataScriptable.intertactableProp = new InteractablePropComponentData();

            dataScriptable.socketProp = new SocketPropComponentData();

            SetUpProp(false);
        }
        else // loading edit
        {
            dataScriptable = propAsset;

            PropDetailData details = new PropDetailData(SetConfirmButton);
            details.Copy(dataScriptable.propDetails);
            topSection.Add(details.GetVisual());
            dataScriptable.propDetails = details;


            PropTypeData typeOfProp = new PropTypeData(ChangedPropType);
            typeOfProp.Copy(dataScriptable.propType);
            topSection.Add(typeOfProp.GetVisual());
            dataScriptable.propType = typeOfProp;

            ChangedPropType(typeOfProp.typeChoice);
        }

        specificTypeContent.Add(topSection);
        specificTypeContent.Add(botSection);
        Add(specificTypeContent);
        completedImport = false;
    }


    public void OnWindowClosed()
    {
        if (instatiatedInSceneObject != null)
            UnityEngine.Object.DestroyImmediate(instatiatedInSceneObject);

        if (!completedImport && scriptableName != null)
        {
            AssetDatabase.DeleteAsset(ScriptablePath + "/" + scriptableName + ".asset");
            AssetDatabase.Refresh();
        }

        if (previewClass != null)
        {
            previewClass.CleanUp();
            previewContent.Clear();
        }
    }

    private void ConfirmProp()
    { 

        if (previewClass != null)
        {
            previewClass.RemovePreviewComponent();
        }


        dataScriptable.propPrefab = instatiatedInSceneObject;
        generatorComponent.UpdateColliderData();
        generatorComponent.UpdatePropDetails();
        UnityEngine.Object.DestroyImmediate(generatorComponent);

        objectName = dataScriptable.propDetails.name;

        // Check if the prefab already exists
        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);

        if (existingPrefab != null)
        {
            if (EditorUtility.DisplayDialog("Prop Name Already Exists", "Would you like to overwrite or create a new prop?", "Overwrite", "Create New"))
            {
                SetLabelsAndClean(PrefabUtility.SaveAsPrefabAsset(instatiatedInSceneObject, PrefabPath));
            }
            else
            {
                createdVariant = true;
                var newDataScriptable = SkillsVRNodes.Managers.ScriptableObjectManager.CreateScriptableObject<PropDataScriptable>(ScriptablePath, objectName);
                newDataScriptable.HardCopy(dataScriptable);
                EditorUtility.SetDirty(newDataScriptable);
                objectName = dataScriptable.propDetails.name;

                SetLabelsAndClean(PrefabUtility.SaveAsPrefabAsset(instatiatedInSceneObject, AssetDatabase.GenerateUniqueAssetPath(PrefabPath)));
            }
        }
        else
        {
            SetLabelsAndClean(PrefabUtility.SaveAsPrefabAsset(instatiatedInSceneObject, PrefabPath));
        }

        PropManager.Validate();

        completedImport = true;

        if (!createdVariant)
            EditorUtility.SetDirty(dataScriptable);
    }

    private void SetLabelsAndClean(GameObject obj)
    {
        //unity asset lables for prop window
        List<string> labelList = AssetDatabase.GetLabels(obj).ToList();
        if (!labelList.Contains("Hotspot"))
            labelList.Add("Hotspot");
        if (!labelList.Contains("Prop"))
            labelList.Add("Prop");
        AssetDatabase.SetLabels(obj, labelList.ToArray());
        AssetDatabase.Refresh();

        if (!createdVariant)
            dataScriptable.propPrefab = obj;

        UnityEngine.Object.DestroyImmediate(instatiatedInSceneObject);
    }

    private void SetUpProp(bool hasPreview)
    {
        ResetInstatiatedPrefab();

        if (generatorComponent == null)
        {
            GeneratorComponentSetup();
        }

        generatorComponent.Generate(instatiatedInSceneObject);
        previewContent.Add(PrepareNextAssetForPreview(hasPreview));
    }

    private bool Validate(string path)
    {
        assetDatabaseObject = (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));

        if (assetDatabaseObject != null)
        {

            string[] split = path.Split('/');
            objectName = split[split.Length - 1].Split('.')[0];

            foreach (var s in split)
            {
                prefabPath += s + "/";
                if (s == "Models")
                    break;
            }

            return true;
        }
        else
        {
            Debug.LogError("Failed to load GameObject from path: " + path);
            return false;
        }
    }

    private void ResetInstatiatedPrefab()
    {

        if (previewClass != null)
        {
            previewClass.RemovePreviewComponent();
            previewClass.CleanUp();
            previewContent.Clear();

        }

        if (instatiatedInSceneObject != null)
            UnityEngine.Object.DestroyImmediate(instatiatedInSceneObject);
        PropManager.Validate();





        instatiatedInSceneObject = UnityEngine.Object.Instantiate(assetDatabaseObject);


    }

    private VisualElement PrepareNextAssetForPreview(bool showColliderPreview)
    {
        previewClass = new ImportPreview();
        previewClass.SetUp(instatiatedInSceneObject);
        return previewClass.StartRendering(showColliderPreview);
    }

    private void GeneratorComponentSetup()
    {
        if (generatorComponent == null)
        {
            generatorComponent = instatiatedInSceneObject.AddComponent<PropGeneratorComponent>();
            generatorComponent.Initialise(dataScriptable);
        }
    }

    private void ChangedPropType(int val)
    {
        switch (val)
        {
            case 0:
                {
                    SetUpProp(false);
                    break;
                }
            case 1:
                {
                    SetUpProp(true);
                    break;
                }
            case 2:
                {
                    SetUpProp(true);
                    break;
                }

        }
    }

    private void SetConfirmButton(bool val)
    {
        confirmButton.SetEnabled(val);
    }
}
