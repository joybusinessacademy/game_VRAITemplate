using SkillsVR.EnterpriseCloudSDK.Data;
using UnityEngine;

namespace SkillsVR.EnterpriseCloudSDK.Networking.API
{
    
    public partial class UpdateSessionStatus : AbstractAPI<UpdateSessionStatus.Data, AbstractAPI.EmptyResponse>
    {
        public enum Status {
            Planned = 0,
            Inprogress,
            Completed,
            Incomplete,
            Passed,
            Failed
        }

        [System.Serializable]
        public class Data
        {
            public string id;
            public int status;
        }
        
        public UpdateSessionStatus(string sessionId, Status iStatus) : base(string.Format(ECAPI.domain + "/api/plannedsession/status").ToString())
        {
            data = new Data() { id = sessionId, status = (int)iStatus };
            requestType = HttpRequestType.PUT;
            authenticated = true;            
        }
    }
}
