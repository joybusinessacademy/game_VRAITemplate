using UnityEngine;
using UnityEngine.UIElements;

namespace Scripts.VisualElements
{
    public static class IStyleExtensions
    {
        public static void SetBorderColor(this IStyle style, Color color)
        {
            SetBorderColor(style, color, color, color, color);
        }
            public static void SetBorderColor(this IStyle style, Color leftColor, Color topColor, Color rightColor, Color bottomColor)
        {
            style.borderTopColor = new StyleColor(leftColor);
            style.borderBottomColor = new StyleColor(topColor);
            style.borderLeftColor = new StyleColor(rightColor);
            style.borderRightColor = new StyleColor(bottomColor);
        }

        public static void SetBorderWidth(this IStyle style, int width, bool round = false)
        {
            style.borderLeftWidth = width;
            style.borderTopWidth = width;
            style.borderRightWidth = width;
            style.borderBottomWidth = width;

            style.borderTopLeftRadius = round ? width : 0;
            style.borderTopRightRadius = round ? width : 0;
            style.borderBottomLeftRadius = round ? width : 0;
            style.borderBottomRightRadius = round ? width : 0;
        }

        public static void SetBorderWidth(this IStyle style, int left, int top, int right, int bottom, bool round = false)
        {
            style.borderLeftWidth = left;
            style.borderTopWidth = top;
            style.borderRightWidth = right;
            style.borderBottomWidth = bottom;

            style.borderTopLeftRadius = round ? Mathf.Min(left, top) : 0;
            style.borderTopRightRadius = round ? Mathf.Min(right, top) : 0;
            style.borderBottomLeftRadius = round ? Mathf.Min(left, bottom) : 0;
            style.borderBottomRightRadius = round ? Mathf.Min(right, bottom) : 0;
        }
    }
}