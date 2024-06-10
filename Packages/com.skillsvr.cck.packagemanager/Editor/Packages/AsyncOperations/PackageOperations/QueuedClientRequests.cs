using System;
using System.Collections;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace SkillsVR.CCK.PackageManager.AsyncOperation.PackageOperations
{

    // The Client requests may fail if try run multiple requests at same time.
    // This class will check and wait for any running Client request.
    // This will make Client request not fail in cases like search packages during adding a package,
    // the search will continue after add request done.
    public class QueuedClientRequest : CustomAsyncOperation<Request>
    {
        public const string REQUEST_CONFLICT_MESSAGE = "is already running and must be completed before another can be started";
        protected Func<Request> CustomRequestMethod { get; set; }
        protected Request CurrentRequest { get; set; }

        public QueuedClientRequest(Func<Request> requestFunc)
        {
            CustomRequestMethod = requestFunc;
        }

        protected override IEnumerator OnProcessRoutine()
        {
            if (null == CustomRequestMethod)
            {
                SetError("Request delegate cannot be null.");
                yield break;
            }
            CurrentRequest = null;
            bool wait = true;
            while (wait)
            {
                CurrentRequest = CustomRequestMethod.Invoke();
                Result = CurrentRequest;
                if (null == CurrentRequest)
                {
                    SetError("The return request from request delegate cannot be null.");
                    yield break;
                }
                while (!CurrentRequest.IsCompleted)
                {
                    yield return null;
                }
                if (CurrentRequest.Status == StatusCode.Failure
                    && CurrentRequest.Error.message.Contains(REQUEST_CONFLICT_MESSAGE))
                {
                    yield return new WaitForSecondsRealtime(0.2f);
                    continue;
                }
                else
                {
                    if (CurrentRequest.Status == StatusCode.Failure)
                    {
                        SetError(CurrentRequest.Error.message);
                    }
                    yield break;
                }
            }
        }
    }

    public class QueuedClientRequest<T> : QueuedClientRequest where T : Request
    {
        public new T Result { get; protected set; }
        public QueuedClientRequest(Func<T> requestFunc) : base(requestFunc)
        {
        }

        protected override IEnumerator OnProcessRoutine()
        {
            yield return base.OnProcessRoutine();
            Result = CurrentRequest as T;
        }
    }
}