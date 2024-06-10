using System.Collections;

namespace SkillsVR.CCK.PackageManager.AsyncOperation
{
    public class CustomRoutineOperation<T> : CustomAsyncOperation<T>
    {
        private IEnumerator CustomRoutine { get; set; }
        public CustomRoutineOperation(IEnumerator routine) :base()
        {
            CustomRoutine = routine;
            MoveNext();
        }

        protected override IEnumerator OnProcessRoutine()
        {
            if (null == CustomRoutine)
            {
                SetError("Custom routine cannot be null.");
                yield break;
            }
            yield return CustomRoutine;
        }

    }
}