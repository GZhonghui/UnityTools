#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WwiseEventPicker : EditorWindow
{
    private string originName;
    private string eventName;
    private string eventBankName;

    private System.Action selectedCallback;

    private bool Selected = false;
    private bool Inited = false;

    private string Filter = "";

    public void InitWith(string eventName, System.Action selectedCallback)
    {
        this.maxSize = new Vector2(480, 320);
        this.minSize = this.maxSize;

        Selected = false;
        Inited = true;

        this.selectedCallback = selectedCallback;

        this.originName = eventName != null ? eventName : "";
        this.eventName = eventName != null ? eventName : "";

        eventBankName = null;
        if (AK.WwiseDefine.dataRevEvents.ContainsKey(this.eventName))
        {
            uint eventId = AK.WwiseDefine.dataRevEvents[this.eventName];
            uint bankId = AK.WwiseDefine.dataEvents[eventId].Bank;
            if (AK.WwiseDefine.dataBanks.ContainsKey(bankId))
            {
                eventBankName = AK.WwiseDefine.dataBanks[bankId].Name;
            }
        }
    }

    public void GetResult(out string stringData)
    {
        stringData = Selected ? this.eventName : this.originName;
    }

    private void OnGUI()
    {
        if (!Inited) return;

        HashSet<string> bankNames = new HashSet<string>();
        HashSet<string> eventNames = new HashSet<string>();
        List<string> bankNamesList = new List<string>();
        List<string> eventNamesList = new List<string>();

        // Filter
        GUI.color = Color.yellow;
        GUILayout.Label("Filter");
        GUI.color = Color.white;
        Filter = GUILayout.TextField(Filter);
        GUILayout.Space(8);

        // Add Events and its Bank
        foreach (var Item in AK.WwiseDefine.dataEvents)
        {
            if (Filter == "" || Item.Value.Name.Contains(Filter))
            {
                if (AK.WwiseDefine.dataBanks.ContainsKey(Item.Value.Bank))
                {
                    bankNames.Add(AK.WwiseDefine.dataBanks[Item.Value.Bank].Name);

                    if (eventBankName != null && AK.WwiseDefine.dataBanks[Item.Value.Bank].Name == eventBankName)
                    {
                        eventNames.Add(Item.Value.Name);
                    }
                }
            }
        }

        // Add Banks
        foreach (var Item in AK.WwiseDefine.dataBanks)
        {
            if (Filter == "" || Item.Value.Name.Contains(Filter))
            {
                bankNames.Add(Item.Value.Name);
            }
        }

        int selectedBankIndex = -1;
        int selectedEventIndex = -1;
        int indexCounter = 0;

        // Select Bank
        indexCounter = 0;
        foreach (var Item in bankNames)
        {
            bankNamesList.Add(Item);
            if (eventBankName != null && Item == eventBankName)
            {
                selectedBankIndex = indexCounter;
            }

            indexCounter += 1;
        }

        // Select Event
        indexCounter = 0;
        foreach (var Item in eventNames)
        {
            eventNamesList.Add(Item);
            if (eventName != null && Item == eventName)
            {
                selectedEventIndex = indexCounter;
            }

            indexCounter = indexCounter + 1;
        }

        // Category
        GUI.color = Color.yellow;
        GUILayout.Label("Select Category");
        GUI.color = Color.white;
        selectedBankIndex = EditorGUILayout.Popup(selectedBankIndex, bankNamesList.ToArray());
        if (selectedBankIndex > -1 && selectedBankIndex < bankNamesList.Count)
        {
            eventBankName = bankNamesList[selectedBankIndex];
        }
        GUILayout.Space(8);

        // Event
        GUI.color = Color.yellow;
        GUILayout.Label("Select Event");
        GUI.color = Color.white;
        selectedEventIndex = EditorGUILayout.Popup(selectedEventIndex, eventNamesList.ToArray());
        if (selectedEventIndex > -1 && selectedEventIndex < eventNamesList.Count)
        {
            eventName = eventNamesList[selectedEventIndex];
        }
        GUILayout.Space(8);

        // Info
        GUI.color = Color.yellow;
        GUILayout.Label("Detail");
        GUI.color = Color.white;
        GUILayout.Label("Event Name: " + eventName);
        string eventId = "";
        string eventGuid = "";
        if (AK.WwiseDefine.dataRevEvents.ContainsKey(eventName))
        {
            eventId = AK.WwiseDefine.dataRevEvents[eventName].ToString();
            eventGuid = AK.WwiseDefine.dataEvents[AK.WwiseDefine.dataRevEvents[eventName]].Guid;
        }
        GUILayout.Label("Event Id: " + eventId);
        GUILayout.Label("Event Guid: " + eventGuid);

        GUILayout.FlexibleSpace();

        // Options
        GUILayout.BeginHorizontal();
        GUI.color = Color.cyan;
        if (GUILayout.Button("Post"))
        {
            AD_WwiseManager.Instance.StopAll();
            AD_WwiseManager.Instance.PostEvent(eventName);
        }
        GUI.color = Color.red;
        if (GUILayout.Button("Stop"))
        {
            AD_WwiseManager.Instance.StopAll();
        }
        GUI.color = Color.white;
        GUILayout.EndHorizontal();

        GUI.color = Color.green;
        if (GUILayout.Button("Select"))
        {
            AD_WwiseManager.Instance.StopAll();
            Selected = true;
            Close();

            selectedCallback?.Invoke();
        }
        GUI.color = Color.white;
    }

    public static bool WwiseEventProperty(
        string eventSelection, string Tips = "Event", System.Action<string> selectedCallback = null,
        bool? is3DSound = null
    )
    {
        bool clicked3DButton = false;

        GUILayout.BeginHorizontal();

        GUILayout.Label(Tips, GUILayout.Width(72));

        if (is3DSound.HasValue)
        {
            clicked3DButton = GUILayout.Button(is3DSound.Value ? "3D" : "2D", GUILayout.Width(36));
        }

        GUI.color = AK.WwiseDefine.dataRevEvents.ContainsKey(eventSelection) ? Color.green : Color.red;
        bool Select = GUILayout.Button(eventSelection);
        GUI.color = Color.white;

        GUILayout.EndHorizontal();

        if (Select)
        {
            var Picker = EditorWindow.GetWindow<WwiseEventPicker>();
            Picker.InitWith(eventSelection, () =>
            {
                Picker.GetResult(out string selectedEventName);
                selectedCallback?.Invoke(selectedEventName);
            });
            Picker.Show();
        }

        return clicked3DButton;
    }
}

#endif // UNITY_EDITOR