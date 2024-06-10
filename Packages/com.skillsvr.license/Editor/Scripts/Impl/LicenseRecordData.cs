using System;

public enum LicenseType
{
	FREE,
	PAID,
	MISSING
}

namespace SkillsVRNodes.License
{
	[Serializable]
	public class LicenseRecordData
	{
		public LicenseType licenseType = LicenseType.PAID;
		public string licenseActivationDate = "";
		public string licenseExpirationDate = "";
	}
}
