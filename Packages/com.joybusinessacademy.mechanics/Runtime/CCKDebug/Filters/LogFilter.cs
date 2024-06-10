using System;
using System.Collections.Generic;

namespace SkillsVRNodes.Diagnostics
{
    public class LogFilter:  IFilter<CCKLogEntry>
    {
        protected List<IFilterPredicate<CCKLogEntry>> BlockedPredicates { get; } = new List<IFilterPredicate<CCKLogEntry>>();

        public IEnumerable<IFilterPredicate<CCKLogEntry>> GetAllPredicates()
        {
            return BlockedPredicates;
        }
        public IFilterPredicate<CCKLogEntry> AddBlockPredicate(IFilterPredicate<CCKLogEntry> predicate)
        {
            if (null != predicate)
            {
                BlockedPredicates.Add(predicate);
            }
            return predicate;
        }

        public bool RemoveBlockPredicate(IFilterPredicate<CCKLogEntry> predicate)
        {
            if (null == predicate)
            {
                return false;
            }
            return BlockedPredicates.Remove(predicate);
        }

        public bool IsBlocked(CCKLogEntry log)
        {
            if (null == log)
            {
                return true;
            }
            foreach(var step in BlockedPredicates)
            {
                if (null == step)
                {
                    continue;
                }
                try
                {
                    bool blocked = step.IsBlocked(log);
                    if (blocked)
                    {
                        return true;
                    }
                }
                catch
                {
                    continue;
                }
            }
            return false;
        }

        
    }
}

