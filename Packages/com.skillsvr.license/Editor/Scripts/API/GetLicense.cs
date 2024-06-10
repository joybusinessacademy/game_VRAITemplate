using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SkillsVR.EnterpriseCloudSDK.Networking.API;
using SkillsVR.EnterpriseCloudSDK;
using System;

namespace SkillsVR.License.Networking.API
{
    public partial class GetLicense : AbstractAPI<AbstractAPI.EmptyData, GetLicense.Response>
    {
        public GetLicense() : base(string.Format(ECAPI.domain + "/api/users/cck-license"))
        {
            requestType = HttpRequestType.GET;
            authenticated = true;
        }

        [System.Serializable]
        public partial class Response : BaseResponse
        {
            public int code;
            public string message;
            public LicenseData data = new LicenseData();
        }

        public class LicenseData
        {
            public DateTime expiryDate;
            public string status;
            public bool hasCck;
            public bool hasPermission;
        }
    }
}
