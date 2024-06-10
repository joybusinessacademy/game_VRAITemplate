using System;
using Props.PropInterfaces;
using UnityEngine;
using UnityEngine.Serialization;

namespace Props
{
	[Serializable]
	public abstract class PropType : IBaseProp
	{
		[FormerlySerializedAs("prop")] [SerializeField] protected PropComponent propComponent;


		public PropType(PropComponent propComponent)
		{
			this.propComponent = propComponent;
		}
		
		public virtual string name => "Base Prop";

		public PropType GetPropType() => this;
		public PropComponent GetPropComponent() => propComponent;

		public abstract void AutoConfigProp();

		public void SetPropComponent(PropComponent component)
		{
			this.propComponent = component;
		}
		
		public T AddComponentAsChildObject<T>(string childName) where T : Component
		{
			GameObject childObject = new GameObject(childName);
			T component = childObject.AddComponent<T>();
			childObject.transform.parent = propComponent.gameObject.transform;

			return component;
		}
	}
}