using UnityEngine;

namespace SkillsVR.TimelineTool
{
	public static class GameObjectExtensions
    {
        public static string GetGameObjectPath(this GameObject gameObject)
        {
            if (null == gameObject)
            {
                return "<null>";
            }
            string path = gameObject.name;
            Transform parentTransform = gameObject.transform.parent;
            while(null != parentTransform)
            {
                path = parentTransform.gameObject.name + "/" + path;
                parentTransform = parentTransform.parent;
            }

            return path;
        }
    }
}