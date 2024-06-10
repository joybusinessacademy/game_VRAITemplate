using SkillsVRNodes.Diagnostics.LogFilters;
using System;
using System.Linq;
using UnityEngine;

namespace SkillsVRNodes.Diagnostics
{
    public static class LogFilterBuilderStepExtensions
    {
        public static IFilter<CCKLogEntry> BlockNoErrors(this IFilter<CCKLogEntry> filter)
        {
            return filter.BlockLogTypes(LogType.Log, LogType.Warning);
        }

        public static IFilter<CCKLogEntry> BlockErrors(this IFilter<CCKLogEntry> filter)
        {
            return filter.BlockLogTypes(LogType.Error, LogType.Exception, LogType.Assert);
        }

        public static IFilter<CCKLogEntry> ErrorsOnly(this IFilter<CCKLogEntry> filter)
        {
            return filter.BlockNoErrors();
        }

        public static IFilter<CCKLogEntry> MatchAnyLogTypes(this IFilter<CCKLogEntry> filter, params LogType[] types)
        {
            filter?.AddBlockPredicate(x => !types.Contains(x.logType));
            return filter;
        }

        public static IFilter<CCKLogEntry> BlockLogTypes(this IFilter<CCKLogEntry> filter, params LogType[] logTypes)
        {
            filter?.AddBlockPredicate(x => logTypes.Contains(x.logType));
            return filter;
        }

        public static IFilter<CCKLogEntry> BlockTags(this IFilter<CCKLogEntry> filter, params string[] tags)
        {
            filter?.AddBlockPredicate(x => null != x.tags && x.tags.Any(t => tags.Contains(t)));
            return filter;
        }

        public static IFilter<CCKLogEntry> MatchAnyTags(this IFilter<CCKLogEntry> filter, params string[] tags)
        {
            filter?.AddBlockPredicate(x => null == x.tags
                || !x.tags.Any(t => tags.Contains(t)));
            return filter;
        }

        public static IFilter<CCKLogEntry> MatchAllTags(this IFilter<CCKLogEntry> filter, params string[] tags)
        {
            filter?.AddBlockPredicate(x => null != x.tags
                && tags.All(t => x.tags.Contains(t)));
            return filter;
        }

        public static IFilter<CCKLogEntry> BlockDuplicateInInterval(this IFilter<CCKLogEntry> filter, float interval)
        {
            filter?.AddBlockPredicate(new BlockDuplicateInInterval(interval));
            return filter;
        }

        public static IFilter<CCKLogEntry> BlockDuplicate(this IFilter<CCKLogEntry> filter)
        {
            filter?.AddBlockPredicate(new BlockDuplicateInInterval(1.0f));
            return filter;
        }

        static IFilter<CCKLogEntry> AddBlockPredicate(this IFilter<CCKLogEntry> filter, Predicate<CCKLogEntry> predicate)
        {
            filter?.AddBlockPredicate(new BlockWithPredicate(predicate));
            return filter;
        }
    }
}

