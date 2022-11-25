#if UNITY_EDITOR
internal static class MarkWwiseAddressable
{
	// Addressable Tools
	// [UnityEditor.MenuItem("Wwise/Mark Wwise Assets Addressable", false, (int)1)]
	public static void DoMarkWwiseAddressable()
	{
		UnityEngine.Debug.Log("Wwise Assets Updated in Addressable Group");

		var Setting = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
		var Group = Setting.FindGroup("Audio");

		var EntryAdded = new System.Collections.Generic.List<UnityEditor.AddressableAssets.Settings.AddressableAssetEntry>();

		System.Action<string> AddToAa = Guid =>
		{
			var Entry = Setting.CreateOrMoveEntry(Guid, Group, readOnly: false, postEvent: false);
			Entry.address = UnityEditor.AssetDatabase.GUIDToAssetPath(Guid);

			EntryAdded.Add(Entry);
		};

		// Init Setting
		AddToAa(UnityEditor.AssetDatabase.AssetPathToGUID(AD_WwiseManager.GetAddressableInitializationSettingsPath()));

		// Wwise Id Json
		AddToAa(UnityEditor.AssetDatabase.AssetPathToGUID(AD_WwiseManager.GetWwiseIdJsonPath()));

		// Bank Refs
		var Guids = UnityEditor.AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/ArtRes/Bundle/Audio/Wwise/Banks" });

		for (int i = 0; i < Guids.Length; i += 1)
		{
			var Guid = Guids[i];
			var Path = UnityEditor.AssetDatabase.GUIDToAssetPath(Guid);

			// Bank Ref
			if (Path.EndsWith(".asset")) AddToAa(Guid);
		}

		Setting.SetDirty(UnityEditor.AddressableAssets.Settings.AddressableAssetSettings.ModificationEvent.EntryMoved, EntryAdded, true);

		// Run in Client
		AddBanksToAaGroup();
	}

	private static void AddBanksToAaGroup()
    {
		UnityEngine.Debug.Log("Wwise Updateing Banks Aa Group");

		#region Wwise Info: art_editor -> client
		// HARD CODE
		string bankJsonPath = System.IO.Path.Combine(
			UnityEngine.Application.dataPath, "ArtRes/Bundle/Audio/Wwise/Banks/Windows/SoundbanksInfo.json"
		);

		string bankXmlPath = System.IO.Path.Combine(
			UnityEngine.Application.dataPath, "ArtRes/Bundle/Audio/Wwise/Banks/Windows/SoundbanksInfo.xml"
		);

		string jsonText = System.IO.File.ReadAllText(bankJsonPath);
		string xmlText = System.IO.File.ReadAllText(bankXmlPath);

		// Not Safe
		jsonText = jsonText.Replace("art_editor", "client");
		xmlText = xmlText.Replace("art_editor", "client");

		System.IO.File.WriteAllText(bankJsonPath, jsonText);
		System.IO.File.WriteAllText(bankXmlPath, xmlText);
        #endregion

        // Not Safe
        var Setting = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;

		var EntryAdded = new System.Collections.Generic.List<UnityEditor.AddressableAssets.Settings.AddressableAssetEntry>();

		System.Action<string, string> AddToAaGroup = (Guid, groupName) =>
		{
			var Group = Setting.FindGroup(groupName);

			var Entry = Setting.CreateOrMoveEntry(Guid, Group, readOnly: false, postEvent: false);
			Entry.address = UnityEditor.AssetDatabase.GUIDToAssetPath(Guid);

			EntryAdded.Add(Entry);
		};

		var Guids = UnityEditor.AssetDatabase.FindAssets("", new[] { "Assets/ArtRes/Bundle/Audio/Wwise/Banks" });

		for (int i = 0; i < Guids.Length; i += 1)
		{
			var Guid = Guids[i];
			var Path = UnityEditor.AssetDatabase.GUIDToAssetPath(Guid);

			// Reimport Banks
			UnityEditor.AssetDatabase.ImportAsset(Path);

			// Bank Data
			if (Path.EndsWith(".bnk"))
			{
				if (Path.Contains("Windows"))
				{
					// 
					if (Path.Contains("Init.bnk"))
					{
						AddToAaGroup(Guid, "WwiseData_Windows_InitBank");
					}
					else
					{
						AddToAaGroup(Guid, "WwiseData_Windows");
					}
				}
			}
		}

		Setting.SetDirty(UnityEditor.AddressableAssets.Settings.AddressableAssetSettings.ModificationEvent.EntryMoved, EntryAdded, true);
	}
}
#endif // #if UNITY_EDITOR