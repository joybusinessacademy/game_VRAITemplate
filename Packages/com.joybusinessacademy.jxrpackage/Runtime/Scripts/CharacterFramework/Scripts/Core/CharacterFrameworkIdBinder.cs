using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JBA.CharacterFramework
{
    public static class CharacterFrameworkIdBinder
    {
        public static string selectionFrameId { get; set; }
        public const string localPlayerId = "@LocalPlayerId";
        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            CharacterFrameworkModule.SetCustomFrameIdCallback(GetFrameId);
        }

        private static string GetFrameId(string inputFrame)
        {
            if ("@LocalPlayerId" == inputFrame)
            {
                return localPlayerId;
            }
            if ("@SelectionFrameId" == inputFrame)
            {
                return selectionFrameId;
            }
            return inputFrame;
        }
    }
}