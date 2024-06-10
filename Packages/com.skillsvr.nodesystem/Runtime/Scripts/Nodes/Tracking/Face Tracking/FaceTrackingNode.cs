using System;
using System.Linq;
using GraphProcessor;
using SkillsVRNodes.ScriptableObjects;
using UnityEngine;
using UnityEngine.Android;
using Object = UnityEngine.Object;

namespace SkillsVRNodes.Scripts.Nodes.Nodes
{
    [Serializable, NodeMenuItem("Telemetry/Face Tracking", typeof(SceneGraph)), NodeMenuItem("Telemetry/Face Tracking", typeof(SubGraph))]
    public class FaceTrackingNode : ExecutableNode
    {

        private GameObject trackingObject;
        private string trackingObjectName = "Tracking Director";

        public EmotionAsset activeEmotionAsset;
        
        public bool isStopNode;
        public float nodeDuration;
        public float targetEmotionDuration;

        public override string name => "Face Tracking";
        public override string icon => "Player";

        public override Color color => NodeColours.UserRig;

        [Output(name = "Pass")] public ConditionalLink passCondition;

        [Output(name = "Fail")] public ConditionalLink failCondition;

        private bool showingActiveEmotion;
        private float targetTimer;
        //private bool showedActiveEmotion;
        public FloatSO alteredVariableSO;

        protected override void OnStart()
        {
           
            base.OnStart();
#if PICO_XR
            if (!Permission.HasUserAuthorizedPermission("com.picovr.permission.FACE_TRACKING"))
            {
                CompleteNode();
                return;
            }
#else
            if (!Permission.HasUserAuthorizedPermission("com.oculus.permission.FACE_TRACKING"))
            {
                CompleteNode();
                return;
            }
					
#endif


            if (isStopNode)
            {
                StopRecord();
                return;
            }

            StartRecord();

            if (nodeDuration > 0)
            {
                WaitMonoBehaviour.Process(nodeDuration, () =>
                {
                    StopRecord();
                });
            }
            targetTimer = 0;
        }


        private void SetUpTrackingObject()
        {
            trackingObject = GameObject.Find(trackingObjectName + "(Clone)");
            if (trackingObject == null)
            {
                trackingObject = Resources.Load(trackingObjectName) as GameObject;
                if (trackingObject != null)
                    trackingObject = GameObject.Instantiate(trackingObject);
            }

            trackingObject.GetComponent<TrackingDirector>().Begin(UpdateData, this);
        }

        private void UpdateData(FaceTrackingFrame frame)
        {
            float avValueforthisFrame = 0;
            showingActiveEmotion = false;
            float value = 0;

#if PICO_XR
            activeEmotionAsset.picoData.ForEach(k =>
                        {
                            value = frame.blendShapeWeight[(int)k.pxr_FaceExpression];
                           CaluateFrame(value, ref avValueforthisFrame, k.weight, k.min, k.max);

                        });
#else
					            activeEmotionAsset.ocData.ForEach(k =>
                        {
                            value = frame.blendShapeWeight[(int)k.faceExpression];
                            CaluateFrame(value, ref avValueforthisFrame, k.weight, k.min, k.max);

                        });
					
#endif
            if ((avValueforthisFrame > 90f && avValueforthisFrame < 110f) || showingActiveEmotion)
                targetTimer += Time.deltaTime;

            //debug
            alteredVariableSO.value = avValueforthisFrame;
        }

        private void CaluateFrame(float val, ref float averageThisFrame, float weight, float min, float max)
        {

            //Method one:
            //flag will remain true if every single blend shape value falls between their min / max
            showingActiveEmotion &= min <= val && max >= val;

            //Method Two:
            //scale the averages with the specific blend shapes weighting.				
            //get this shapes av;
            float blendShapeAv = (val / ((max + min) * 0.5f)) * 100f;
            //scale by it's weight
            float weightedAv = (weight * 0.01f) * blendShapeAv;
            //acculmulate the weighted avs for the whole frame, 100 is max.
            averageThisFrame += weightedAv;
        }

        private void StartRecord()
        {
            if (Application.isEditor)
            {
                var overrider = new GameObject().AddComponent<FaceTrackingOverride>();
                overrider.onInput += StopRecord;
            }

            SetUpTrackingObject();
        }

        private void StopRecord()
        {

            if (trackingObject != null)
                trackingObject.GetComponent<TrackingDirector>().End(this);

            TrackingFrameComponent.CollectDataFace -= UpdateData;

            CompleteNode();
            RunLink(GetPass() ? nameof(passCondition) : nameof(failCondition));
        }

        public bool GetPass()
        {

            if (Application.isEditor)
            {
                FaceTrackingOverride faceTrackingOverride = Object.FindObjectOfType<FaceTrackingOverride>();
                bool value = faceTrackingOverride != null && faceTrackingOverride.pass;
                GameObject.DestroyImmediate(faceTrackingOverride.gameObject);
                return value;
            }

            return targetTimer > targetEmotionDuration;
        }
    }
}