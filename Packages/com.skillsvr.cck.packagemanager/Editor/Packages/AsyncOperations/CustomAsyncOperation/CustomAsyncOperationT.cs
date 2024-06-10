using System.Collections.Generic;

namespace SkillsVR.CCK.PackageManager.AsyncOperation
{
    public abstract class CustomAsyncOperation<T> : CustomAsyncOperation, ICustomAsyncOperation<T>
    {
        public virtual T Result { get; protected set; }

        public override List<string> GetExtraInfoStrings()
        {
            var list = base.GetExtraInfoStrings();
            list.Add(Result.ToText("Result"));
            return list;
        }
    }
}