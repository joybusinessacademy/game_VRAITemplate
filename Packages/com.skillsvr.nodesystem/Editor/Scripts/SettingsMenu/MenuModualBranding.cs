using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Scripts.VisualElements;
using UnityEditor;
using SkillsVRNodes.Managers.Utility;
using System;
using System.IO;

public class MenuModualBranding : MenuModualContentBase
{
    private const string dataPathTypename = "Brandingdata";
    private BrandingScriptable modualAsset;

    private VisualElement iconParent;
    private Image iconChosenLocal;
    private List<Sprite> possibleIconSprites = new List<Sprite>();

    private const string CCK_PS_SELECTEDPROJECT = "CCK_PS_SELECTEDPROJECT";

    public MenuModualBranding(string title, BrandingScriptable brandingScriptable)
    {
        if(brandingScriptable)
        {
			modualAsset = brandingScriptable;// TryFindData<BrandingScriptable>(dataPathPrefix + dataPathTypename + ".asset");
			CreateContentModual(title);

			iconParent = new VisualElement();
			iconParent.Add(SetUpChosenIcon());
			//iconParent.Add(SetUpIconDropdown());
			// Reference to the SelectionIcon

			AddNewPairOfSettings("Main Icon: ", iconParent);

			//var mainColor = new ColorField();
			//mainColor.value = modualAsset.mainColor;
			//mainColor.RegisterCallback<ChangeEvent<Color>>(evt =>
			//{
			//	modualAsset.mainColor = evt.newValue;
			//});
			//AddNewPairOfSettings("Primary Color: ", mainColor);

			//var secondColor = new ColorField();
			//secondColor.value = modualAsset.secondaryColor;
			//secondColor.RegisterCallback<ChangeEvent<Color>>(evt =>
			//{
			//	modualAsset.secondaryColor = evt.newValue;
			//});
			//AddNewPairOfSettings("Secondary Color: ", secondColor);

			//var slogan = new TextField();
			//slogan.value = modualAsset.slogan;
			//slogan.RegisterValueChangedCallback(OnSloganChange);
			//slogan.style.marginLeft = 5;
			//AddNewPairOfSettings("Slogan: ", slogan);
		}
    }

    private void RefreshMainIconContent()
    {
        iconParent = new VisualElement();
        iconParent.Add(SetUpChosenIcon());
        //iconParent.Add(SetUpIconDropdown());

       if (TryGetRightContainer("Main Icon: ", out right))
        {
            right.Clear();
            right.Add(iconParent);
        }
    }

    private VisualElement SetUpChosenIcon()
    {
        VisualElement iconHolder = new VisualElement();
        //USS
        iconHolder.style.minHeight = 100;
        iconHolder.style.maxHeight = 100;
        iconHolder.style.maxWidth = 100;
        iconHolder.style.minWidth = 100;

        //iconHolder.style.SetBorderWidth(4, true);
        iconHolder.style.SetBorderColor(new Color(0,0,0,0.5f));

        iconHolder.style.backgroundColor = new Color(0, 0, 0, 0.25f);
        iconHolder.style.alignSelf = Align.FlexEnd;

        iconChosenLocal = new Image();
        {
            //USS
            iconChosenLocal.style.minHeight = 100;
            iconChosenLocal.style.maxHeight = 100;
            iconChosenLocal.style.maxWidth = 100;
            iconChosenLocal.style.minWidth = 100;
        }

        if (modualAsset.textureIcon != null)
        {
            iconChosenLocal.image = modualAsset.textureIcon;
            //iconChosenLocal.sprite = modualAsset.mainIcon;
        }
        else
        {
            iconChosenLocal.image = Resources.Load<Texture2D>("Icon/Unity");
        }

		// Create a new Text element
		Label textElement = new Label
		{
			text = "Select Icon: ",  // Set the desired text content
			style =
	        {
		        color = Color.white,  // Set the text color
                backgroundColor = Color.black,  // Set the background color
                alignSelf = Align.FlexEnd,  // Center-align the text horizontally
                alignItems = Align.FlexEnd,
                marginTop = 80,  // Adjust the top margin as needed
                marginBottom = 0,  // Adjust the bottom margin as needed

            }
		};


		iconHolder.RegisterCallback<ClickEvent>(OnIconClicked);

		iconHolder.Add(iconChosenLocal);

		iconChosenLocal.Add(textElement);
		return iconHolder;
    }

	private void OnIconClicked(ClickEvent evt)
	{
		string path = EditorUtility.OpenFilePanel("Select Texture", "Assets/", "png,jpg,jpeg,tif,tiff,bmp,gif");
            LoadAndCreateSprite(path);

		//// Open a file dialog to select an image
		//string imagePath = EditorUtility.OpenFilePanel("Select Icon", "", "png,jpg,jpeg"); // Filter for image file types
  //      LoadAndCreateSprite(imagePath);
	}

	private VisualElement SetUpIconDropdown()
    {
        //Find all sprites if we havn't already
        if (possibleIconSprites.Count == 0)
        {
            string[] guids = AssetDatabase.FindAssets("t:Sprite");
            

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);

                if (sprite != null)
                {
                    possibleIconSprites.Add(sprite);
                }
            }
        }

        List<Button> imagesForDropdown = new List<Button>();
        for (int i = 0; i < possibleIconSprites.Count; i++)
        {

            Button button = new Button();
            Image img = new Image() { sprite = possibleIconSprites[i] };
            button.Add(img);
            button.clicked += () => IconChosen(img);
            //USS
            button.style.maxHeight = 50;
            button.style.maxWidth = 50;

            imagesForDropdown.Add(button);
        }

        var dropdown = new ListDropdown<Button>("Possible Icons", imagesForDropdown, (Button var)=>{ return var; });
        //USS 
        dropdown.style.alignSelf = Align.FlexEnd;
        dropdown.style.maxWidth = 200;
        dropdown.style.flexDirection = FlexDirection.Row;
        dropdown.style.flexWrap = Wrap.Wrap;
        dropdown.Expanded = false;
        dropdown.Refresh();
        return dropdown;
    }

    private void IconChosen(Image myImage)
    {
        modualAsset.textureIcon = myImage.image as Texture2D;
        //modualAsset.mainIcon = myImage.sprite;
        iconChosenLocal = myImage;
        RefreshMainIconContent();
    }
	//private void OnSloganChange(ChangeEvent<string> evt)
	//{
	//    modualAsset.slogan = evt.newValue;
	//    EditorUtility.SetDirty(modualAsset);
	//}

    public void LoadAndCreateSprite(string imagePath)
	{
		if (!string.IsNullOrEmpty(imagePath))
		{
			string imageName = Path.GetFileNameWithoutExtension(imagePath);

			// Load the image file as bytes
			byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);

			// Create a new Texture2D
			Texture2D tex = new Texture2D(2, 2); // You can set the initial dimensions as needed

			// Load the image data into the Texture2D
			bool success = ImageConversion.LoadImage(tex, imageBytes);

			if (success)
			{
                iconChosenLocal.image = tex;
                modualAsset.textureIcon = tex;
                ///
                if(AssetDatabase.FindAssets(imagePath).Length == 0) {

                    string projectName = SessionState.GetString(CCK_PS_SELECTEDPROJECT, AssetDatabaseFileCache.GetCurrentMainGraphName());

                    string spriteLocation = "Assets/Contexts/" + projectName + "/Sprites";

                    if(!Directory.Exists(spriteLocation))
                        Directory.CreateDirectory(spriteLocation);

                    string fileName = Path.GetFileName(imagePath);
                    string destinationPath = Path.Combine(spriteLocation, fileName);

                    if (File.Exists(destinationPath))
                        return;

                    try
                    {
                        File.Copy(imagePath, destinationPath);
                        Debug.Log("Sprite created and saved as an asset.");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Error: " + ex.Message);
                    }

			    	AssetDatabase.SaveAssets();
                }
			}
			else
			{
				Debug.LogError("Failed to load the image.");
			}
		}
		else
		{
			Debug.LogError("Image path is empty or null.");
		}
	}
}