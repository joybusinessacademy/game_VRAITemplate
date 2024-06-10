using System;
using UnityEngine;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    // Runtime representation of a TextClip.
    // The Serializable attribute is required to be animated by timeline, and used as a template.
    [Serializable]
    public class LegacyTextPlayableBehaviour : PlayableBehaviour
    {
        [Tooltip("The color of the text")]
        public Color color = Color.white;

        [Tooltip("The text to display")]
        public string text = "";
    }
}
