using Props;
using UnityEngine;

[RequireComponent(typeof(PropComponent))]
public class PropEventHandler : MonoBehaviour
{
	public PropComponent propComponent;
	public UnityEventProp eventProp;

	private void Awake()
	{
		if(propComponent == null)
			propComponent = GetComponent<PropComponent>();

		if(propComponent.propType.GetType() == typeof(UnityEventProp))
			eventProp = propComponent.propType as UnityEventProp;
	}

	public void Finish()
	{
		if(eventProp != null)
			eventProp.unityEvent.Invoke();
	}

	private void OnValidate()
	{
		if (propComponent == null)
			propComponent = GetComponent<PropComponent>();
	}
}
