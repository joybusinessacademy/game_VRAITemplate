using UnityEngine;

namespace SkillsVR
{
	public static class GameObjectExtension
    {
        public static string GetPathString(this GameObject gameObject)
        {
            string path = gameObject.name;
            Transform currTrans = gameObject.transform.parent;
            while (null != currTrans)
            {
                path = currTrans.gameObject.name + "." + path;
                currTrans = currTrans.parent;
            }
            path = (null == gameObject.scene || string.IsNullOrWhiteSpace(gameObject.scene.name)
                ? "Untitled." : gameObject.scene.name + ".") + path;
            return path;
        }
    }
}
