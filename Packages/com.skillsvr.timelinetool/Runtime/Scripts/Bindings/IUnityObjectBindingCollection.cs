namespace SkillsVR.TimelineTool.Bindings
{
    public interface IUnityObjectBindingCollection
    {
        UnityEngine.Object GetBindingValue(string id, object caller);
    }
}