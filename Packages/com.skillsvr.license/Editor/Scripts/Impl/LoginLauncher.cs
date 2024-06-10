using SkillsVR.Login;
using UnityEditor;

[InitializeOnLoad]
public static class LoginLauncher
{
	internal const string FIRST_TIME = "FIRST_TIME_LOGINLAUNCHER";

	static LoginLauncher()
	{
		EditorApplication.update += RunOnce;
	}

	private static void RunOnce()
	{
		var firstTime = SessionState.GetBool(FIRST_TIME, true);
		if (firstTime)
		{
			while (EditorApplication.isCompiling)
				return;

			SessionState.SetBool(FIRST_TIME, false);

			LoginManager.ShowWindow();

			EditorApplication.update -= RunOnce;

		}
	}

}
