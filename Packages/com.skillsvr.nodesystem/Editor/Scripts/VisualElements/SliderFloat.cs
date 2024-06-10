using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scripts.VisualElements
{
	public class SliderFloat : VisualElement
	{
		private float currentValue;
		private readonly Slider slider;
		private readonly FloatField floatField;
		private readonly Action<float> changeEvent = _ => { };
		
		public SliderFloat(float min, float max, float currentValue, string label, Action<float> changeEvent)
		{
			
			styleSheets.Add(Resources.Load<StyleSheet>("SliderFloat"));
			this.changeEvent += changeEvent;
			
			Label visualElement = new(label)
			{
				style =
				{
					width = new StyleLength(Length.Percent(50f)),
				}
			};
			Add(visualElement);
			
			
			slider = new Slider(min, max)
			{
				value = currentValue,
			};
			slider.RegisterValueChangedCallback(evt => SetValue(evt.newValue));
			Add(slider);

			floatField = new FloatField
			{
				value = currentValue
			};
			floatField.RegisterValueChangedCallback(evt => SetValue(evt.newValue));
			Add(floatField);
			
		}


		public void SetValue(float newValue)
		{
			newValue = Mathf.Clamp(newValue, slider.lowValue, slider.highValue);
			slider.SetValueWithoutNotify(newValue);
			floatField.SetValueWithoutNotify(newValue);
			changeEvent?.Invoke(newValue);
		}
	}
}