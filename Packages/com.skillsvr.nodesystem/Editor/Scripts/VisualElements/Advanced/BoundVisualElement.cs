using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace Scripts.VisualElements.Advanced
{
	public class BoundVisualElement : IDisposable
	{
		public const int delayTime = 300;
		
		protected readonly BoundVisualElementFactory buildVisualElement;
		
		public readonly List<VisualElement> BoundVisualElements = new List<VisualElement>();
		public readonly VisualElement PrimaryVisualElement;

		public Action onAnyUpdate;
		
		public BoundVisualElement(BoundVisualElementFactory buildVisualElement, VisualElement primaryVisualElement)
		{
			PrimaryVisualElement = primaryVisualElement;
			this.buildVisualElement = buildVisualElement;
			
			primaryVisualElement.RegisterCallback<DetachFromPanelEvent>(t => DestroyEverything());
		}

		public void DestroyEverything()
		{
			foreach (VisualElement visualElement in BoundVisualElements)
			{
				visualElement?.RemoveFromHierarchy();
			}
		}

		public VisualElement GetNewVisualElement()
		{
			VisualElement newVisualElement = new();
			newVisualElement.Add(buildVisualElement.Invoke(RefreshAllElements));

			newVisualElement.RegisterCallback<ChangeEvent<string>>(OnChangeEvent);
			newVisualElement.RegisterCallback<ChangeEvent<float>>(OnChangeEvent);
			newVisualElement.RegisterCallback<ChangeEvent<bool>>(OnChangeEvent);
			newVisualElement.RegisterCallback<ChangeEvent<int>>(OnChangeEvent);
			newVisualElement.RegisterCallback<ChangeEvent<Color>>(OnChangeEvent);

			BoundVisualElements.Add(newVisualElement);

			return newVisualElement;

			void OnChangeEvent<T>(ChangeEvent<T> evt) => Callback(evt, newVisualElement);
		}

		public virtual void RefreshAllElements(VisualElement inspectorVisualElement)
		{
			List<VisualElement> temp = new();
			foreach (VisualElement visualElement in BoundVisualElements.Where(visualElement => visualElement.parent != null))
			{
				temp.Add(visualElement);
				if (inspectorVisualElement == visualElement)
				{
					continue;
				}
				visualElement.Clear();
				visualElement.Add(buildVisualElement.Invoke(RefreshAllElements));
			}
			BoundVisualElements.Clear();
			BoundVisualElements.AddRange(temp);
			onAnyUpdate.Invoke();
		}
		
		protected void Callback<T>(ChangeEvent<T> t, VisualElement inspectorVisualElement)
		{
			if (t.newValue.GetHashCode() == t.previousValue.GetHashCode())
			{
				return;
			}
			RefreshVisuals(inspectorVisualElement);
		}
		
		protected IVisualElementScheduledItem DelayRefreshScheduledItem;

		protected void RefreshVisuals(VisualElement inspectorVisualElement)
		{
			DelayRefreshScheduledItem = PrimaryVisualElement.schedule.Execute(() =>
			{
				RefreshAllElements(inspectorVisualElement);
			});
			DelayRefreshScheduledItem.ExecuteLater(delayTime);
		}
		
		public void Dispose()
		{
			
		}
	}

	public delegate VisualElement BoundVisualElementFactory(Action<VisualElement> refreshAction);
}