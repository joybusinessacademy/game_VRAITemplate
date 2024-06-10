namespace SkillsVR.Mechanic.Core
{

    [RegisterMechanicEventParameterType(OnActiveStateChanged, typeof(bool))]
    [RegisterMechanicEventParameterType(OnError, typeof(string))]
    [RegisterMechanicEventParameterType(OnException, typeof(System.Exception))]
    public enum MechSysEvent
    {
        None,
        BeforeStart,
        OnStart,
        BeforeStop,
        OnStop,
        OnActiveStateChanged,
        OnReset,
        OnSetData,
        OnError,
        OnException,
        AfterFullStop,
    }

    [RegisterMechanicEventParameterType(StartLoadAsset, typeof(string))]
    [RegisterMechanicEventParameterType(FinishLoadAsset, typeof(IMechanicSystemResult))]
    [RegisterMechanicEventParameterType(StartLoadMechanic, typeof(string))]
    [RegisterMechanicEventParameterType(FiniahLoadMechanic, typeof(IMechanicSystemResult))]
    [RegisterMechanicEventParameterType(Cancelled, typeof(IMechanicSystemResult))]
    [RegisterMechanicEventParameterType(Error, typeof(IMechanicSystemResult))]
    public enum MechSysSpawnStateEvent
    {
        Idle,
        StartLoadAsset,
        FinishLoadAsset,
        StartLoadMechanic,
        FiniahLoadMechanic,
        Cancelled,
        Error,
        Ready,
        NoResult
    }

	[RegisterMechanicEventParameterType(OnGameObjectActiveChanged, typeof(bool))]
	public enum MechSysMonoEvents
    {
        None,
        OnAwake,
        OnDestroy,
        OnStart,
        OnEnable,
        OnDisable,
        OnUpdate,
        OnLateUpdate,
        OnGameObjectActiveChanged,
    }

    [RegisterMechanicEventParameterType(OnVisualStateChanged, typeof(bool))]
    public enum MechSysVisualEvents
    {
        NoChange,
        BeforeShow,
        OnShow,
        BeforeHide,
        OnHide,
        OnVisualStateChanged,
    }

    public interface IMechanicSystemEvent : IEvent<IMechanicSystem>
    {
    }
}
