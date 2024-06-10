using System.Collections;
using UnityEngine;

namespace SkillsVR.Telemetry.Networking.API
{
    public class WaitForCCKLogin : IEnumerator
    {
        private const string CCK_LOGGEDIN = "CCK_USERLOGGEDIN";

        public object Current => action.Current;

        public bool IsDone { get; protected set; }

        IEnumerator action;
        public WaitForCCKLogin()
        {
            IsDone = false;
            action = WaitForLogin();
        }

        public bool MoveNext()
        {
            return action.MoveNext();
        }

        public void Reset()
        {
            IsDone = false;
        }

        protected IEnumerator WaitForLogin()
        {
            IsDone = false;
#if UNITY_EDITOR
            while(!UnityEditor.SessionState.GetBool(CCK_LOGGEDIN, false))
            {
                var op = new WaitForSecondsRealtime(1.0f);
                while(op.keepWaiting)
                {
                    yield return null;
                }
            }
#endif

            var waitForLoginPanelDispear = new WaitForSecondsRealtime(1.0f);
            while (waitForLoginPanelDispear.keepWaiting)
            {
                yield return null;
            }
            IsDone = true;
        }
    }
}
