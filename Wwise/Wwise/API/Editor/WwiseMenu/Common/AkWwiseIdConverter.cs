#if UNITY_EDITOR
internal static class AkWwiseIDConverter
{
	private static readonly string s_bankDir = UnityEngine.Application.dataPath;

	private static readonly string s_converterScript = System.IO.Path.Combine(
		System.IO.Path.Combine(System.IO.Path.Combine(UnityEngine.Application.dataPath, "Wwise"), "Tools"),
		"WwiseIDConverter.py");

	private static readonly string s_progTitle = "WwiseUnity: Converting SoundBank IDs";

	// By Zhonghui, Dont Use Ids in C#
	// [UnityEditor.MenuItem("Wwise/Convert Wwise SoundBank IDs", false, (int) AkWwiseMenuOrder.ConvertIDs)]
	public static void ConvertWwiseSoundBankIDs()
	{
		/*
		var bankIdHeaderPath =
			UnityEditor.EditorUtility.OpenFilePanel("Choose Wwise SoundBank ID C++ header", s_bankDir, "h");
		if (string.IsNullOrEmpty(bankIdHeaderPath))
		{
			UnityEngine.Debug.Log("WwiseUnity: User canceled the action.");
			return;
		}
		*/

		// By Zhonghui
		string bankIdHeaderPath = System.IO.Path.Combine(UnityEngine.Application.dataPath,
			"StreamingAssets", "Audio", "GeneratedSoundBanks", "Wwise_IDs.h"
		);

		string pythonEnv = System.IO.File.ReadAllText(System.IO.Path.Combine(UnityEngine.Application.dataPath,
			"WwiseData", "Editor", "PyEnv.txt"
		));

		var start = new System.Diagnostics.ProcessStartInfo();
		start.FileName = System.IO.Path.Combine(pythonEnv, "python.exe");
		start.Arguments = string.Format("\"{0}\" \"{1}\" \"{2}\"", s_converterScript, bankIdHeaderPath,
			UnityEngine.Application.dataPath // By Zhonghui
		);
		start.UseShellExecute = false;
		start.RedirectStandardOutput = true;

		var progMsg = "WwiseUnity: Converting C++ SoundBank IDs into C# ...";
		UnityEditor.EditorUtility.DisplayProgressBar(s_progTitle, progMsg, 0.5f);

		using (var process = System.Diagnostics.Process.Start(start))
		{
			process.WaitForExit();
			try
			{
				//ExitCode throws InvalidOperationException if the process is hanging
				if (process.ExitCode == 0)
				{
					UnityEditor.EditorUtility.DisplayProgressBar(s_progTitle, progMsg, 1.0f);
					UnityEngine.Debug.Log(string.Format(
						"WwiseUnity: SoundBank ID conversion succeeded. Find generated Unity script under {0}.", s_bankDir));
				}
				else
					UnityEngine.Debug.LogError("WwiseUnity: Conversion failed.");

				UnityEditor.AssetDatabase.Refresh();
			}
			catch (System.Exception ex)
			{
				UnityEditor.AssetDatabase.Refresh();

				UnityEditor.EditorUtility.ClearProgressBar();
				UnityEngine.Debug.LogError(string.Format(
					"WwiseUnity: SoundBank ID conversion process failed with exception: {}. Check detailed logs under the folder: Assets/Wwise/Logs.",
					ex));
			}

			UnityEditor.EditorUtility.ClearProgressBar();
		}
	}
}
#endif // #if UNITY_EDITOR