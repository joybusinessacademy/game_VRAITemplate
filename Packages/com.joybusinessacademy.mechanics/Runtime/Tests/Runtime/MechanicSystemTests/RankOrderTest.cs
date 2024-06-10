using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class RankOrderTest
{
    MechanicsManager mechanicsManager;
    //AddressableServerDataScriptable addressableServerDataScriptable;

	RankOrderSystemData rankOrderTestData;
    SpawnerRankOrder spawnerRankOrder;

    private void GenerateSceneData()
    {
        GameObject go = new GameObject();
        go.AddComponent<MechanicsManager>();
        mechanicsManager = go.GetComponent<MechanicsManager>();
        //mechanicsManager.addressableServerDataScriptable = addressableServerDataScriptable;
    }

    private void GenerateRankOrderTestData()
    {
        //rankOrderTestData = ScriptableObject.CreateInstance<RankOrderQuestionScriptable>();

        RankOrderItemData rankOrderItemData = new RankOrderItemData();
        RankOrderSlotData rankOrderSlotData = new RankOrderSlotData();

        rankOrderTestData.rankOrderItems.Add(rankOrderItemData);
        rankOrderTestData.rankOrderSlots.Add(rankOrderSlotData);
    }

    // A Test behaves as an ordinary method
    [Test]
    public void RankOrderTestSimplePasses()
    {

    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator RankOrderSpawningTest()
    {
        // Use the Assert class to test conditions
        GenerateRankOrderTestData();
        GenerateSceneData();

        GameObject go = new GameObject();

        yield return null;

        go.AddComponent<SpawnerRankOrder>();
        spawnerRankOrder = go.GetComponent<SpawnerRankOrder>();
        spawnerRankOrder.mechanicData = rankOrderTestData;

        spawnerRankOrder.spawnOnAwake = true;
        spawnerRankOrder.startMechanicWhenReady = true;

        if (spawnerRankOrder != null)
        {
            yield return new WaitForSeconds(1);
            RankOrderSystem rankOrderSystem = spawnerRankOrder.GetComponentInChildren<RankOrderSystem>();

            if (rankOrderSystem != null)
            {
                Assert.Pass("Rank Order Spawned");
            }
            else
            {
                Assert.Fail("Rank Order System not spawned");
            }

        }
        else
        {
            Assert.Fail("Spawner Failed to Spawn");
        }

    }
}
