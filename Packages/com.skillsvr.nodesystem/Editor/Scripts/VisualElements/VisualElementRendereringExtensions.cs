using GraphProcessor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scripts.VisualElements
{
	public static class VisualElementRendereringExtensions
	{
		public static T FindParentByType<T>(this VisualElement visual, bool inherit = true) where T : VisualElement
		{
			return visual.FindParentByType(typeof(T), inherit) as T;
		}

		public static VisualElement FindParentByType(this VisualElement visual, Type type, bool inherit = true)
		{
			if (null == visual || null == type)
			{
				return null;
			}
			VisualElement tester = visual;

			while (null != tester)
			{
				if (tester.GetType() == type
					|| (inherit && type.IsAssignableFrom(tester.GetType())))
				{
					return tester;
				}
				tester = tester.parent;
			}
			return null;
		}

		public static VisualElement QueryInChain<T1, T2>(this VisualElement root) where T1: VisualElement where T2 : VisualElement
		{
			if (null == root)
			{
				return null;
			}
			var t1 = root.Q<T1>();
			if (null == t1)
			{
				return null;
			}
			var t2 = t1.Q<T2>();
			return t2;
		}

		public static VisualElement QueryInChain<T1, T2, T3>(this VisualElement root) 
			where T1 : VisualElement 
			where T2 : VisualElement
			where T3 : VisualElement
		{
			var t = root.QueryInChain<T1, T2>();
			return null == t ? null : t.Q<T3>();
		}

		public static VisualElement SetBackgroundImage(this VisualElement visual, Texture2D image)
		{
			visual.style.backgroundImage = image;
			return visual;
		}

        public static VisualElement SetBackgroundImage(this VisualElement visual, Texture2D image, Color color, float alpha = 1.0f)
        {
            visual.style.backgroundImage = image;
            return visual.SetunityBackgroundImageTintColor(color, alpha);
        }

        public static VisualElement SetBackgroundImage(this VisualElement visual, string resourcePath)
		{
			visual.style.backgroundImage = Resources.Load<Texture2D>(resourcePath);
			return visual;
		}

        public static VisualElement SetBackgrounImage(this VisualElement visual, string resourcePath, Color color, float alpha = 1.0f)
        {
            visual.style.backgroundImage = Resources.Load<Texture2D>(resourcePath);
            return visual.SetunityBackgroundImageTintColor(color, alpha);
        }

        public static VisualElement SetBackgroundColor(this VisualElement visual, Color color)
		{
			visual.style.backgroundColor = color;
			return visual;
		}

		public static VisualElement SetunityBackgroundImageTintColor(this VisualElement visual, Color color)
		{
			visual.style.unityBackgroundImageTintColor = color;
			return visual;
		}
		public static VisualElement SetBackgroundColor(this VisualElement visual, Color color, float alpha)
		{
			var c = color;
			c.a = alpha;
			visual.style.backgroundColor = c;
			return visual;
		}

		public static VisualElement SetunityBackgroundImageTintColor(this VisualElement visual, Color color, float alpha)
		{
			var c = color;
			c.a = alpha;
			visual.style.unityBackgroundImageTintColor = c;
			return visual;
		}

		public static VisualElement SetBackgroundColorAlpha01(this VisualElement visual, float alpha)
		{
			var color = visual.style.backgroundColor.value;
			color.a = alpha;
			visual.style.backgroundColor = color;
			return visual;
		}

		public static VisualElement DisplayInRow(this VisualElement visual, bool reverse = false)
		{
			visual.style.flexDirection = reverse ? FlexDirection.RowReverse : FlexDirection.Row;
			return visual;
		}

        public static VisualElement DisplayInColumn(this VisualElement visual, bool reverse = false)
        {
            visual.style.flexDirection = reverse ? FlexDirection.ColumnReverse : FlexDirection.Column;
            return visual;
        }

        public static VisualElement AlignSelf(this VisualElement visual, Align value)
        {
            visual.style.alignSelf = value;
            return visual;
        }

        public static VisualElement AlignSelfCenter(this VisualElement visual)
        {
            visual.style.alignSelf = Align.Center;
            return visual;
        }

		public static VisualElement MinWidth(this VisualElement visual, int value)
		{
			visual.style.minWidth = value;
			return visual;
		}
        public static VisualElement MaxWidth(this VisualElement visual, int value)
        {
            visual.style.maxWidth = value;
            return visual;
        }

        public static VisualElement MinHeight(this VisualElement visual, int value)
        {
            visual.style.minHeight = value;
            return visual;
        }
        public static VisualElement MaxHeight(this VisualElement visual, int value)
        {
            visual.style.maxHeight = value;
            return visual;
        }

        public static VisualElement SetMargin(this VisualElement visual, float value)
		{
			visual.style.marginBottom = value;
			visual.style.marginTop = value;
			visual.style.marginLeft = value;
			visual.style.marginRight = value;
			return visual;
		}

		public static VisualElement SetMargin(this VisualElement visual, float left, float top, float right, float bottom)
		{
			visual.style.marginBottom = bottom;
			visual.style.marginTop = top;
			visual.style.marginLeft = left;
			visual.style.marginRight = right;
			return visual;
		}

		public static VisualElement SetPadding(this VisualElement visual, float value)
		{
			visual.style.paddingBottom = value;
			visual.style.paddingTop = value;
			visual.style.paddingLeft = value;
			visual.style.paddingRight = value;
			return visual;
		}

		public static VisualElement SetTextAlign(this VisualElement visual, TextAnchor value)
		{
			visual.style.unityTextAlign = value;
			return visual;
		}

        public static VisualElement SetTextAlignCenter(this VisualElement visual)
        {
            visual.style.unityTextAlign = TextAnchor.MiddleCenter;
            return visual;
        }

        public static VisualElement StretchWidth(this VisualElement visual, float value = 1.0f)
        {
			visual.style.flexGrow = value;
            return visual;
        }
        public static VisualElement StretchHeight(this VisualElement visual, float value = 1.0f)
        {
            visual.style.flexShrink = value;
            return visual;
        }

        public static VisualElement SetSize(this VisualElement visual, int width, int height)
		{
			visual.style.width = width;
			visual.style.height = height;
			return visual;
		}

		public static VisualElement SetSize(this VisualElement visual, int size)
		{
			visual.style.width = size;
			visual.style.height = size;
			return visual;
		}

		public static VisualElement SetSize(this VisualElement visual, Vector2 size)
		{
			visual.style.width = size.x;
			visual.style.height = size.y;
			return visual;
		}

        public static VisualElement LockSize(this VisualElement visual, int width, int height)
        {
            visual.style.width = width;
            visual.style.height = height;
            visual.style.minWidth = width;
            visual.style.minHeight = height;
            visual.style.maxWidth = width;
            visual.style.maxHeight = height;
            return visual;
		}

		public static VisualElement LockSize(this VisualElement visual, int size)
		{
			visual.style.width = size;
			visual.style.height = size;
			visual.style.minWidth = size;
			visual.style.minHeight = size;
			visual.style.maxWidth = size;
			visual.style.maxHeight = size;
			return visual;
		}
        public static VisualElement LockSize(this VisualElement visual, Vector2 size)
        {
            visual.style.width = size.x;
            visual.style.height = size.y;
            visual.style.minWidth = size.x;
            visual.style.minHeight = size.y;
            visual.style.maxWidth = size.x;
            visual.style.maxHeight = size.y;
            return visual;
        }

        public static VisualElement WrapText(this VisualElement visual, bool wrap = true)
        {
            visual.style.whiteSpace = wrap ? WhiteSpace.Normal : WhiteSpace.NoWrap;
            return visual;
        }

        public static VisualElement GetOrCreatChildByName(this VisualElement visual, string name)
		{
			var item = visual.Children().Where(x=> x.name == name).FirstOrDefault();
			if (null == item)
			{
				item = new VisualElement();
				item.name = name;
				visual.Add(item);
			}
			return item;
		}

		public static void ExecOnceOnEvent<EVENT_TYPE>(this VisualElement element, EventCallback<EVENT_TYPE> customEventCallback) where EVENT_TYPE : EventBase<EVENT_TYPE>, new()
		{
			if (null == element || null == customEventCallback)
			{
				return;
			}

			EventCallback<EVENT_TYPE> callbackToRemove = null;
			EventCallback<EVENT_TYPE> selfUnregisterEventCallback = (evt) => {
				if (null != callbackToRemove)
				{
					element?.UnregisterCallback<EVENT_TYPE>(callbackToRemove);
				}
				customEventCallback?.Invoke(evt);
			};
			callbackToRemove = selfUnregisterEventCallback;
			element.RegisterCallback<EVENT_TYPE>(selfUnregisterEventCallback);
		}

		public static void ExecOnceOnRenderReady(this VisualElement element, Action customAction)
		{
			if (null == element || null == customAction)
			{
				return;
			}	

			if (element.IsRenderReady())
			{
				customAction?.Invoke();
				return;
			}

			element.ExecOnceOnEvent<GeometryChangedEvent>((evt) => { customAction?.Invoke(); });
		}


		public static bool IsRenderReady(this VisualElement element)
		{
			if (null == element)
			{
				return false;
			}
			var rect = element.worldBound;
			return !(float.IsNaN(rect.width) || float.IsNaN(rect.height));
		}


		public static bool CopySizeFrom(this VisualElement itemToResize, VisualElement source, bool useLocalSize = true)
		{
			if (null == itemToResize || null == source)
			{
				return false;
			}

			Rect targetRect = useLocalSize ? source.localBound : source.worldBound;
			itemToResize.style.width = targetRect.width;
			itemToResize.style.height = targetRect.height;
			return float.NaN != itemToResize.style.height && float.NaN != itemToResize.style.width;
		}

		public static bool CopyPositionFrom(this VisualElement itemToMove, VisualElement source, bool useLocalPos = true)
		{
			if (null == itemToMove || null == source)
			{
				return false;
			}

			Rect targetRect = useLocalPos ? source.localBound : source.worldBound;
			itemToMove.style.top = useLocalPos ? -1.0f * source.style.borderTopWidth.value : targetRect.yMin;
			itemToMove.style.left = useLocalPos ? -1.0f * source.style.borderLeftWidth.value : targetRect.xMin;
			return float.NaN != itemToMove.style.top && float.NaN != itemToMove.style.left;
		}

		public static bool CopyPosAndSizeFrom(this VisualElement distItem, VisualElement source, bool useLocal = true)
		{
			return CopySizeFrom(distItem, source, useLocal) && CopyPositionFrom(distItem, source, useLocal);
		}

		public static bool CopyOffsetPosition(this VisualElement distItem, VisualElement root, VisualElement source)
		{
			Rect sourceRect = source.worldBound;
			Rect rootRect = root.worldBound;
			distItem.style.top = sourceRect.yMin - rootRect.yMin;
			distItem.style.left = sourceRect.xMin - rootRect.xMin;
			return float.NaN != distItem.style.top && float.NaN != distItem.style.left;
		}

		public static void MoveToWorldPosition(this VisualElement itemToMove, Vector2 pos)
		{
			var scale = itemToMove.worldTransform.lossyScale;
			var offsetPos = null == itemToMove.parent ? pos : pos - itemToMove.parent.worldBound.position;
			itemToMove.style.top = offsetPos.y / scale.y;
			itemToMove.style.left = offsetPos.x/ scale.x;
		}

		public static void MoveTo(this VisualElement itemToMove, VisualElement target, Vector2 offset)
		{
			var toPos = target.worldBound.position;
			MoveToWorldPosition(itemToMove, toPos + offset);
		}

		public static void MoveTo(this VisualElement itemToMove, VisualElement target)
		{
			MoveTo(itemToMove, target, Vector2.zero);
		}

		public static void Move(this VisualElement visual, int x, int y)
		{
			visual.MoveTo(visual, new Vector2(x, y));
		}

		public static Vector2 GetWorldBoundTopRight(this VisualElement visual)
		{
			return new Vector2(visual.worldBound.max.x, visual.worldBound.min.y);
		}

		public static Vector2 GetWorldBoundLeftBottom(this VisualElement visual)
		{
			return new Vector2(visual.worldBound.min.x, visual.worldBound.max.y);
		}

		public static Vector2 GetStyleSize(this VisualElement visual)
		{
			return new Vector2(visual.style.width.value.value, visual.style.height.value.value);
		}

		public static Vector2 GetworldTransformSize(this VisualElement visual)
		{
			var scale = visual.worldTransform.lossyScale;
			var size = visual.GetStyleSize();
			size.x = 0.0f == scale.x ? size.x : size.x * scale.x;
			size.y = 0.0f == scale.y ? size.y : size.y * scale.y;
			return size;
		}

		public static Rect GetScaleWorldBound(this VisualElement visual)
		{
			var scale = visual.worldTransform.lossyScale;
			var rect = visual.worldBound;
			rect.xMax *= scale.x;
			rect.xMin *= scale.x;
			rect.yMin *= scale.y;
			rect.yMax *= scale.y;
			return rect;
		}
	}
}