using SkillsVR.TimelineTool.Bindings;
using System;
using UnityEngine.Playables;

namespace SkillsVR.TimelineTool.AnimatorTimeline
{
    [Serializable]
    public class PlayableDirectorProvider : AbstractComponentProviderGetComponent<PlayableDirector>
    {
        public override object Clone()
        {
            var clone = new PlayableDirectorProvider();
            clone.gameObjectProvider = null == this.gameObjectProvider ? null : this.gameObjectProvider.Clone() as IGameObjectProvider;
            clone.enableCache = this.enableCache;
            return clone;
        }
    }
}