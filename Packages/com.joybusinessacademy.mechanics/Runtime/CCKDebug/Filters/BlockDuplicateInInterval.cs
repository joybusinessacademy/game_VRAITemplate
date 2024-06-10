using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkillsVRNodes.Diagnostics.LogFilters
{
    public class BlockDuplicateInInterval : IFilterPredicate<CCKLogEntry>
    {
        public float intervalInSec { get; set; } = 1.0f;


        protected Dictionary<int, float> managedLogTime = new Dictionary<int, float>();

        public BlockDuplicateInInterval(float minDuplicateInternal = 1.0f)
        {
            intervalInSec = minDuplicateInternal;
        }

        public bool IsBlocked(CCKLogEntry log)
        {
            ClearExpiredData();
            int id = log.GetConentHashCode();
            if (0 == id)
            {
                return true;
            }

            float recordTime = 0.0f;
            float currTime = Time.realtimeSinceStartup;
            if (managedLogTime.TryGetValue(id, out recordTime))
            {
                // existing 
                managedLogTime[id] = currTime;
                float timeDiff = (currTime - recordTime);
                return timeDiff < intervalInSec;
            }
            else
            {
                // new item
                managedLogTime.Add(id, currTime);
                return false;
            }
        }

        protected void ClearExpiredData()
        {
            float currTime = Time.realtimeSinceStartup;
            var itemsToDelete = managedLogTime.Where(x => intervalInSec < (currTime - x.Value)).ToList();
            foreach(var item in itemsToDelete)
            {
                managedLogTime.Remove(item.Key);
            }
        }
    }
}

