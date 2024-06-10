using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVR.CCK.PackageManager.UI.Views
{
    public class Splitter : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<Splitter, UxmlTraits> { }
        
        public Splitter()
        {
            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
            this.SetCursor(MouseCursor.ResizeHorizontal);
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            StopDrag();
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (null == parent)
            {
                return;
            }

            if (0 == evt.button)
            {
                parent.RegisterCallback<MouseUpEvent>(OnMouseUp);
                parent.RegisterCallback<MouseMoveEvent>(OnMouseMove);
                parent.RegisterCallback<MouseOutEvent>(OnMouseOut);
            }
        }
        private void StopDrag()
        {
            if (null == parent)
            {
                return;
            }
            parent.UnregisterCallback<MouseUpEvent>(OnMouseUp);
            parent.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            parent.UnregisterCallback<MouseOutEvent>(OnMouseOut);
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (evt.button == 0)
            {
                StopDrag();
            }
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (null == parent)
            {
                StopDrag();
                return;
            }
            OnDrag(evt.mouseDelta);
        }

        
       
        private void OnMouseOut(MouseOutEvent evt)
        {
            if (null == parent)
            {
                return;
            }
            if (parent.worldBound.Contains(evt.mousePosition))
            {
                return;
            }
            StopDrag();
        }

        private void OnDrag(Vector2 mouseDelta)
        {
            if (null == parent)
            {
                return;
            }
            var children = parent.Children().ToList();

            var myIndex = children.IndexOf(this);
            var prevIndex = Mathf.Max(0, myIndex - 1);
            var nextIndex = Mathf.Min(children.Count - 1, myIndex + 1);

            if (myIndex == prevIndex || myIndex == nextIndex)
            {
                return;
            }

            var prevItem = children[prevIndex];
            var nextItem = children[nextIndex];
            var dir = parent.style.flexDirection.value;
            switch (dir)
            {
                case FlexDirection.Column:
                case FlexDirection.ColumnReverse:
                    if (null != prevItem)
                    {
                        prevItem.style.width = prevItem.worldBound.width + mouseDelta.x;
                    }
                    if (null != nextItem)
                    {
                        nextItem.style.width = nextItem.worldBound.width - mouseDelta.x;
                    }
                    break;
                case FlexDirection.Row:
                case FlexDirection.RowReverse:
                    if (null != prevItem)
                    {
                        prevItem.style.height = prevItem.worldBound.height + mouseDelta.x;
                    }
                    if (null != nextItem)
                    {
                        nextItem.style.height = nextItem.worldBound.height - mouseDelta.x;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}