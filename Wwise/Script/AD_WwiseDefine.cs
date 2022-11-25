using System.Collections.Generic;
using UnityEngine.AddressableAssets;

namespace AK
{
    public class WwiseDefine
    {
        public class WwiseObject
        {
            public string Name;
            public string Guid;
            public uint Bank;

            public WwiseObject(string Name, string Guid, uint Bank)
            {
                this.Name = Name;
                this.Guid = Guid;
                this.Bank = Bank;
            }
        }

        public static Dictionary<uint, WwiseObject> dataBanks = new Dictionary<uint, WwiseObject>();
        public static Dictionary<string, uint> dataRevBanks = new Dictionary<string, uint>();
        
        public static Dictionary<uint, WwiseObject> dataEvents = new Dictionary<uint, WwiseObject>();
        public static Dictionary<string, uint> dataRevEvents = new Dictionary<string, uint>();

        public static Dictionary<uint, WwiseObject> dataDialogueEvents = new Dictionary<uint, WwiseObject>();
        public static Dictionary<string, uint> dataRevDialogueEvents = new Dictionary<string, uint>();
        
        public static Dictionary<uint, WwiseObject> dataGameParameters = new Dictionary<uint, WwiseObject>();
        public static Dictionary<string, uint> dataRevGameParameters = new Dictionary<string, uint>();

        // Load from Json
        public static void LoadDefine()
        {
            /*
            var jsonText = Addressables.LoadAssetAsync<UnityEngine.TextAsset>(
                AD_WwiseManager.GetWwiseIdJsonPath()
            ).WaitForCompletion();
            */

            UnityEngine.TextAsset jsonText = UT_ResourceLoadManager.Instance.LoadUObject(
                AD_WwiseManager.GetWwiseIdJsonPath()
            ) as UnityEngine.TextAsset;

            LitJson.JsonData jsonData = LitJson.JsonMapper.ToObject(jsonText.text);

            System.Action<string, Dictionary<uint, WwiseObject>, Dictionary<string, uint>> Scan = (Type, Dict, revDict) =>
            {
                // Run in Editor
                Dict.Clear();
                revDict.Clear();

                if (jsonData.ContainsKey(Type))
                {
                    LitJson.JsonData typeData = jsonData[Type];

                    for (int i = 0; i < typeData.Count; i += 1)
                    {
                        LitJson.JsonData Data = typeData[i];

                        uint Id = (uint)Data["Id"];
                        string Name = (string)Data["Name"];
                        string Guid = (string)Data["Guid"];
                        uint Bank = (uint)Data["Bank"];

                        Dict.Add(Id, new WwiseObject(Name, Guid, Bank));
                        revDict.Add(Name, Id);
                    }
                }
            };

            Scan("Banks", dataBanks, dataRevBanks);
            Scan("Events", dataEvents, dataRevEvents);
            Scan("DialogueEvents", dataDialogueEvents, dataRevDialogueEvents);
            Scan("GameParameters", dataGameParameters, dataRevGameParameters);

            UT_ResourceLoadManager.Instance.UnLoadUOject(jsonText, false);
        }
    }
}
