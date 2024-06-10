using System;
using System.Collections.Generic;

namespace SkillsVR.Impl
{
    public class Result : IResult
    {
        public bool success { get; set; } = false;
        public int code { get; set; } = 0;
        public Exception exception { get; set; } = null;
        public object sender { get; set; } = null;
        public string message { get; set; } = String.Empty;
        public IEnumerable<object> args { get; set; } = new object[0];
        public virtual object resultObject { get; set; }

        public Result(object successResultObject, object resultSender, string info = "")
        {
            success = true;
            code = 0;
            resultObject = successResultObject;
            sender = resultSender;
            message = info;
        }

        public Result(Exception exc, object resultSender, int failCode, string info = "")
        {
            success = false;
            code = failCode;
            exception = exc;
            message = info;
        }

        public Result(Exception exc, object failResultObject, object resultSender, int failCode, string info = "")
        {
            success = false;
            code = failCode;
            exception = exc;
            message = info;
            resultObject = failResultObject;
        }

        public void SetArgs(params object[] args)
        {
            this.args = null == args ? new object[0] : args;
        }
        public void CopyArgs(IResult other)
        {
            if (null == other)
            {
                return;
            }
            args = other.args;
        }

        public override string ToString()
        {
            return GetString("{r} \r\nInfo: {msg}\r\nCode:{c}\r\nSender: {st} {s}\r\nResult Obj: {ot} {o}\r\nArgs:{at}\r\nException: {exc}");
        }

        /// <summary>
        /// Get string with format.
        /// </summary>
        /// <param name="format">output string format, 
        /// {r} - result, success or not, 
        /// {c} - code
        /// {c} - code
        /// {exc} - exception
        /// {s} - sender
        /// {st} - sender type
        /// {msg} - message
        /// {o} - result object
        /// {ot} - result object type
        /// {a} - action args
        /// {at} - action args with type info
        /// </param>
        /// <returns></returns>
        public virtual string GetString(string format)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                return ToString();
            }
            return format
                .Replace("{r}", success.GetObjectString())
                .Replace("{c}", code.GetObjectString())
                .Replace("{exc}", exception.GetObjectString())
                .Replace("{s}", sender.GetObjectString())
                .Replace("{st}", sender.GetTypeName())
                .Replace("{msg}", message.GetObjectString())
                .Replace("{o}", resultObject.GetObjectString())
                .Replace("{ot}", resultObject.GetTypeName())
                .Replace("{a}", args.GetObjectListString("{o}"))
                .Replace("{at}", args.GetObjectListString());
        }
    }

    public class Result<DATA_TYPE> : Result, IResult<DATA_TYPE>
    {
        public new DATA_TYPE resultObject { get; set; }

        public Result(DATA_TYPE successResultObject, object resultSender, string message = "") 
            : base(successResultObject, resultSender, message)
        {
            resultObject = successResultObject;
        }

        public Result(Exception exc, object resultSender, int failCode, string message = "") 
            : base(exc, resultSender, failCode, message)
        {
        }

        public Result(Exception exc, DATA_TYPE failResultObject, object resultSender, int failCode, string message = "")
            : base(exc, failResultObject, resultSender, failCode, message)
        {
            resultObject = failResultObject;
        }
    }
}
