using UnityEngine;
using UnityEngine.Playables;

namespace SkillsVR.TimelineTool
{
	public static class PlayableExtensions
    {
        public static Component GetResolverComponent(this Playable playable) 
        {
            var graph = playable.GetGraph();
            if (!graph.IsValid())
            {
                return null;
            }

            var resolver = playable.GetGraph().GetResolver();
            if (null == resolver)
            {
                return null;
            }

            Component resolverComponent = resolver as Component;
            if (null == resolverComponent)
            {
                return null;
            }

            return resolverComponent;
        }

        public static T GetComponentInResolver<T>(this Playable playable) where T : Component
        {
            var resolver = playable.GetResolverComponent();
            if (null == resolver)
            {
                return null;
            }
            return resolver.GetComponent<T>();
        }
        public static T GetComponentInResolverChildren<T>(this Playable playable) where T : Component
        {
            var resolver = playable.GetResolverComponent();
            if (null == resolver)
            {
                return null;
            }
            return resolver.GetComponentInChildren<T>();
        }
    }
}