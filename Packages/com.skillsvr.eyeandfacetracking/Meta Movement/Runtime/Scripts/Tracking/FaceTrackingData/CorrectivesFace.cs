// Copyright (c) Meta Platforms, Inc. and affiliates.

using Newtonsoft.Json;
using Oculus.Interaction;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.XR;
using static OVRFaceExpressions;

namespace Oculus.Movement.Tracking
{
    /// <summary>
    /// Face class that allows applying correctives and/or
    /// modifiers.
    /// </summary>
    public class CorrectivesFace : OVRCustomFace
    {
        /// <summary>
        /// If true, the correctives driver will apply correctives.
        /// </summary>
        public bool CorrectivesEnabled { get; set; }

        /// <summary>
        /// Last updated expression weights.
        /// </summary>
        public float[] ExpressionWeights { get; private set; }

        /// <summary>
        /// Allows one to freeze current values obtained from facial expressions component.
        /// </summary>
        public bool FreezeExpressionWeights { get; set; }

        /// <summary>
        /// Optional blendshape modifier component.
        /// </summary>
        [SerializeField]
        [Optional]
        [Tooltip(CorrectivesFaceTooltips.BlendshapeModifier)]
        protected BlendshapeModifier _blendshapeModifier;

        /// <inheritdoc cref="_blendshapeModifier"/>
        public BlendshapeModifier BlendshapeModifier
        {
            get { return _blendshapeModifier; }
            set { _blendshapeModifier = value; }
        }

        /// <summary>
        /// The json file containing the in-betweens and combinations data.
        /// </summary>
        [SerializeField]
        [Optional]
        [Tooltip(CorrectivesFaceTooltips.CombinationShapesTextAsset)]
        protected TextAsset _combinationShapesTextAsset;

        /// <inheritdoc cref="_combinationShapesTextAsset"/>
        public TextAsset CombinationShapesTextAsset
        {
            get { return _combinationShapesTextAsset; }
            set { _combinationShapesTextAsset = value; }
        }

        /// <summary>
        /// Allows modifying retargeting type field.
        /// </summary>
        public RetargetingType RetargetingTypeField
        {
            get => RetargetingValue;
            set => RetargetingValue = value;
        }

        /// <summary>
        /// Allows modifying duplicates field.
        /// </summary>
        public bool AllowDuplicateMappingField
        {
            get => AllowDuplicateMapping;
            set => AllowDuplicateMapping = value;
        }

        /// <summary>
        /// Cached mesh blendshape values.
        /// </summary>
        private float[] _cachedBlendshapeValues;
        public float[] cachedBlendshapeValues => _cachedBlendshapeValues;
    
        /// <summary>
        /// The correctives module.
        /// </summary>
        protected CorrectivesModule _correctivesModule;

        /// <summary>
        /// Initializes weights and correctives module.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            InitializeExpressionWeights();

            CorrectivesEnabled = true;
            if (_combinationShapesTextAsset != null)
            {
                _correctivesModule = new CorrectivesModule(_combinationShapesTextAsset);
            }
        }

        /// <summary>
        /// Returns OVRFaceExpressions value for the blend shape index provided.
        /// </summary>
        /// <param name="blendShapeIndex">Blend shape index.</param>
        /// <returns>OVRFaceExpression value.</returns>
        public OVRFaceExpressions.FaceExpression GetFaceExpressionForIndex(int blendShapeIndex)
        {
            return GetFaceExpression(blendShapeIndex);
        }

        /// <summary>
        /// Initialize the expression weights array.
        /// </summary>
        protected void InitializeExpressionWeights()
        {
            ExpressionWeights = new float[(int)OVRFaceExpressions.FaceExpression.Max];
            for (int i = 0; i < (int)OVRFaceExpressions.FaceExpression.Max; i++)
            {
                ExpressionWeights[i] = 0.0f;
            }
        }

        /// <summary>
        /// Activates a single face expression and sets the other to zero.
        /// </summary>
        /// <param name="faceExpression">Face expression to activate.</param>
        public void ActivateFaceExpression(OVRFaceExpressions.FaceExpression faceExpression)
        {
            if (ExpressionWeights == null ||
                ExpressionWeights.Length != (int)OVRFaceExpressions.FaceExpression.Max)
            {
                InitializeExpressionWeights();
            }
            InitializeCachedValues();

            int numBlendshapes = SkinnedMesh.sharedMesh.blendShapeCount;
            for (int blendShapeIndex = 0; blendShapeIndex < numBlendshapes; ++blendShapeIndex)
            {
                OVRFaceExpressions.FaceExpression blendShapeToFaceExpression = GetFaceExpression(blendShapeIndex);
                if (blendShapeToFaceExpression >= OVRFaceExpressions.FaceExpression.Max ||
                    blendShapeToFaceExpression < 0)
                {
                    continue;
                }
                ExpressionWeights[(int)blendShapeToFaceExpression] = (blendShapeToFaceExpression == faceExpression) ?
                    1.0f : 0.0f;
            }

            InitializeCachedValues();
            UpdateCachedMeshValues();
            if (_correctivesModule != null && CorrectivesEnabled)
            {
                _correctivesModule.ApplyCorrectives(_cachedBlendshapeValues);
            }

            UpdateSkinnedMesh();
        }

        private InputDevice leftController;
        private InputDevice rightController;

        protected override void Start()
        {
            base.Start();

            List<InputDevice> devices = new List<InputDevice>();

            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller & InputDeviceCharacteristics.TrackedDevice, devices);
            foreach (var device in devices)
            {
                if (device.characteristics.HasFlag(InputDeviceCharacteristics.Right))
                {
                    rightController = device;
                }
            }
        }

        /// <summary>
        /// Applies correctives to values before updating the skinned mesh.
        /// </summary>
        protected override void Update()
        {
            if (ExpressionWeights == null ||
                ExpressionWeights.Length != (int)OVRFaceExpressions.FaceExpression.Max)
            {
                InitializeExpressionWeights();
            }

            if (_faceExpressions.enabled &&
                _faceExpressions.FaceTrackingEnabled &&
                _faceExpressions.ValidExpressions)
            {
                UpdateExpressionWeights();
                InitializeCachedValues();
                UpdateCachedMeshValues();
                if (_correctivesModule != null && CorrectivesEnabled)
                {
                    _correctivesModule.ApplyCorrectives(_cachedBlendshapeValues);
                }

                UpdateSkinnedMesh();

#if false
                if (rightController != null && gameObject.name.Contains("head"))
                {
                    // Check if the B button (Button.Two) is pressed on the left controller
                    if (rightController.TryGetFeatureValue(CommonUsages.primaryButton, out bool isBButtonPressed) && isBButtonPressed)
                    {
                        Dictionary<string, float> myDictionary = new Dictionary<string, float>();
                        foreach (var id in Enum.GetValues(typeof(FaceExpression)))
                        {
                            int index = (int)id;
                            if (_cachedBlendshapeValues.Length > index && index > 0)
                                myDictionary.Add(id.ToString(), _cachedBlendshapeValues[(int)id]);
                        }

                        string json = JsonConvert.SerializeObject(myDictionary);
                        File.WriteAllText(System.IO.Path.Combine(Application.persistentDataPath, "B.json"), json);
                    }

                    // Check if the Y button (Button.Two) is pressed on the right controller
                    if (rightController.TryGetFeatureValue(CommonUsages.secondaryButton, out bool isYButtonPressed) && isYButtonPressed)
                    {
                        // Add your code here to respond to the button press
                        Dictionary<string, float> myDictionary = new Dictionary<string, float>();
                        foreach (var id in Enum.GetValues(typeof(FaceExpression)))
                        {
                            int index = (int)id;
                            if (_cachedBlendshapeValues.Length > index && index > 0)
                                myDictionary.Add(id.ToString(), _cachedBlendshapeValues[(int)id]);
                        }

                        string json = JsonConvert.SerializeObject(myDictionary);
                        File.WriteAllText(System.IO.Path.Combine(Application.persistentDataPath, "A.json"), json);
                    }
                }
#endif
            }
        }

        /// <summary>
        /// Update the expression weight values from face tracking data.
        /// </summary>
        protected void UpdateExpressionWeights()
        {
            if (FreezeExpressionWeights)
            {
                return;
            }

            int numBlendshapes = SkinnedMesh.sharedMesh.blendShapeCount;
            for (int blendShapeIndex = 0; blendShapeIndex < numBlendshapes; ++blendShapeIndex)
            {
                OVRFaceExpressions.FaceExpression blendShapeToFaceExpression = GetFaceExpression(blendShapeIndex);
                if (blendShapeToFaceExpression >= OVRFaceExpressions.FaceExpression.Max ||
                    blendShapeToFaceExpression < 0)
                {
                    continue;
                }
                ExpressionWeights[(int)blendShapeToFaceExpression] = _faceExpressions[blendShapeToFaceExpression];
            }
        }

        /// <summary>
        /// Initialize cached blendshape values.
        /// </summary>
        protected void InitializeCachedValues()
        {
            int numBlendshapes = SkinnedMesh.sharedMesh.blendShapeCount;
            if (_cachedBlendshapeValues == null)
            {
                _cachedBlendshapeValues = new float[numBlendshapes];
                for (int i = 0; i < numBlendshapes; i++)
                {
                    _cachedBlendshapeValues[i] = 0.0f;
                }
            }
        }

        /// <summary>
        /// Update the initialized cached values.
        /// </summary>
        protected void UpdateCachedMeshValues()
        {
            int numBlendshapes = SkinnedMesh.sharedMesh.blendShapeCount;
            for (int blendShapeIndex = 0; blendShapeIndex < numBlendshapes; ++blendShapeIndex)
            {
                OVRFaceExpressions.FaceExpression blendShapeToFaceExpression =
                    GetFaceExpression(blendShapeIndex);
                if (blendShapeToFaceExpression >= OVRFaceExpressions.FaceExpression.Max ||
                    blendShapeToFaceExpression < 0)
                {
                    continue;
                }

                float currentWeight = ExpressionWeights[(int)blendShapeToFaceExpression];
                // Recover true eyes closed values
                if (blendShapeToFaceExpression == OVRFaceExpressions.FaceExpression.EyesClosedL)
                {
                    currentWeight += ExpressionWeights[(int)OVRFaceExpressions.FaceExpression.EyesLookDownL];
                }
                else if (blendShapeToFaceExpression == OVRFaceExpressions.FaceExpression.EyesClosedR)
                {
                    currentWeight += ExpressionWeights[(int)OVRFaceExpressions.FaceExpression.EyesLookDownR];
                }

                if (_blendshapeModifier != null)
                {
                    currentWeight = _blendshapeModifier.GetModifiedWeight(blendShapeToFaceExpression, currentWeight);
                }
                _cachedBlendshapeValues[blendShapeIndex] = currentWeight * BlendShapeStrengthMultiplier;
            }
        }

        /// <summary>
        /// Update the skinned mesh with the cached blendshape values.
        /// </summary>
        protected void UpdateSkinnedMesh()
        {
            var numBlendshapes = _cachedBlendshapeValues.Length;
            for (int blendShapeIndex = 0; blendShapeIndex < numBlendshapes; ++blendShapeIndex)
            {
                SkinnedMesh.SetBlendShapeWeight(blendShapeIndex, _cachedBlendshapeValues[blendShapeIndex]);
            }
        }
    }
}
