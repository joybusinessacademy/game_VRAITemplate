using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuModualContentBase : VisualElement
{
    protected VisualElement left;
    protected VisualElement right;

    protected const string dataPathPrefix= "Assets/Contexts/Settings/";

    Dictionary<string, VisualElement> rightsideByAssociatedLable = new Dictionary<string, VisualElement>();
    protected virtual void CreateContentModual(string title)
    {
        style.backgroundColor = new Color(1, 1, 1, 0.15f);
        style.marginTop = 10;
        var name = new Label(title);
		name.style.paddingTop = 5;
		name.style.paddingBottom = 5;
		name.style.paddingLeft = 5;
        name.style.fontSize = 15;
        name.style.unityFontStyleAndWeight = FontStyle.Bold;
        Add(name);
    }

    protected void AddNewPairOfSettings(string lableName, VisualElement element)
    {
        var coloums = new VisualElement
        {
            name = "columns"
        };
        coloums.style.flexDirection = FlexDirection.Row;


        left = new VisualElement
        {
            name = "LeftSide"
        };

        left.style.paddingLeft = 5;
        //left.style.paddingTop = new Length(10f, LengthUnit.Pixel);
        //left.style.paddingBottom = new Length(10f, LengthUnit.Pixel);
        left.style.flexGrow = 1;
        coloums.Add(left);

        right = new VisualElement
        {
            name = "RightSide"
        };

		right.style.paddingLeft = 5;
		//right.style.paddingTop = new Length(10f, LengthUnit.Pixel);
		//right.style.paddingBottom = new Length(10f, LengthUnit.Pixel);
		right.style.flexGrow = 1;
        right.style.maxWidth = 300;
        coloums.Add(right);

        Add(coloums);

        left.Add(new Label(lableName));
        right.Add(element);

        //cache for adding more of this particular right element
        if (!rightsideByAssociatedLable.ContainsKey(lableName))
            rightsideByAssociatedLable.Add(lableName, right);
    }

    protected TScriptableObject TryFindData<TScriptableObject>(string assetPath) where TScriptableObject : ScriptableObject
    {
        string assetGUID = AssetDatabase.AssetPathToGUID(assetPath);

        if (!string.IsNullOrEmpty(assetGUID))
        {
            return AssetDatabase.LoadAssetAtPath<TScriptableObject>(assetPath);
        }
        else
        {
            Directory.CreateDirectory(assetPath);

            var asset = ScriptableObject.CreateInstance<TScriptableObject>();
            AssetDatabase.Refresh();

            AssetDatabase.CreateAsset(asset, assetPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return asset;
        }
    }

    protected bool TryGetRightContainer(string lableNameKey, out VisualElement rightSide)
    {
        rightSide = null;
        if (rightsideByAssociatedLable.ContainsKey(lableNameKey))
        {
            rightSide = rightsideByAssociatedLable[lableNameKey];
            return true;
        }

        return false;
    }
    
}
