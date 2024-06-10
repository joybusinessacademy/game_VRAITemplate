using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JBA.XRPlayerPackage.XRDebug
{
    public class JRRadialMenu : MonoBehaviour
    {
        public Transform cursorTransform;
        
        public Vector3 touchPosition = Vector2.zero;
        
        private JRRadialSection topRadialSection;
        private JRRadialSection rightRadialSection;
        private JRRadialSection bottomRadialSection;
        private JRRadialSection leftRadialSection;
        private JRRadialSection focusedRadialSection;

        private List<JRRadialSection> radialSections = new List<JRRadialSection>();

        public float radius = 20f;
#if UNITY_EDITOR
        public Vector2 editorTouchPosition;
#endif
        private float cursorAngle;

        private Transform originalParent;

        private void Awake()
        {

#if !ENVIRONMENT_DEVELOPMENT
            gameObject.SetActive(false);
            return;
#endif
            originalParent = transform.parent ?? null;

            radialSections.Add(topRadialSection);
            radialSections.Add(rightRadialSection);
            radialSections.Add(bottomRadialSection);
            radialSections.Add(leftRadialSection);

            radialSections.RemoveAll(item => item == null);
        }

        // Update is called once per frame
        void Update()
        {
#if UNITY_EDITOR
            touchPosition = editorTouchPosition;
            touchPosition.x = Mathf.Clamp(touchPosition.x, -1, 1) * radius;
            touchPosition.y = Mathf.Clamp(touchPosition.y, -1, 1) * radius;
            touchPosition.z = 0;
#else
        touchPosition.x = Input.GetAxis(DebugControlManager.occulusRightStickHorizontalAxis)*radius;
        touchPosition.y = Input.GetAxis(DebugControlManager.occulusRightStickVerticalAxis)*radius;
#endif
            Vector2 direction = touchPosition;
            cursorAngle = GetAtan2Angle(direction);

            SetCursorPosition(cursorTransform, touchPosition);

            SetFocusedRadial(cursorAngle);

            if (Input.GetButtonUp(DebugControlManager.occulusRightStickButton))
            {
                ExecuteRadialSectionEvent();
            }
        }

        public void SetParent(Transform transform)
        {
            gameObject.transform.SetParent(transform);
        }

        public void SetFocusedRadial(float angle)
        {
            bool inRightSection = (angle > 45 && angle <= 135);
            bool inBottomSection = (angle > 135 && angle <= 225);
            bool inLeftSection = (angle > 225 && angle <= 315);

            JRRadialSection currentfocusedRadialSection = focusedRadialSection;

            if (inRightSection)
            {
                currentfocusedRadialSection = rightRadialSection;
            }
            else if (inBottomSection)
            {
                currentfocusedRadialSection = bottomRadialSection;
            }
            else if (inLeftSection)
            {
                currentfocusedRadialSection = leftRadialSection;
            }
            else
            {
                currentfocusedRadialSection = topRadialSection;
            }

            // Allows for no option to be selected with a chance for human error
            if (touchPosition.x <= radius / 5 && touchPosition.x >= -radius / 5)
            {
                if (touchPosition.y <= radius / 5 && touchPosition.y >= -radius / 5)
                {
                    currentfocusedRadialSection = null;
                }
            }

            if (currentfocusedRadialSection != focusedRadialSection)
            {
                focusedRadialSection = currentfocusedRadialSection;
                HighlightFocusedRadialSection();
            }
        }

        private void HighlightFocusedRadialSection()
        {
            if (radialSections.Count < 1)
            {
                Debug.LogError("radialSections are not set");
                return;
            }

            foreach (var item in radialSections)
            {
                item.SetFocusedSection(item == focusedRadialSection);
            }
        }
        
        private void ExecuteRadialSectionEvent()
        {
            if (focusedRadialSection?.onSelectEvent == null)
            {
                gameObject.SetActive(false);
                return;
            }

            focusedRadialSection.onSelectEvent.Invoke();
        }

        private float GetAtan2Angle(Vector2 direction)
        {
            float angle = Mathf.Atan2(direction.x, direction.y);
            angle *= Mathf.Rad2Deg;

            angle = angle < 0 ? angle + 360 : angle;

            return angle;
        }

        private void SetCursorPosition(Transform cursor, Vector3 newPosition)
        {
            cursor.localPosition = newPosition;
        }
    }
}