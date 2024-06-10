using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ImportDirectorWindow : EditorWindow
{
    static ImportDirectorWindow window;
    static Button newButton, editButton;
    static List<string> filepaths = new List<string>();
    static List<PropDataScriptable> propAssets = new List<PropDataScriptable>();
    static TwoPaneSplitView splitView;
    static VisualElement currentAssetModual;
    static int currentPathIndex = 0;

    public static void ShowWindow()
    {
        window = (ImportDirectorWindow)GetWindow(typeof(ImportDirectorWindow));
        window.titleContent = new GUIContent("Asset Import");
        window.minSize = new Vector2(800, 400);

         newButton = new Button(() => {
            SkillsVRNodes.Managers.Utility.SkillsVRAssetImporter.ImportAssetsButton();
        })
        {
            text = "Import New"    
        };

         editButton = new Button(() => {
            LoadAllEditable();
        })
        {
            text = "Edit Existing"
        };


        window.rootVisualElement.Add(newButton);
        window.rootVisualElement.Add(editButton);

        allreadyImported = SkillsVRNodes.Managers.ScriptableObjectManager.GetAllInstances<PropDataScriptable>();

        if (allreadyImported.Count == 0)
            editButton.SetEnabled(false);

        window.Focus();
        window.Show();
    }

    static List<PropDataScriptable> allreadyImported = new List<PropDataScriptable>();
    private static void LoadAllEditable()
    {
        window.rootVisualElement.Remove(newButton);
        window.rootVisualElement.Remove(editButton);

       

        List<string> paths = new List<string>();
        List<PropDataScriptable> assets = new List<PropDataScriptable>();

        foreach (var propAsset in allreadyImported)
        {
            paths = new List<string>();
            assets = new List<PropDataScriptable>();

            newButton = new Button(() => {

                paths.Add(propAsset.assetDatabaseObjectPath);
                assets.Add(propAsset);
                LoadAssetEdit(paths, assets);
            })
            {
                text = propAsset.propDetails.name
            };

            window.rootVisualElement.Add(newButton);
        }
    }


    public static void LoadAssetEdit(List<string> _filepaths, List<PropDataScriptable> _propAssets)
    {
        window.rootVisualElement.Clear();

        filepaths = _filepaths;
        propAssets = _propAssets;
        currentPathIndex = 0;
        splitView = new TwoPaneSplitView(0, 800, TwoPaneSplitViewOrientation.Horizontal);
        
        ShowNextAsset();
        window.rootVisualElement.Add(splitView);
    }

    static void ShowNextAsset()
    {
        if (currentPathIndex >= filepaths.Count)
        {
            window.Close();
        }
        else
        {

            var leftSide = new VisualElement();
            leftSide.style.paddingLeft = 10;
            leftSide.style.paddingRight = 10;
            leftSide.style.paddingTop = 10;

            var rightSide = new VisualElement();
            rightSide.style.paddingLeft = 10;
            rightSide.style.paddingRight = 10;
            rightSide.style.paddingTop = 10;

            if (currentAssetModual != null)
                currentAssetModual.Clear();

            currentAssetModual = new PropImportModule(filepaths[currentPathIndex], rightSide,
                propAssets == null ? null : propAssets[currentPathIndex],
                () =>
                {
                    ContinueShowNextAsset();
                });

            leftSide.Add(currentAssetModual);

            splitView = new TwoPaneSplitView(0, 800, TwoPaneSplitViewOrientation.Horizontal);
            splitView.Add(leftSide);
            splitView.Add(rightSide);
        }
    }

    static private void ContinueShowNextAsset()
    {
        currentPathIndex++;
        ShowNextAsset();
        window.rootVisualElement.Clear();
        window.rootVisualElement.Add(splitView);
    }

    public void OnDestroy()
    {
        //TODO: add base for common leverages of modules - check if settings modual base has crossover
        if ((currentAssetModual as PropImportModule) != null)
            (currentAssetModual as PropImportModule).OnWindowClosed();
    }
}
