using SkillsVR.CCK.PackageManager.Data;
using System.Collections.Generic;

namespace SkillsVR.CCK.PackageManager.AsyncOperations
{
    public class CCKAssetQueryResults
    {
        public int TotalResultsCount { get; protected set; }
        public IEnumerable<CCKAssetInfo> Assets { get; protected set; }
        public CCKAssetQueryArgs QueryArgs { get; protected set; }

        public CCKAssetQueryResults(IEnumerable<CCKAssetInfo> resultAssets, int total, CCKAssetQueryArgs query)
        {
            Assets = resultAssets ?? new List<CCKAssetInfo>(0);
            TotalResultsCount = total;
            QueryArgs = query;
        }
    }
}