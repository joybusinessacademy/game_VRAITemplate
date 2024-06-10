namespace Props.PropInterfaces
{
	public interface IBaseProp
	{
		public PropType GetPropType();
		public PropComponent GetPropComponent();
		public void AutoConfigProp();
		public void SetPropComponent(PropComponent component);
	}
}