using SkillsVR.UnityExtenstion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SkillsVRNodes;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PlayerDistributer : MonoBehaviour
{

    [SerializeField]
    private GameObject overridePlayer;
    private const string defaultPrefabName = "VRPlayerVisualScriptingSystem";

    private static GameObject playerPrefab;
    private static Dictionary<string, GameObject> createdPlayers = new();
    private static List<PlayerSpawnPosition> currentSpawnPositions = new();

    public static GameObject LocalPlayer => TryGetPlayer(SystemInfo.deviceUniqueIdentifier);

    private void Awake()
    {
        playerPrefab = overridePlayer ? overridePlayer : Resources.Load(defaultPrefabName) as GameObject;
    }

    public static bool ExistPlayer(string uid)
    {
        return createdPlayers.ContainsKey(uid);
    }

    public static GameObject GetPlayer(string uid)
    {
        return TryGetPlayer(uid);
    }
    
    private static GameObject TryGetPlayer(string uid)
    {
        if (createdPlayers.TryGetValue(uid, out GameObject player))
        {
            return player;
        }

        return SpawnPlayer();       
    }

    private static GameObject SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            playerPrefab = Resources.Load(defaultPrefabName) as GameObject;
        }

        GameObject playerObject = Instantiate(playerPrefab);

        playerObject.AddComponent<PlayerIdentifier>().Set(SystemInfo.deviceUniqueIdentifier);
        createdPlayers.Add(SystemInfo.deviceUniqueIdentifier, playerObject);
        return playerObject;
    }

    private static int ProduceNewId()
    {
        return createdPlayers.Count;
    }

    public static void OnPlayerDestroyed(string uid)
    {
        if (createdPlayers.ContainsKey(uid) == false)
        {
            return;
        }

        createdPlayers.Remove(uid);
    }
    
    public static PlayerSpawnPosition TryGetSpawnPosition(string uid)
    {
        return currentSpawnPositions.FirstOrDefault(sp => sp.Uid == uid);
    }
    
    public static void RegisterSpawnPosition(PlayerSpawnPosition spawnPosition)
    {
        if (spawnPosition != null && currentSpawnPositions.Contains(spawnPosition) == false)
        {
            currentSpawnPositions.Add(spawnPosition);
        }
    }
    
    public static void UnregisterSpawnPosition(PlayerSpawnPosition spawnPosition)
    {
        if (spawnPosition != null && currentSpawnPositions.Contains(spawnPosition))
        {
            currentSpawnPositions.Remove(spawnPosition);
        }
    }
}
