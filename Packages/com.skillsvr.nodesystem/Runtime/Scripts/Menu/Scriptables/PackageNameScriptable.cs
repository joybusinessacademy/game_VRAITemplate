using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PackageNameScriptable : ScriptableObject
{
    public string CompanyName { set { companyName = value; SetPlayerSettings(); } get 
        {
            return companyName.IsNullOrWhitespace() ? "CCK" : companyName;
        }
    }
    string companyName;

    public string ProductName { set { productName = value; SetPlayerSettings(); } 
        get
        { 
            return productName.IsNullOrWhitespace() ? "ProductName" : productName;
        } 
    }
    string productName;

    public string Version { set { version = value; SetPlayerSettings(); }
        get
        {
            return version.IsNullOrWhitespace() ? "0.1" : version;
        }
    }
    string version;

    string packageName { get { return "com." + CompanyName + "." + ProductName; } }

    void SetPlayerSettings()
    {
#if UNITY_EDITOR
        PlayerSettings.productName =  ProductName;
        PlayerSettings.companyName = CompanyName;
        PlayerSettings.bundleVersion = Version; 
        PlayerSettings.applicationIdentifier = packageName;
#endif
    }

}
