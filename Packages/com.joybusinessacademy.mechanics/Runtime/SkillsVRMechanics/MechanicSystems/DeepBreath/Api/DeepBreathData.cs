namespace SkillsVR.Mechanic.MechanicSystems.DeepBreath
{
    [System.Serializable]
	public class DeepBreathData
    {
        // style 0
        public float duration = 6.0f;
        public bool autoHideAfterSuccess = true;
        public bool autoBrathOut = true;
        public float timeout = 0.0f;

        // use in breathe style 1
        public int style = 1;
        public float breathIn = 2.0f;
        public float breatheOut = 3.0f;
        public float style2Timeout = 20.0f;

    }
}
