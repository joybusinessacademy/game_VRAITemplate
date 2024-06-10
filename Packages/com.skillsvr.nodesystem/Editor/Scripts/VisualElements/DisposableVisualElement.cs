using UnityEngine.UIElements;

namespace VisualElements
{
	public abstract class DisposableVisualElement : VisualElement, System.IDisposable
	{
		public abstract void Dispose();
	}
}