using UnityEngine;

namespace JBA.CharacterFramework.Tracker
{
    public class CharacterFrameworkNodeTracker : MonoBehaviour
    {
        public CharacterFrameworkNodeTypeEnum nodeType = CharacterFrameworkNodeTypeEnum.None;


        public string id 
        { 
            get 
            { 
                if (string.IsNullOrEmpty(idCache)) 
                { 
                    idCache = nodeType.ToString();
                } 
                return idCache;
            }
        }

        private string idCache;
    }
}