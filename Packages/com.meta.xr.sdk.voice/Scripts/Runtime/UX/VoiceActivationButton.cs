/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Meta.WitAi;
using Meta.WitAi.Requests;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Oculus.VoiceSDK.UX
{
    [RequireComponent(typeof(Button))]
    public class VoiceActivationButton : MonoBehaviour
    {
        // The button to be observed
        private Button _button;
        // The button label to be adjusted with state
        private Text _buttonLabel;

        [Tooltip("Reference to the current voice service")]
        [SerializeField] private VoiceService _voiceService;

        [Tooltip("Text to be shown while the voice service is not active")]
        [SerializeField] private string _activateText = "Activate";
        [Tooltip("Whether to immediately send data to service or to wait for the audio threshold")]
        [SerializeField] private bool _activateImmediately = false;

        [Tooltip("Text to be shown while the voice service is active")]
        [SerializeField] private string _deactivateText = "Deactivate";
        [Tooltip("Whether to immediately abort request activation on deactivate")]
        [SerializeField] private bool _deactivateAndAbort = false;

        // Current request
        private VoiceServiceRequest _request;
        private bool _isActive = false;

        // Get button & label
        private void Awake()
        {
            _buttonLabel = GetComponentInChildren<Text>();
            _button = GetComponent<Button>();
            if (_voiceService == null)
            {
                _voiceService = FindObjectOfType<VoiceService>();
            }
        }
        // Add click delegate
        private void OnEnable()
        {
            RefreshActive();
            if (_button != null)
            {
                _button.onClick.AddListener(OnClick);
            }
        }
        // Remove click delegate
        private void OnDisable()
        {
            _isActive = false;
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnClick);
            }
        }

        // On click, activate if not active & deactivate if active
        private void OnClick()
        {
            if (!_isActive)
            {
                Activate();
            }
            else
            {
                Deactivate();
            }
        }

        public UnityEvent<string> onComplete;

        // Activate depending on settings
        private void Activate()
        {
            if (!_activateImmediately)
            {
                _request = _voiceService.Activate(GetRequestEvents());
            }
            else
            {
                _request = _voiceService.ActivateImmediately(GetRequestEvents());
            }
        }

        // Deactivate depending on settings
        private void Deactivate()
        {
            if (!_deactivateAndAbort)
            {
                _request.DeactivateAudio();
            }
            else
            {
                _request.Cancel();
            }
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartJRecording();
            }

            if (Input.GetKeyUp(KeyCode.Space))
            {
                StopJRecording();
            }
        }
#endif

        private List<string> strings = new List<string>();
        private bool jRecording = false;
        public void StartJRecording()
        {
            jRecording = true;
            Activate();
        }

        public void StopJRecording()
        {

            jRecording = false;
            Debug.Log(collectedString);

            Deactivate();
            
            onComplete.Invoke(collectedString);

            strings.Clear();
        }

        // Get events
        private VoiceServiceRequestEvents GetRequestEvents()
        {
            VoiceServiceRequestEvents events = new VoiceServiceRequestEvents();
            events.OnInit.AddListener(OnInit);
            events.OnComplete.AddListener(OnComplete);
            return events;
        }
        // Request initialized
        private void OnInit(VoiceServiceRequest request)
        {
            _isActive = true;
            RefreshActive();
        }

        public Text displayText;
        public TMPro.TextMeshProUGUI realDisplay;
        public string collectedString => string.Join("", strings.ToArray()) + displayText.text;

        // Request completed
        private void OnComplete(VoiceServiceRequest request)
        {
            _isActive = false;
            RefreshActive();

            // we still going
            if (
#if UNITY_EDITOR
                Input.GetKey(KeyCode.Space) ||
#endif
                jRecording)
            {
                strings.Add(displayText.text + " ");
                displayText.text = string.Empty;
                Activate();
            }
        }

        private const int maxWords = 50;

        private void FixedUpdate()
        {
            realDisplay.text = collectedString;
            string[] words = realDisplay.text.Split(' ');

            int startWordIndex = Mathf.Max(0, words.Length - maxWords);
            string lastFewWords = string.Join(" ", words, startWordIndex, words.Length - startWordIndex);

            realDisplay.text = lastFewWords;
        }

        // Refresh active text
        private void RefreshActive()
        {
            if (_buttonLabel != null)
            {
                //_buttonLabel.text = _isActive ? _deactivateText : _activateText;
            }
        }
    }
}
