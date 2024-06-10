using SkillsVR.CCK.PackageManager.AsyncOperation.Github;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace SkillsVR.CCK.PackageManager.AsyncOperation
{
    public abstract class CustomAsyncOperation : ICustomAsyncOperation, IDisposable
    {
        protected abstract IEnumerator OnProcessRoutine();
        protected IEnumerator RootRoutine { get; set; }
        protected Stack<IEnumerator> RoutineStack { get; } = new Stack<IEnumerator>();
        public virtual OperationState State { get; protected set; }
        public virtual bool IsComplete { get; protected set; }
        public virtual string Error { get; protected set; }
        public virtual string ErrorStackTrace { get; protected set; }
        public virtual object Current => null;

        public CustomAsyncOperation()
        {
            InitWithoutStart();
        }

        public virtual List<string> GetExtraInfoStrings()
        {
            return new List<string>() {
                State.ToText("State"),
                IsComplete.ToText("IsComplete"),
                Error.ToText("Error")
            };
        }

        protected virtual void InitWithoutStart()
        {
            State = OperationState.InProgress;
            RootRoutine = OnProcessRoutine();
            RoutineStack.Push(RootRoutine);
        }

        public virtual bool MoveNext()
        {
            if (IsComplete)
            {
                return false;
            }

            IEnumerator currentRoutine = null;
            if (!RoutineStack.TryPeek(out currentRoutine))
            {
                IsComplete = true;
                State = OperationState.Success;
                return false;
            }
            
            try
            {
                bool next = currentRoutine.MoveNext();
                if (!next)
                {
                    RoutineStack.TryPop(out _);
                    return CheckMoveNext(currentRoutine);
                }
                var returnValue = currentRoutine.Current;
                return CustomYieldReturnValueMoveNext(returnValue);
            }
            catch(Exception e)
            {
                SetError(e);
                return false;
            }
        }

        protected bool CheckMoveNext(IEnumerator routine)
        {
            // Check Existing Errors
            if (IsComplete
                || TryProcessReturnError(routine))
            {
                Dispose();
                return false;
            }

            // Has next step
            if (RoutineStack.Count > 0)
            {
                return true;
            }
            else
            {
                // All Finish
                State = OperationState.Success;
                IsComplete = true;
                Dispose();
                return false;
            }
        }

        protected bool CustomYieldReturnValueMoveNext(object returnValue)
        {
            if (null == returnValue)
            {
                return true ;
            }

            if (returnValue is IEnumerator sub)
            {
                RoutineStack.Push(sub);
                return true;
            }
            else if (returnValue is YieldInstruction yi)
            {
                // Unity native YieldInstruction has no way to get keepWaiting value in edit mode.
                // So handle some of them with default ways.
                RoutineStack.Push(WaitForNativeYieldInstruction(yi));
                return true;
            }
            else if (returnValue is UnityWebRequest webRequest)
            {
                var ie = WaitForWebRequest(webRequest);
                if (IsComplete)
                {
                    return false;
                }
                RoutineStack.Push(ie);
                return true;
            }
            else
            {
                SetError("Not supported yield return type " + returnValue.GetType());
                return false;
            }
        }

        protected virtual void TrySetErrorFrom(CustomAsyncOperation other)
        {
            if (null == other || null == other.Error)
            {
                return;
            }
            IsComplete = true;
            State = OperationState.Failure;
            Error = other.Error;
            ErrorStackTrace = other.ErrorStackTrace;
            Dispose();
        }

        protected virtual void SetError(string errorMsg)
        {
            errorMsg = string.IsNullOrWhiteSpace(errorMsg) ? "An error occurs." : errorMsg;
            SetError(new Exception(errorMsg));
        }

        protected virtual void SetError(Exception e)
        {
            IsComplete = true;
            State = OperationState.Failure;
            Error = e.Message;
            ErrorStackTrace = e.StackTrace;
            Dispose();
        }

        protected void TryDisposeRoutine(IEnumerator routine)
        {
            if (null == routine)
            {
                return;
            }
            IDisposable disposable = routine as IDisposable;
            disposable?.Dispose();
        }
        protected bool TryProcessReturnError(IEnumerator routine)
        {
            if (TryProcessErrorFromObject(routine))
            {
                return true;
            }
            var returnValue = GetFinalReturnValue(routine);
            return TryProcessErrorFromObject(returnValue);
        }

        protected bool TryProcessErrorFromObject(object returnValue)
        {
            if (null == returnValue)
            {
                return false;
            }

            if (returnValue is Exception exc)
            {
                SetError(exc);
                return true;
            }
            else if (returnValue is ICustomAsyncOperation customOp)
            {
                if (!string.IsNullOrWhiteSpace(customOp.Error))
                {
                    SetError(customOp.Error);
                    return true;
                }
            }
            else if (returnValue is UnityWebRequestAsyncOperation webOp)
            {
                if (null == webOp
                    || null == webOp.webRequest)
                {
                    return false;
                }
                return TryProcessErrorFromWebRequest(webOp.webRequest);
            }
            else if (returnValue is UnityWebRequest webRequest)
            {
                return TryProcessErrorFromWebRequest(webRequest);
            }
            return false;
        }

        protected bool TryProcessErrorFromWebRequest(UnityWebRequest webRequest)
        {
            if (null == webRequest 
                || webRequest.result <= UnityWebRequest.Result.Success)
            {
                return false;
            }
            string error = string.IsNullOrWhiteSpace(webRequest.error) ? webRequest.result.ToString() : webRequest.error;
            SetError(error);
            return true;
        }

        private object GetFinalReturnValue(IEnumerator routine)
        {
            if (null == routine)
            {
                return null;
            }

            var v = routine.Current;
            if (null == v)
            {
                return null;
            }
            if (v is ICustomAsyncOperation customAsyncOperation)
            {
                return GetFinalReturnValue(customAsyncOperation);
            }
            if (v is UnityWebRequest 
                || v is UnityWebRequestAsyncOperation)
            {
                return v;
            }

            if (v is IEnumerator sub)
            {
                return GetFinalReturnValue(sub);
            }
            return v;
        }
        protected virtual IEnumerator WaitForWebRequest(UnityWebRequest webRequest)
        {
            if (null == webRequest)
            {
                yield break;
            }
            while(null != webRequest && !webRequest.isDone)
            {
                yield return null;
            }
            TryProcessErrorFromWebRequest(webRequest);
        }

        protected virtual IEnumerator WaitForNativeYieldInstruction(YieldInstruction yi)
        {
            if (null == yi)
            {
                yield break;
            }

            if (Application.isPlaying)
            {
                // On play mode the unity yild instruction is handled by native code and could be run with coroutine.
                yield return yi;
                yield break;
            }

            if (yi is WaitForSeconds waitSec)
            {
                var f = waitSec.GetType().GetField("m_Seconds", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                float t = (float)f.GetValue(waitSec);
                yield return new WaitForSecondsRealtime(t);
                yield break;
            }
            else if (yi is WaitForEndOfFrame)
            {
                yield return null;
                yield break;
            }
            else if (yi is WaitForFixedUpdate)
            {
                yield return new WaitForSecondsRealtime(Time.fixedDeltaTime);
            }
            else if (yi is UnityEngine.AsyncOperation req)
            {
                while(null != req && !req.isDone)
                {
                    yield return null;
                }
                yield break;
            }

            SetError($"Yield instraction type {yi.GetType()} is not support run in edit mode." );
        }

        public virtual void Dispose()
        {
            IEnumerator item = null;
            while(RoutineStack.TryPop(out item))
            {
                if (null == item)
                {
                    continue;
                }
                TryDisposeRoutine(item);
            }
        }

        public void Reset()
        {
            throw new NotSupportedException(
                "Custom Async Operation not support reuse or reset.\r\n" +
                "Please make new instance and try again.");
        }

        public override string ToString()
        {
            return ToString(0);
        }

        public string ToString(int indent)
        {
            string space = new string(' ', Mathf.Max(0, indent));
            string info = $"{space}({this.GetHashCode()}) {this.GetType().Name} ({this.GetType().Namespace}) ";
            var extraInfoLines = GetExtraInfoStrings();
            if (null != extraInfoLines)
            {
                info += "\r\n";
                foreach (var line in extraInfoLines)
                {
                    info += space + "    " + line + "\r\n";
                }
            }
            return info;
        }
    }
}