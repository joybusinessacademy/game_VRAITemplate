using System;
using System.Collections.Generic;

namespace SkillsVR
{
    public interface IResult : IGetFormatString
    {
        bool success { get; }
        int code { get; }
        Exception exception { get; }
        object sender { get; }
        string message { get; }
        IEnumerable<object> args { get; }
        object resultObject { get; }
    }

    public interface IResult<DATA_TYPE> : IResult
    {
        new DATA_TYPE resultObject { get; }
    }
}
