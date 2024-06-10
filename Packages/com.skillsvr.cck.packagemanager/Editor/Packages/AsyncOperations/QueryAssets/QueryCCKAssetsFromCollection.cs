using SkillsVR.CCK.PackageManager.AsyncOperation;
using SkillsVR.CCK.PackageManager.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkillsVR.CCK.PackageManager.AsyncOperations
{
    public class QueryCCKAssetsFromCollection : CustomAsyncOperation<CCKAssetQueryResults>
    {
        public CCKAssetQueryArgs Query { get; protected set; }

        protected IEnumerable<CCKAssetInfo> inputSource;

        public QueryCCKAssetsFromCollection(CCKAssetQueryArgs rawQuery, IEnumerable<CCKAssetInfo> src)
        {
            Query = rawQuery;
            inputSource = src;
            Result = null;
            MoveNext();
        }
        protected override IEnumerator OnProcessRoutine()
        {
            if (null == inputSource)
            {
                SetError("Nothing to query. The collection cannot be null.");
                yield break;
            }
            var r = inputSource;

            if (null != Query)
            {
                r = r.QueryAssets(Query.Categories, x => Query.Categories.Select(k => k.ToLower()).Any(k => !string.IsNullOrWhiteSpace(x.category) && x.category.ToLower().StartsWith(k)));
                r = r.QueryAssets(Query.Packages, x => Query.Packages.Select(k => k.ToLower()).Any(k => !string.IsNullOrWhiteSpace(x.packageName) && x.packageName.ToLower() == k));
                r = r.QueryAssets(Query.Types, x => Query.Types.Select(k => k.ToLower()).Any(k => !string.IsNullOrWhiteSpace(x.type) && x.type.ToLower() == k));
                r = r.QueryAssets(Query.KeyWords, x => Query.KeyWords.Select(k => k.ToLower()).Any(k => !string.IsNullOrWhiteSpace(x.displayName) && x.displayName.ToLower().Contains(k)));
            }

            int totalCount = r.Count();
            r = r.OrderBy(x => x.displayName);

            int from = null == Query ? 0 : Query.from;
            int size = null == Query ? 24 : Query.size;
            r = r.Skip(from).Take(size);
            Result = new CCKAssetQueryResults(r, totalCount, Query);
        }

        public override List<string> GetExtraInfoStrings()
        {
            var list = base.GetExtraInfoStrings();
            list.Add(Query.ToText("Query"));
            list.Add("Input: " + (null == inputSource ? "null" : inputSource.Count() + "items"));
            list.Add(Result.ToText("Result"));
            return list;
        }
    }

    static class IENUMEX
    {
        public static IEnumerable<CCKAssetInfo> QueryAssets(this IEnumerable<CCKAssetInfo> src, IEnumerable<string> keys, Predicate<CCKAssetInfo> predicate)
        {
            if (null == src)
            {
                return src;
            }
            if (null == keys)
            {
                return src;
            }
            keys = keys.Where(x => !string.IsNullOrWhiteSpace(x));
            if(0 == keys.Count())
            {
                return src;
            }
            if (null == predicate)
            {
                return src;
            }

            return src.Where(x=> predicate(x));
        }
    }
}