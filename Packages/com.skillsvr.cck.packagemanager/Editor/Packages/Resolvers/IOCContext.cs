namespace SkillsVR.CCK.PackageManager.Ioc
{
    public class IOCContext
    {
        private static IBinder initBinder;
        public static IBinder Default
        {
            get
            {
                return initBinder;
            }
            set
            {
                if (null != initBinder)
                {
                    throw new System.Exception("IOC Content Binder already set to " + initBinder.GetType().Name);
                }
                initBinder = value;
            }
        }
    }
}