﻿using SkillsVR.EnterpriseCloudSDK.Networking;
using SkillsVR.EnterpriseCloudSDK.Networking.API;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace SkillsVR.EnterpriseCloudSDK.Networking.API
{
    public class SSOLogin
    {
        public static IEnumerator SendSSOLoginForm(SSOLoginData loginData, Action<SSOLoginResponse> onSuccess, Action<string> onFail = null)
        {
            if (null == loginData)
            {
                onFail?.Invoke("SendSSOLoginForm Fail: Login data cannot be null or empty.");
                yield break;
            }
            if (!loginData.IsValid())
            {
                onFail?.Invoke("SendSSOLoginForm Fail: Invalid Login Data.");
                yield break;
            }
            using (UnityWebRequest www = UnityWebRequest.Post(loginData.loginUrl, loginData.GetLoginForm()))
            {
                Debug.Log(string.Join(" ", "Start Login", loginData.userName, "to", loginData.loginUrl));
                www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.LogError(string.Join(" ", "Login Fail:", www.error, loginData.userName, loginData.loginUrl));
                    onFail?.Invoke(www.error);
                }
                else
                {
                    SSOLoginResponse response = new SSOLoginResponse();
                    bool success = false;
                    try
                    {
                        response.Read(www.downloadHandler.text);
                        success = true;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(string.Join(" ", "Login Fail:", e.Message, loginData.userName, loginData.loginUrl));
                        onFail?.Invoke(e.Message);
                    }
                    if (success)
                    {
                        Debug.Log(string.Join(" ", "Login Success ", loginData.userName, "at", loginData.loginUrl));
                        onSuccess?.Invoke(response);
                    }
                }
            }
        }
    }
}