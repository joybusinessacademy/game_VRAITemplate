using SkillsVR.EnterpriseCloudSDK;
using SkillsVR.EnterpriseCloudSDK.Networking.API;
using System;
using static SkillsVR.Telemetry.Networking.API.PostEditorLog;

namespace SkillsVR.Telemetry.Networking.API
{
    public class PostEditorLog : AbstractAPI<EditorLogData, AbstractAPI.EmptyResponse>
	{
		public PostEditorLog(string filename) : base(ECAPI.domain + "/api/cck/logs?filename=" + filename)
		{
			requestType = HttpRequestType.POST;
			authenticated = true;
		}

		[Serializable]
		public class EditorLogData
		{
			public string logContent;
		}
	}
}
