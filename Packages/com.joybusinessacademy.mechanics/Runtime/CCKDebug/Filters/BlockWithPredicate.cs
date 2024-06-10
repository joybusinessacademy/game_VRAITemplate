using System;

namespace SkillsVRNodes.Diagnostics.LogFilters
{
    public class BlockWithPredicate : IFilterPredicate<CCKLogEntry>
    {
        protected Predicate<CCKLogEntry> BlockPredicate { get; set; }
        public BlockWithPredicate(Predicate<CCKLogEntry> predicate)
        {
            BlockPredicate = predicate;
        }

        public bool IsBlocked(CCKLogEntry log)
        {
            if (null == log)
            {
                return true;
            }
            if (null == BlockPredicate)
            {
                return false;
            }
            return BlockPredicate.Invoke(log);
        }
    }
}

