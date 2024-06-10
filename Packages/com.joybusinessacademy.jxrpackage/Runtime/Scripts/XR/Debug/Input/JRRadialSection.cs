using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JBA.XRPlayerPackage.XRDebug
{
    [RequireComponent(typeof(Image))]
    public class JRRadialSection : MonoBehaviour
    {
        public UnityEngine.Events.UnityEvent onSelectEvent;

        public Color startColour;
        public Color focusedColour = new Color(118, 118, 118, 0.85f);

        private JRRadialMenu radialMenu;

        private Image sectorImage;

        private void Awake()
        {
            sectorImage = GetComponent<Image>();
            startColour = sectorImage.color;
            radialMenu = FindObjectOfType<JRRadialMenu>();
            onSelectEvent.AddListener(() => CloseRadialMenu());
        }

        internal void SetFocusedSection(bool isfocused)
        {
            if (onSelectEvent != null)
                sectorImage.color = isfocused ? focusedColour : startColour;
        }

        public void CloseRadialMenu()
        {
            radialMenu.gameObject.SetActive(false);
        }

    }
}