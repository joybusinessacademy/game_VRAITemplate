using System.Collections;
using System.Collections.Generic;

namespace SkillsVR.CCK.PackageManager.AsyncOperation
{
    public enum OperationState
    {
        None,
        InProgress,
        Success,
        Failure,
    }

    public interface ICustomAsyncOperation : IEnumerator
    {
        OperationState State { get; }
        bool IsComplete { get; }
        string Error { get; }
        string ErrorStackTrace { get; }
        List<string> GetExtraInfoStrings();
    }

    public interface ICustomAsyncOperation<T> : ICustomAsyncOperation
    {
        T Result { get; }
    }
}