#if UNITY_EDITOR
internal static class MarkWwiseAddressable
{
    // Addressable Tools, Call from Other Place:
    // ArtEditor: Parse Wwise Json
    // 
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
        var Guids = UnityEditor.AssetDatabase.FindAssets("t:ScriptableObject", new[] { AD_WwiseManager.GetBanksPath() });

        for (int i = 0; i < Guids.Length; i += 1)
        {
            var Guid = Guids[i];
            var Path = UnityEditor.AssetDatabase.GUIDToAssetPath(Guid);

            // Bank Ref
            if (Path.EndsWith(".asset")) AddToAa(Guid);
        }

        Setting.SetDirty(UnityEditor.AddressableAssets.Settings.AddressableAssetSettings.ModificationEvent.EntryMoved, EntryAdded, true);

        AddBanksToAaGroup();

        CopyLuaIdTable(); // Client
    }

    private static void AddBanksToAaGroup(string Platform = "Windows")
    {
        UnityEngine.Debug.Log("Wwise Updateing Banks Aa Group");

#region Wwise Info
        string bankJsonPath = System.IO.Path.Combine(
            UnityEngine.Application.dataPath, "..", AD_WwiseManager.GetBanksPath(), Platform, "SoundbanksInfo.json"
        );

        string bankXmlPath = System.IO.Path.Combine(
            UnityEngine.Application.dataPath, "..", AD_WwiseManager.GetBanksPath(), Platform, "SoundbanksInfo.xml"
        );

        // string jsonText = System.IO.File.ReadAllText(bankJsonPath);
        // string xmlText = System.IO.File.ReadAllText(bankXmlPath);

        // Not Safe
        // jsonText = jsonText.Replace("art_editor", "client");
        // xmlText = xmlText.Replace("art_editor", "client");

        // System.IO.File.WriteAllText(bankJsonPath, jsonText);
        // System.IO.File.WriteAllText(bankXmlPath, xmlText);
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

        var Guids = UnityEditor.AssetDatabase.FindAssets("", new[] { AD_WwiseManager.GetBanksPath() });

        string[] Platforms = new string[] { "Windows", "Android" };

        for (int i = 0; i < Guids.Length; i += 1)
        {
            var Guid = Guids[i];
            var Path = UnityEditor.AssetDatabase.GUIDToAssetPath(Guid);

            // Slow
            // Reimport Banks
            // UnityEditor.AssetDatabase.ImportAsset(Path);

            // Bank Data And Steaming Data
            if (Path.EndsWith(".bnk") || Path.EndsWith(".wem"))
            {
                for (int j = 0; j < Platforms.Length; j += 1)
                {
                    if (Path.Contains(Platforms[j]))
                    {
                        if (Path.Contains("Init.bnk"))
                        {
                            AddToAaGroup(Guid, "WwiseData_" + Platforms[j] + "_InitBank");
                        }
                        else
                        {
                            AddToAaGroup(Guid, "WwiseData_" + Platforms[j]);
                        }

                        break;
                    }
                }
            }
        }

        Setting.SetDirty(UnityEditor.AddressableAssets.Settings.AddressableAssetSettings.ModificationEvent.EntryMoved, EntryAdded, true);
    }

    private static void CopyLuaIdTable()
    {
        string sourcePath = AD_WwiseManager.GetLuaIdTableSourceFilePath();
        string targetPath = AD_WwiseManager.GetLuaIdTableTargetFilePath();

        if (System.IO.File.Exists(sourcePath) && System.IO.File.Exists(targetPath))
        {
            string luaIdContent = System.IO.File.ReadAllText(sourcePath);
            System.IO.File.WriteAllText(targetPath, luaIdContent);
        }
    }
}
#endif // #if UNITY_EDITOR