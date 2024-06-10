using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuView : VisualElement
{
    ScrollView menuContentScroller;
    public MenuView(PackageNameScriptable packageNameScriptable, BrandingScriptable brandingScriptable)
    {
        menuContentScroller = new ScrollView(ScrollViewMode.Vertical);

		menuContentScroller.Add(new MenuModualPackageName("Android Package", packageNameScriptable));
        menuContentScroller.Add(new MenuModualBranding("Branding",brandingScriptable));
        //menuContentScroller.Add(new MenuModualEnterprise("Enterprise Link"));
		
        Add(menuContentScroller);
    }
}
