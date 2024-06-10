using UnityEngine;

namespace SkillsVRNodes.Managers.Utility
{
    public static class GameObjectDestroyExtensions
    {
        public static void Destroy(this GameObject gameObject)
        {
            if (null == gameObject)
            {
                return;
            }
            GameObject.Destroy(gameObject);
        }
    }
}
