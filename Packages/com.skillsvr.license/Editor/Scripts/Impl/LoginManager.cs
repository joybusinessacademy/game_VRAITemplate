using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System.IO;
using SkillsVR.EnterpriseCloudSDK.Networking;
using SkillsVR.EnterpriseCloudSDK.Editor.Editors;
using SkillsVR.EnterpriseCloudSDK.Data;
using SkillsVR.EnterpriseCloudSDK;
using SkillsVR.EnterpriseCloudSDK.Networking.API;
using Unity.EditorCoroutines.Editor;
using Newtonsoft.Json;
using SkillsVR.License.Networking.API;
using static SkillsVR.License.Networking.API.GetLicense;
using System;

namespace SkillsVR.Login
{
	public class LoginManager : EditorWindow
	{
		//Login Buttons
		private static Button loginBtn;
		private static Button contactUsBtn;
		private static Button forgotPasswordBtn;
		private static Button termsAndConditionsBtn;
		private static Button showPasswordBtn;
		
		private static TextField emailAddressTF;
		private static TextField passwordTF;

		private static ScrollView termsSV;
		private static Button termsReturnBtn;

		private static Toggle termsAndConditionsToggle;

		private static bool passwordVisibility = false;
		private bool focused;
		private static bool termsAcceptedState = false;

		private static DropdownField regionDropdownMenu;
		private static string region = "AU";
		private static List<string> regionChoices = new List<string> { "AU", "US" };

		private static Label errorLog;

		//License
		private static Label licenseDetails;
		private static Button licenseExtensionBtn;
		private static Button licenseGetHelpBtn;

		private static LoginManager windowInstance;

		private static LicenseData licenseOrganizationData = new LicenseData();

		private static string jsonFileName = "LicenseMockData.txt";

		private static bool checkingLogin = false;

		private static Action OnSuccessfulLogin;

		private const string CCK_LOGGEDIN = "CCK_USERLOGGEDIN";
		private const string EDITORFILE_UE = "EDITORLOGGER_UE";
		private const string EDITORFILE_ULS = "EDITORLOGGER_ULS";
		private const string EDITORFILE_UED = "EDITORLOGGER_UED";

		[MenuItem("SkillsVR CCK/Login Window")]
		public static void ShowWindow()
		{
			if(recordAsset == null)
				recordAsset = ECRecordCollectionAssetEditor.CreateOrLoadAsset();

			GenerateWindowInstance();

			GenerateLoginUI();

			windowInstance.ShowModal();
		}

		private static void GenerateWindowInstance()
		{
			if (windowInstance == null)
				windowInstance = CreateInstance<LoginManager>();

			windowInstance.titleContent = new GUIContent("CCK Login");
			windowInstance.minSize = windowInstance.maxSize = new Vector2(800, 800);
			windowInstance.position = new Rect(0, 0, 800, 800);// new Rect(windowInstance.position.x, windowInstance.position.y, windowInstance.rootVisualElement.resolvedStyle.width, windowInstance.rootVisualElement.resolvedStyle.height);

		}

		private static ECRecordCollectionAsset recordAsset = null;

		private void OnGUI()
		{
			if (focused == false)
			{
				rootVisualElement.Focus();
			}

			focused = true;
		}

		private void OnDestroy()
		{
			focused = false;
			CheckLicenseOnClose();
		}

		private static void CheckLicenseOnClose()
		{
#if NODE_DEVELOPMENT
			string path = (Application.streamingAssetsPath + "/" + jsonFileName);

			if (File.Exists(path))
				return;
#endif

			//Dont check this when closing on purpose
			if (checkingLogin)
				return;

			if (licenseOrganizationData == null || !licenseOrganizationData.hasCck)
			{
				EditorCoroutineUtility.StartCoroutineOwnerless(WaitForClose());
			}
			else if (!licenseOrganizationData.hasPermission)
			{
				EditorCoroutineUtility.StartCoroutineOwnerless(WaitForClose(true));				
			}
		}

		private static IEnumerator WaitForClose(bool validLogin = false)
		{
			yield return new EditorWaitForSeconds(1);

			windowInstance = null;

			if(validLogin)
				ShowLicenseInvalid();
			else
				ShowWindow();
		}

		private static void GenerateLoginUI()
		{
			// Load UXML file
			VisualTreeAsset visualTreeAsset = Resources.Load<VisualTreeAsset>("Login_UXML");
			StyleSheet styleSheet = Resources.Load<StyleSheet>("Login_SS");
			VisualElement root = windowInstance.rootVisualElement;
			
			root.Clear();
			visualTreeAsset.CloneTree(root);

			root.styleSheets.Add(styleSheet);
			emailAddressTF = root.Q<TextField>("login-email");
			passwordTF = root.Q<TextField>("login-password");
			loginBtn = root.Q<Button>("login-submit");
			contactUsBtn = root.Q<Button>("login-contactus");
			forgotPasswordBtn = root.Q<Button>("login-forgotpassword");
			termsAndConditionsBtn = root.Q<Button>("login-terms");
			showPasswordBtn = root.Q<Button>("login-togglepassword");

			termsAndConditionsToggle = root.Q<Toggle>("login-termstoggle");

			if (termsAndConditionsToggle != null)
				termsAndConditionsToggle.value = termsAcceptedState;

			errorLog = root.Q<Label>("login-error-emailpassword");
			SetErrorLog();//Turns Visible Off
			regionDropdownMenu = root.Q<DropdownField>("login-region-dropdown");

			if (regionDropdownMenu != null)
			{
				regionDropdownMenu.choices = regionChoices;
				regionDropdownMenu.RegisterValueChangedCallback<string>(OnRegionChanged);
				regionDropdownMenu.value = region;
			}

			loginBtn.clicked += OnLoginButtonClicked;
			contactUsBtn.clicked += OnContactUsButtonClicked;
			forgotPasswordBtn.clicked += OnForgotPasswordButtonClicked;
			termsAndConditionsBtn.clicked += OnTermsAndConditionsButtonClicked;
			showPasswordBtn.clicked += TogglePasswordVisibility;

			termsAndConditionsToggle.RegisterValueChangedCallback<bool>(OnTermsToggleChanged);

			// Register a callback for when the UXML is bound to the hierarchy
			root.RegisterCallback<GeometryChangedEvent>(evt => OnGeometryChanged(evt));
		}

		private static void OnTermsToggleChanged(ChangeEvent<bool> evt)
		{
			termsAcceptedState = evt.newValue;
		}

		private static void OnRegionChanged(ChangeEvent<string> evt)
		{
			region = evt.newValue;
		}

		private static void RemoveLoginCallbacks()
		{
			if(loginBtn != null)
				loginBtn.clicked -= OnLoginButtonClicked;
			if (contactUsBtn != null)
				contactUsBtn.clicked -= OnContactUsButtonClicked;
			if (forgotPasswordBtn != null)
				forgotPasswordBtn.clicked -= OnForgotPasswordButtonClicked;
			if (termsAndConditionsBtn != null)
				termsAndConditionsBtn.clicked -= OnTermsAndConditionsButtonClicked;
			if (showPasswordBtn != null)
				showPasswordBtn.clicked -= TogglePasswordVisibility;
			if (regionDropdownMenu != null)
				regionDropdownMenu.UnregisterValueChangedCallback<string>(OnRegionChanged);
		}

		private static void RemoveTermsCallbacks()
		{
			if(termsReturnBtn != null)
				termsReturnBtn.clicked -= OnTermReturnBtnClicked;
		}

		private static void OnGeometryChanged(GeometryChangedEvent evt)
		{
			// Unregister the callback to avoid multiple calls
			windowInstance.rootVisualElement.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);

			windowInstance.minSize = windowInstance.maxSize = new Vector2(800, 800);
		}

		private static void SetErrorLog(string error = "")
		{
			if (error == "")
				errorLog.visible = false;
			else
			{
				errorLog.text = error;
				errorLog.visible = true;
			}
		}

		private static void OnLoginButtonClicked()
		{
			SetErrorLog();

			if(string.IsNullOrEmpty(emailAddressTF.text) || string.IsNullOrEmpty(passwordTF.text))
			{
				//Missing Field Error
				SetErrorLog("Missing Field Items : Email or Password");
				return;
			}

			if(termsAcceptedState == false)
			{
				SetErrorLog("Terms and Conditions has not been accepted");
				return;
			}

			ConfigureLoginData();

			recordAsset.currentConfig.loginData.userName = emailAddressTF.text;
			recordAsset.currentConfig.loginData.password = passwordTF.text;

			checkingLogin = true;

			windowInstance.Close();

			ECAPI.Login(recordAsset.currentConfig.loginData, OnLoginSuccess, LoginError);
			//SilentLogin();
		}

		private static void ConfigureLoginData()
		{
			string targetId = string.Empty;

			switch (SSOLoginData.regions[recordAsset.currentConfig.loginData.selectedRegion])
			{
				case "US":
					targetId = "prod-us";
					break;
				case "AU":
					targetId = "prod-au";
					break;
				case "US-Test":
					targetId = "test-us";
					break;
				case "AU-Test":
					targetId = "test-au";
					break;
				case "AU-Dev":
					targetId = "dev-au";
					break;
			}


			var config = SkillsVR.EnterpriseCloudSDK.Editor.Networking.ConfigService.Get(targetId);

			recordAsset.currentConfig.loginData.clientId = config.clientId;
			recordAsset.currentConfig.loginData.loginUrl = config.ropcUrl;
			recordAsset.currentConfig.loginData.scope = config.scope;
			recordAsset.currentConfig.domain = config.domain;

			PlayerPrefs.SetString("OCAPIM_SUB_KEY", config.subscriptionKey);
			ECAPI.domain = config.domain;
		}

		private static void LoginError(string obj)
		{
			checkingLogin = false;

			EditorCoroutineUtility.StartCoroutineOwnerless(WaitForClose());

			SetErrorLog("Failed to Login - Check Email and Password - If not working contact SkillsVR");
		}

		private static void OnLoginSuccess(SSOLoginResponse obj)
		{
			SendGetLicnese();
		}

		private static void OnContactUsButtonClicked()
		{
			Application.OpenURL("https://skillsvr.com");
		}

		private static void OnForgotPasswordButtonClicked()
		{
			Application.OpenURL("https://skillsvr.com");
		}

		private static void OnTermsAndConditionsButtonClicked()
		{
			RemoveLoginCallbacks();
			GenerateTermsAndConditionsUI();
		}
		
		public static string GetUserEmailAddress()
		{
			return recordAsset != null ? recordAsset.currentConfig.loginData.userName : "Missing User Name";
		}

		public static string GetLicenseStatus()
		{
			return licenseOrganizationData != null ? licenseOrganizationData.status: "Missing Status";
		}

		public static string GetExpirationDate()
		{
			return licenseOrganizationData != null ? licenseOrganizationData.expiryDate.ToString() : "Missing Date";
		}

		private static void GenerateLicenseInvalidUI()
		{
			// Load UXML file
			VisualTreeAsset visualTreeAsset = Resources.Load<VisualTreeAsset>("License"); 
			VisualElement root = windowInstance.rootVisualElement;
			root.Clear();

			visualTreeAsset.CloneTree(root);

			licenseDetails = root.Q<Label>("license-detail");
			licenseExtensionBtn = root.Q<Button>("license-extension-btn");
			licenseGetHelpBtn = root.Q<Button>("license-help-btn");

			licenseDetails.text = "Your license expired on " + licenseOrganizationData.expiryDate;

			if(licenseExtensionBtn!= null)
				licenseExtensionBtn.clicked += OnLicenseExtensionBtn;

			if(licenseGetHelpBtn!= null)
				licenseGetHelpBtn.clicked += OnLicenseGetHelpBtn;
		}

		private static void OnLicenseGetHelpBtn()
		{
			Application.OpenURL("https://skillsvr.com");
		}

		private static void OnLicenseExtensionBtn()
		{
			Application.OpenURL("https://skillsvr.com");
		}

		private static void GenerateTermsAndConditionsUI()
		{
			// Load UXML file
			VisualTreeAsset visualTreeAsset = Resources.Load<VisualTreeAsset>("TermsAndCondition");
			VisualElement root = windowInstance.rootVisualElement;
			root.Clear();

			visualTreeAsset.CloneTree(root);

			termsSV = root.Q<ScrollView>("termsandcon-scroll");
			termsReturnBtn = root.Q<Button>("termsandcon-return");

			if (termsSV != null)
			{
				termsSV.Clear();

				TextAsset textFile = Resources.Load<TextAsset>("Terms and Conditions - CCK");

				string filePath = "";
				
				if (textFile != null)
					filePath = AssetDatabase.GetAssetPath(textFile);

				if (filePath != string.Empty && File.Exists(filePath))
				{
					string fileContents = File.ReadAllText(filePath);

					// Split the file contents into two strings
					int halfLength = fileContents.Length / 2;
					string firstHalf = fileContents.Substring(0, halfLength);
					string secondHalf = fileContents.Substring(halfLength);

					Label label1 = new Label();
					label1.text = firstHalf;
					label1.style.whiteSpace = WhiteSpace.Normal;
					label1.style.color = Color.black;

					Label label2 = new Label();
					label2.text = secondHalf;
					label2.style.whiteSpace = WhiteSpace.Normal;
					label2.style.color = Color.black;

					termsSV.Add(label1);
					termsSV.Add(label2);
				}
			}

			if (termsReturnBtn != null)
				termsReturnBtn.clicked += OnTermReturnBtnClicked;
		}

		private static void OnTermReturnBtnClicked()
		{
			RemoveTermsCallbacks();
			GenerateLoginUI();
		}

		private static void TogglePasswordVisibility()
		{
			passwordVisibility = !passwordVisibility;

			passwordTF.isPasswordField = !passwordVisibility;
			SetBackgroundImage(showPasswordBtn,!passwordVisibility ? "Icon/Visible" : "Icon/NotVisible");
		}

		public static VisualElement SetBackgroundImage(VisualElement visual, string resourcePath)
		{
			visual.style.backgroundImage = Resources.Load<Texture2D>(resourcePath);
			return visual;
		}

		public static void GetLicneseData(System.Action<GetLicense.Response> success = null, System.Action<string> failed = null)
		{
			GetLicense getLicneseDataConfig = new GetLicense();
			RESTService.Send(getLicneseDataConfig, success, failed);
		}

		public static void SendGetLicnese()
		{
			GetLicneseData(OnSuccessOfLicenseData, OnFailOfLicenseData);
		}

		private static void OnFailOfLicenseData(string obj)
		{
			ShowWindow();
		}

		private static void OnSuccessOfLicenseData(GetLicense.Response obj)
		{
			//TODO: API Not Converting beforehand
			GetLicense.Response response1 = JsonConvert.DeserializeObject<GetLicense.Response>(obj.rawData);

			licenseOrganizationData = response1.data;
			bool licenseValid = (licenseOrganizationData.hasCck && licenseOrganizationData.hasPermission);

			checkingLogin = false;

			if (licenseValid)
			{
				
				SessionState.SetString(EDITORFILE_UE, GetUserEmailAddress());
				SessionState.SetString(EDITORFILE_ULS, licenseOrganizationData.status);
				SessionState.SetString(EDITORFILE_UED, licenseOrganizationData.expiryDate.ToString());

				SessionState.SetBool(CCK_LOGGEDIN, true);
				//EditorLogger.EditorLogIntilization();
				OnSuccessfulLogin?.Invoke();
				windowInstance.Close();
				windowInstance = null;
			}
			else
			{
				ShowLicenseInvalid();
			}
		}

		private static void ShowLicenseInvalid()
		{
			GenerateWindowInstance();
			GenerateLicenseInvalidUI();
			windowInstance.ShowModal();
		}

	}
}