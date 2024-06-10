using System;

namespace SkillsVR.Login.Data
{
	public class LicenseDataHolder
	{
		public int code { get; set; }
		public string message { get; set; }
		public LicenseData data { get; set; }
	}

	public class LicenseData
	{
		public string OrgAdminEmail { get; set; }
		public string OrgAdminFirstName { get; set; }
		public string OrgAdminLastName { get; set; }
		public DateTime ExpiryDate { get; set; }
		public string Status { get; set; }
		public bool HasCck { get; set; }
		public bool HasPermission { get; set; }
	}
}