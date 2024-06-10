using SkillsVR.Impl;
using System;

namespace SkillsVR.Mechanic.Core.Impl
{

    public class  MechanicSystemResult : Result<IMechanicSystem>, IMechanicSystemResult
    {
        public MechanicSystemResult(IMechanicSystem successResultObject, object resultSender, string message = "")
            : base(successResultObject, resultSender, message)
        {
        }

        public MechanicSystemResult(Exception exc, object resultSender, int failCode, string actionInfo = "")
            : base(exc, resultSender, failCode, actionInfo)
        {
        }

        public MechanicSystemResult(Exception exc, IMechanicSystem failResultObject, object resultSender, int failCode, string actionInfo = "")
            : base(exc, failResultObject, resultSender, failCode, actionInfo)
        {
        }
    }
}
