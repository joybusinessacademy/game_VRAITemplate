using Scripts.VisualElements;
using SkillsVRNodes.Managers.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
//using System.Reflection.Emit;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
//using SkillsVR.EnterpriseCloudSDK.Networking.API;
//using SkillsVR.EnterpriseCloudSDK.Data;
//using SkillsVR.EnterpriseCloudSDK.Editor.Editors;
//using SkillsVR.EnterpriseCloudSDK;
//using static SkillsVR.EnterpriseCloudSDK.Editor.EnterpriseCloudSDKEditorWindow;
//using SkillsVR.EnterpriseCloudSDK.Networking;

public class MenuModualEnterprise : MenuModualContentBase
{
    //private const string dataPathTypename = "Enterprisedata";
    //private EnterpriseScriptable modualAsset;
    ////==========
    //private const string ECCloudClassName = "SkillsVR.EnterpriseCloudSDK.Editor.EnterpriseCloudSDKEditorWindow, com.skillsvr.EnterpriseCloudSDK.editor";

    ////==========

    //private VisualElement regionParent;
    //private List<string> possibleRegions = new List<string>() { "US", "AU" };

    //private string currentRegion;
    //private bool isLoggedIn;
    //private bool hasConfig;
    //private ECRecordCollectionAsset recordAsset = null;
    //private Button GetConfigButton;
    //private VisualElement learningRecParent;


    //public MenuModualEnterprise(string title)
    //{
    //    modualAsset = TryFindData<EnterpriseScriptable>(dataPathPrefix + dataPathTypename + ".asset");
    //    CreateContentModual(title);

    //    //================
    //    Type windowType = Type.GetType(ECCloudClassName);
    //    EditorWindow.GetWindow(windowType, true, null, true);
    //    //=================
    //    isLoggedIn = false;
    //    hasConfig = false;

    //    //region
    //    regionParent = new VisualElement();
    //    regionParent.Add(SetUpRegionLabel());
    //    regionParent.Add(SetUpRegionDropdown());
    //    AddNewPairOfSettings("Region: ", regionParent);

    //    //login
    //    var loginText = new TextField();
    //    loginText.value = modualAsset.username;
    //    loginText.RegisterValueChangedCallback(OnUserChange => { modualAsset.username = OnUserChange.newValue; });
    //    loginText.style.marginLeft = 5;
    //    AddNewPairOfSettings("Login: ", loginText);

    //    var passText = new TextField();
    //    passText.value = modualAsset.password;
    //    passText.RegisterValueChangedCallback(OnPassChange => { modualAsset.password = OnPassChange.newValue; } );
    //    passText.style.marginLeft = 5;
    //    AddNewPairOfSettings("Password: ", passText);

    //    var scenarioIdText = new TextField();
    //    scenarioIdText.value = modualAsset.scenarioID;
    //    scenarioIdText.RegisterValueChangedCallback(OnSidChange => { modualAsset.scenarioID = OnSidChange.newValue; });
    //    scenarioIdText.style.marginLeft = 5;
    //    AddNewPairOfSettings("Scenario ID: ", scenarioIdText);

    //    var loginbutton = new Button(() => { TryLogin(); });
    //    loginbutton.Add(new Label("Login"));
    //    Add(loginbutton);

    //    CheckShowGetConfigButton();
    //    CheckShowLearningRecord();
    //}

    ////REGION
    //private VisualElement SetUpRegionLabel()
    //{
    //    var currentRegion = new Label(modualAsset.regionTag.IsNullOrWhitespace() ? possibleRegions[0] :modualAsset.regionTag );     
    //    return currentRegion;
    //}
    //private VisualElement SetUpRegionDropdown()
    //{
    //    List<Button> buttonsToAdd = new List<Button>();
    //    foreach (var item in possibleRegions)
    //    {
    //        var label = new Label(item);
    //        var button = new Button();
    //        button.Add(label);
    //        button.clicked += () => OnRegionSelected(label);
    //        buttonsToAdd.Add(button);
    //    }

    //    var dropdown = new ListDropdown<Button>("Possible Regions", buttonsToAdd, (Button var) => { return var; });
    //    dropdown.style.alignSelf = Align.FlexEnd;
    //    dropdown.style.maxWidth = 200;
    //    return dropdown;

    //}
    //private void RefreshRegionContent()
    //{
    //    regionParent = new VisualElement();
    //    regionParent.Add(SetUpRegionLabel());
    //    regionParent.Add(SetUpRegionDropdown());

    //    if (TryGetRightContainer("Region: ", out right))
    //    {
    //        right.Clear();
    //        right.Add(regionParent);
    //    }
    //}

    ////LOGIN
    //private void TryLogin()
    //{
    //    string targetId = string.Empty;

    //    recordAsset = ECRecordCollectionAssetEditor.CreateOrLoadAsset();

    //    switch (modualAsset.regionTag)
    //    {
    //        case "US":
    //            targetId = "prod-us";
    //            break;
    //        case "AU":
    //            targetId = "prod-au";
    //            break;
    //        case "US-Test":
    //            targetId = "test-us";
    //            break;
    //        case "AU-Test":
    //            targetId = "test-au";
    //            break;
    //        case "AU-Dev":
    //            targetId = "dev-au";
    //            break;
    //    }

    //    var config = SkillsVR.EnterpriseCloudSDK.Editor.Networking.ConfigService.Get(targetId);

    //    recordAsset.currentConfig.loginData.clientId = config.clientId;
    //    recordAsset.currentConfig.loginData.loginUrl = config.ropcUrl;
    //    recordAsset.currentConfig.loginData.scope = config.scope;
    //    recordAsset.currentConfig.domain = config.domain;

    //    PlayerPrefs.SetString("OCAPIM_SUB_KEY", config.subscriptionKey);

    //    ECAPI.domain = config.domain;
    //    ECAPI.Login(recordAsset.currentConfig.loginData,
    //        OnLoginSuccess => {

    //            // parse token
    //            JSONNode node = Decode(RESTCore.AccessToken);
    //            PlayerPrefs.SetString("ORGCODE", node["extension_OrgCode"].ToString().Replace("\"", string.Empty));
    //            PlayerPrefs.Save();
    //            isLoggedIn = true;
    //            CheckShowGetConfigButton();
    //        },
    //        LogError=> {
    //            Debug.LogError("Error Logging In");
    //        });

    //    PlayerPrefs.Save();
    //}

    //private void CheckShowGetConfigButton()
    //{
    //    if (isLoggedIn && ECAPI.HasLoginToken())
    //    {
    //        GetConfigButton = new Button(()=> {
    //                ECAPI.GetConfig(recordAsset.currentConfig.scenarioId, RecieveConfigResponse=> {

    //                    recordAsset.currentConfig.managedRecords.Clear();
    //                    if (null == RecieveConfigResponse || null == RecieveConfigResponse.data || 0 == RecieveConfigResponse.data.Length)
    //                    {
    //                        Debug.Log("No context loaded.");
    //                        SaveRecordAsset();
    //                        return;
    //                    }
    //                    recordAsset.AddRange(RecieveConfigResponse.data);
    //                        SaveRecordAsset();

    //                    //ECRecordContentEditorWidget.GetEditorRenderingWidgetListFromRecordCollection(recordAsset.currentConfig.manageRecordsMemory);

    //                    //vvvv do something with those vvvv
    //                    //recordAsset.currentConfig.manageRecordsMemory
    //                    CheckShowLearningRecord();

    //                }, LogError=> {
    //                    Debug.LogError("Error Getting Config");
    //                });
    //        });
    //        GetConfigButton.text = "Get Configs";

    //        Add(GetConfigButton);
    //    }
    //    else
    //    {
    //        if(GetConfigButton!=null)
    //            Remove(GetConfigButton);
    //    }
    //}

    //private void SaveRecordAsset()
    //{
    //    EditorUtility.CopySerialized(recordAsset, recordAsset);
    //    AssetDatabase.SaveAssets();
    //    AssetDatabase.Refresh();
    //}

    //private void CheckShowLearningRecord()
    //{
    //    if(isLoggedIn && ECAPI.HasLoginToken() && recordAsset.currentConfig.managedRecords.Count > 0)
    //    {
    //        learningRecParent = new VisualElement();

            

    //        if (TryGetRightContainer("Learning Record Stuff: ", out right))
    //        {
    //            right.Clear();               
    //        }

    //        foreach (var item in recordAsset.currentConfig.manageRecordsMemory)
    //        {
    //            var singleRecord = new VisualElement();
    //            var namelabel = new Label(item.name);
    //            namelabel.style.whiteSpace = WhiteSpace.Normal;
    //            singleRecord.Add(namelabel);

    //            var singleRecordDetails = new VisualElement();

    //            var toggle = new Toggle();
    //            toggle.RegisterValueChangedCallback<bool>(Ontoggled => {
    //                toggle.value = !toggle.value;
    //                item.gameScoreBool = toggle.value;
    //            });
    //            singleRecordDetails.Add(toggle);

    //            var label = new Label();
    //            label.text = item.requirement;
    //            singleRecordDetails.Add(label);

    //            singleRecord.Add(singleRecordDetails);
    //            learningRecParent.Add(singleRecord);

    //        }

    //        AddNewPairOfSettings("Learning Record Stuff: ", learningRecParent);
    //    }
    //}

    //private void RecordFuncToggleChange(ChangeEvent<bool> evt)
    //{
    //    throw new NotImplementedException();
    //}

    //private void OnRegionSelected(Label label)
    //{
    //    modualAsset.regionTag = label.text;
    //    EditorUtility.SetDirty(modualAsset);
    //    RefreshRegionContent();
    //}

    //protected override void CreateContentModual(string title)
    //{
    //    base.CreateContentModual(title);
    //}
}
