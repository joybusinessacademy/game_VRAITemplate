//using ICSharpCode.NRefactory.Ast;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


public class MenuModualPackageName : MenuModualContentBase
{

    private PackageNameScriptable modualAsset;

    public MenuModualPackageName(string title, PackageNameScriptable modualAsset)
    {
        if (modualAsset != null)
        {
			this.modualAsset = modualAsset;// = TryFindData<PackageNameScriptable>(dataPathPrefix + dataPathTypename + ".asset");
			CreateContentModual(title);

			var companyName = new TextField();
			companyName.RegisterValueChangedCallback(OnCompanyNameChange);
			companyName.style.marginLeft = 5;

			if(!string.IsNullOrEmpty(modualAsset.CompanyName))
				companyName.value = modualAsset.CompanyName;
			else
			{
				companyName.value = PlayerSettings.companyName;
				modualAsset.CompanyName = companyName.value;
			}

			AddNewPairOfSettings("Company Name: ", companyName);

			//var productName = new TextField();
			//productName.RegisterValueChangedCallback(OnProductNameChange);
			//productName.style.marginLeft = 5;
			//if (!string.IsNullOrEmpty(modualAsset.productName))
			//	productName.value = modualAsset.productName;
			//else
			//{
			//	productName.value = PlayerSettings.productName;
			//	modualAsset.productName = productName.value;
			//}

			//AddNewPairOfSettings("Product Name: ", productName);

			var verisonText = new TextField();
			verisonText.value = modualAsset.Version;
			verisonText.RegisterValueChangedCallback(OnVersionChange);
			verisonText.style.marginLeft = 5;

			if (!string.IsNullOrEmpty(modualAsset.Version))
				verisonText.value = modualAsset.Version;
			else
			{
				verisonText.value = PlayerSettings.bundleVersion;
				modualAsset.Version = verisonText.value;
			}

			AddNewPairOfSettings("Version Number: ", verisonText);

		}
	}

    private void OnVersionChange(ChangeEvent<string> evt)
    {
        modualAsset.Version = evt.newValue;
		PlayerSettings.bundleVersion= evt.newValue;
        EditorUtility.SetDirty(modualAsset);
    }

	private void OnCompanyNameChange(ChangeEvent<string> evt)
	{
		modualAsset.CompanyName = evt.newValue;
        EditorUtility.SetDirty(modualAsset);
	}

	private void OnProductNameChange(ChangeEvent<string> evt)
	{

        EditorUtility.SetDirty(modualAsset);
	}

	protected override void CreateContentModual(string title)
    {
        base.CreateContentModual(title);
    }

}
