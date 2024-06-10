
using System.Collections.Generic;

namespace SkillsVRNodes.Diagnostics
{
    public interface IFilterPredicate<TARGET_TYPE>
    {
        bool IsBlocked(TARGET_TYPE log);
    }

    public interface IFilter<TARGET_TYPE> : IFilterPredicate<TARGET_TYPE>
    {
        IFilterPredicate<TARGET_TYPE> AddBlockPredicate(IFilterPredicate<TARGET_TYPE> predicate);
        bool RemoveBlockPredicate(IFilterPredicate<TARGET_TYPE> predicate);

        IEnumerable<IFilterPredicate<TARGET_TYPE>> GetAllPredicates();
    }
}

