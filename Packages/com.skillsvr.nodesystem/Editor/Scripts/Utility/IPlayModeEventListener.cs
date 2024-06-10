using System;

namespace SkillsVRNodes.Managers.Utility
{
    public interface IPlayModeEventListener
	{
        event Action OnEnterPlayMode;
        event Action OnExitPlayMode;
        event Action OnEnterEditMode;
        event Action OnExitEditMode;
        event Action<bool> OnPlayMode;
        event Action<bool> OnEditMode;

        void Clear();
    }
}
