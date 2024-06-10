using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scripts.VisualElements.Advanced
{
	public class DuelBoundVisualElement : BoundVisualElement
	{
		protected readonly BoundVisualElementFactory buildSecondaryVisualElement;
		public readonly List<VisualElement> secondaryBoundVisualElements = new();

		
		public DuelBoundVisualElement(BoundVisualElementFactory buildVisualElement,
			BoundVisualElementFactory secondaryVisualElement, VisualElement primaryVisualElement) : 
			base(buildVisualElement, primaryVisualElement)
		{
			buildSecondaryVisualElement = secondaryVisualElement;
		}


		public override void RefreshAllElements(VisualElement inspectorVisualElement)
		{
			base.RefreshAllElements(inspectorVisualElement);
			List<VisualElement> temp = new();
			foreach (var visualElement in secondaryBoundVisualElements)
			{
				if (visualElement.parent == null)
				{
					continue;
				}
				
				temp.Add(visualElement);
				if (inspectorVisualElement == visualElement)
				{
					continue;
				}
				visualElement.Clear();
				visualElement.Add(buildSecondaryVisualElement.Invoke(RefreshAllElements));
			}
			secondaryBoundVisualElements.Clear();
			secondaryBoundVisualElements.AddRange(temp);
		}

		[CanBeNull]
		public VisualElement GetSecondaryVisualElement()
		{
			VisualElement newVisualElement = new();
			VisualElement child = buildSecondaryVisualElement.Invoke(RefreshAllElements);
			if (child == null)
			{
				return null;
			}
			newVisualElement.Add(child);
			
			newVisualElement.RegisterCallback<ChangeEvent<string>>(OnChangeEvent);
			newVisualElement.RegisterCallback<ChangeEvent<float>>(OnChangeEvent);
			newVisualElement.RegisterCallback<ChangeEvent<bool>>(OnChangeEvent);
			newVisualElement.RegisterCallback<ChangeEvent<int>>(OnChangeEvent);
			newVisualElement.RegisterCallback<ChangeEvent<Color>>(OnChangeEvent);
			newVisualElement.RegisterCallback<ChangeEvent<Object>>(OnChangeEvent);

			secondaryBoundVisualElements.Add(newVisualElement);

			return newVisualElement;

			void OnChangeEvent<T>(ChangeEvent<T> evt) => Callback(evt, newVisualElement);
		}
	}
}