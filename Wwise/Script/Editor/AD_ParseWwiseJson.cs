#if UNITY_EDITOR
internal static class ParseWwiseJson
{
	private static readonly string s_converterScript = System.IO.Path.Combine(
		UnityEngine.Application.dataPath,
		"Script", "CSharp", "Wwise", "Tool", "ParseWwiseJson.py"
	);

	// Don't Run in Client
#if Art_Editor
	[UnityEditor.MenuItem("Wwise/Parse Wwise Json", false, (int)0)]
#endif
	public static void DoParseWwiseJson()
	{
		// Deprecation
		/*
		string pythonEnv = System.IO.File.ReadAllText(System.IO.Path.Combine(UnityEngine.Application.dataPath,
			"WwiseData", "Editor", "PyEnv.txt"
		));
		*/

		var start = new System.Diagnostics.ProcessStartInfo();

		// Deprecation
		// start.FileName = System.IO.Path.Combine(pythonEnv, "python.exe");
		start.FileName = "python.exe";

		start.Arguments = string.Format("\"{0}\" \"{1}\"", s_converterScript, UnityEngine.Application.dataPath);
		start.UseShellExecute = false;
		start.RedirectStandardOutput = true;

		using (var process = System.Diagnostics.Process.Start(start))
		{
			process.WaitForExit();
			try
			{
				if (process.ExitCode == 0) UnityEngine.Debug.Log("Wwise: Json to C# Define Updated");
				else UnityEngine.Debug.LogError("Wwise: Run Python Failed, " + process.ExitCode.ToString());

				UnityEditor.AssetDatabase.Refresh();
			}
			catch (System.Exception ex)
			{
				UnityEditor.AssetDatabase.Refresh();

				UnityEngine.Debug.LogError(string.Format("Wwise: Run C# Failed, {}", ex));
			}
		}

		// All Wwise Assets
		MarkWwiseAddressable.DoMarkWwiseAddressable();
	}
}
#endif // #if UNITY_EDITOR