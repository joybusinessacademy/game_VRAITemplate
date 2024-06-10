namespace SkillsVRNodes.Diagnostics
{
    public interface ICCKLogProcessor
    {
        IFilter<CCKLogEntry> LogFilter { get; set; }
        void OnReceiveLog(CCKLogEntry log);
    }
}

