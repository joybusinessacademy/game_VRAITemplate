using System;
using GraphProcessor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Android;
using System.Collections;
using SkillsVR.Mechanic.Core;
using Unity.XR.Oculus;


namespace SkillsVRNodes
{
    [RequireComponent(typeof(AudioSource))]
    public class MainGraphExecutorComponent : MonoBehaviour
    {
        [Header("Graph to Run on Start")]
        public MainGraph graph;

        public SceneNodeExecutor NodeExecutor;

        public UnityEvent onGraphComplete;

        private const string ECRecordOnCompleteMethod = "ECSubmitScoreThenQuitApplication";

        public AudioSource SceneAudioSource
        {
            get
            {
                return this.GetComponent<AudioSource>();
            }
        }

        public void FadeInAudio(float duration)
        {
            StartCoroutine(FadeInAudioCoroutine(duration));
        }

        private IEnumerator FadeInAudioCoroutine(float duration)
        {
            while (Math.Abs(SceneAudioSource.volume - 1) > 0.01f)
            {
                yield return new WaitForFixedUpdate();
                SceneAudioSource.volume += Time.fixedTime / duration;
            }
            SceneAudioSource.volume = 1;
        }

        public void FadeOutAudio(float duration)
        {
            StartCoroutine(FadeOutAudioCoroutine(duration));
        }

        private IEnumerator FadeOutAudioCoroutine(float duration)
        {
            while (SceneAudioSource.volume != 0)
            {
                yield return new WaitForFixedUpdate();
                SceneAudioSource.volume -= Time.fixedTime / duration;
            }
            SceneAudioSource.volume = 0;
        }

        private void Awake()
        {
            SceneAudioSource.volume = 0;
            NodeExecutor = new SceneNodeExecutor(graph);

            NodeExecutor.InitializeGraph();

            NodeExecutor.onEndAction += () => onGraphComplete.Invoke();

            // leave onGraphComplete for inspector controlled
            NodeExecutor.onEndAction += () =>
            {

                //TODO: Remove Mechanics package Reference
                var logExporter = FindObjectOfType<LogExporter>();
                if (logExporter != null)
                    logExporter.WriteDataToFile();

                // quitting the app now, i dont mind using GameObject.Find
                var restCoreObject = GameObject.Find("RESTService");
                restCoreObject.SendMessage(ECRecordOnCompleteMethod, SendMessageOptions.DontRequireReceiver);
            };

        }

        private void Start()
        {
            StartCoroutine(ConfirmPermission());
        }

        private IEnumerator ConfirmPermission()
        {
            yield return new WaitForEndOfFrame();

#if PLATFORM_ANDROID
            while (!CheckAllPermissions())
            {

                if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
                {

                    Permission.RequestUserPermission(Permission.Microphone);
                    yield return new WaitUntil(() => Permission.HasUserAuthorizedPermission(Permission.Microphone));
                }

#if PICO_XR
                if (SystemInfo.deviceModel.Contains("Pico A8E50"))
                {
                    if (!Permission.HasUserAuthorizedPermission("com.picovr.permission.EYE_TRACKING"))
                    {
                        Permission.RequestUserPermission("com.picovr.permission.EYE_TRACKING");
                        yield return new WaitUntil(() => Permission.HasUserAuthorizedPermission("com.picovr.permission.EYE_TRACKING"));
                    }
                    if (!Permission.HasUserAuthorizedPermission("com.picovr.permission.FACE_TRACKING"))
                    {
                        Permission.RequestUserPermission("com.picovr.permission.FACE_TRACKING");
                        yield return new WaitUntil(() => Permission.HasUserAuthorizedPermission("com.picovr.permission.FACE_TRACKING"));
                    }
                }
#else

                if (Unity.XR.Oculus.Utils.GetSystemHeadsetType() == SystemHeadset.Placeholder_10)
                {
                    if (!Permission.HasUserAuthorizedPermission("com.oculus.permission.EYE_TRACKING"))
                    {
                        Permission.RequestUserPermission("com.oculus.permission.EYE_TRACKING");
                        yield return new WaitUntil(() => Permission.HasUserAuthorizedPermission("com.oculus.permission.EYE_TRACKING"));
                    }
                    if (!Permission.HasUserAuthorizedPermission("com.oculus.permission.FACE_TRACKING"))
                    {
                        Permission.RequestUserPermission("com.oculus.permission.FACE_TRACKING");
                        yield return new WaitUntil(() => Permission.HasUserAuthorizedPermission("com.oculus.permission.FACE_TRACKING"));
                    }
                }
#endif

            }
#endif

            PlayerDistributer.GetPlayer(SystemInfo.deviceUniqueIdentifier);
            StartCoroutine(WaitPoolInitAndDelayStart(2.0f));
        }

        bool CheckAllPermissions()
        {
            bool rtn = Permission.HasUserAuthorizedPermission(Permission.Microphone);


#if PICO_XR
            if (SystemInfo.deviceModel.Contains("Pico A8E50"))
            {
                rtn = (Permission.HasUserAuthorizedPermission(Permission.Microphone) &&
                Permission.HasUserAuthorizedPermission("com.picovr.permission.EYE_TRACKING") &&
                Permission.HasUserAuthorizedPermission("com.picovr.permission.FACE_TRACKING"));
            }
#else


            if (Unity.XR.Oculus.Utils.GetSystemHeadsetType() == SystemHeadset.Placeholder_10)
                {
                rtn = (Permission.HasUserAuthorizedPermission(Permission.Microphone) &&
                        Permission.HasUserAuthorizedPermission("com.oculus.permission.EYE_TRACKING") &&
                        Permission.HasUserAuthorizedPermission("com.oculus.permission.FACE_TRACKING"));
            }
#endif
            return rtn;

        }

        private bool isInitProgressing;
        private IEnumerator WaitPoolInitAndDelayStart(float timeoutInSec = 15.0f)
        {
            var pool = MechanicProvider.Current.ConvertToInterface<IPooledMechanicProvider>();
            if (null == pool)
            {
                // If not pool, start directly
                NodeExecutor.Start();
                yield break;
            }

            Debug.Log("Mechanic pool detected, main graph waits for mechanics ready.");
            isInitProgressing = true;
            // otherwise start after pool objects ready.
            pool.AddOneTimeAllObjectsReadyListener(OnPoolInitReady);

            yield return new WaitForSeconds(Mathf.Max(1, timeoutInSec));
            // if timeout, start anyway
            if (isInitProgressing)
            {
                isInitProgressing = false;
                pool.RemoveAllObjectsReadyListener(OnPoolInitReady);
                Debug.LogError("Timeout for mechanic init! Start main graph anyway.");
                NodeExecutor.Start();
            }
        }

        private void OnPoolInitReady()
        {
            Debug.Log("Mechanic pool init ready, start main graph.");
            isInitProgressing = false;
            NodeExecutor.Start();
        }

        public void AddListener(Action action)
        {
            NodeExecutor.onEndAction += action;
        }
    }
}
