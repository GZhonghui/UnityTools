#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AD_WwiseBank))]
public class AD_WwiseBankEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var ownGo = target as AD_WwiseBank;

        serializedObject.Update();

        var bankProperty = serializedObject.FindProperty("m_LoadBanks");
        var bankCount = bankProperty.arraySize;

        var baseLifeCycle = serializedObject.FindProperty("m_LifeCycle");

        var triggerTypeStr = new string[]
        {
            "None",
            "Component Start Destroy",
            "Trigger Enter Exit"
        };

        var triggerType = new AD_WwiseMonoBase.LifeCycleType[]
        {
            AD_WwiseMonoBase.LifeCycleType.None,
            AD_WwiseMonoBase.LifeCycleType.ComponentStartDestroy,
            AD_WwiseMonoBase.LifeCycleType.TriggerEnterExit
        };

        var triggerTypeInv = new Dictionary<AD_WwiseMonoBase.LifeCycleType, int>();
        for (int i = 0; i < triggerType.Length; i += 1)
        {
            triggerTypeInv[triggerType[i]] = i;
        }

        EditorGUILayout.HelpBox("Load and Keep Bank in Memory", MessageType.Info);

        // Buttons
        GUILayout.BeginHorizontal();
        GUI.color = Color.green;
        bool clickedAdd = GUILayout.Button("Add");
        GUI.color = Color.red;
        bool clickedRemove = GUILayout.Button("Remove");
        GUI.color = Color.white;
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Trigger", GUILayout.Width(64));

        // Trigger
        int selectIndex = 0;
        if (triggerTypeInv.ContainsKey((AD_WwiseMonoBase.LifeCycleType)baseLifeCycle.intValue))
        {
            selectIndex = triggerTypeInv[(AD_WwiseMonoBase.LifeCycleType)baseLifeCycle.intValue];
        }

        selectIndex = UnityEditor.EditorGUILayout.Popup(selectIndex, triggerTypeStr);

        if (selectIndex >= 0 && selectIndex < triggerType.Length)
        {
            baseLifeCycle.intValue = (int)triggerType[selectIndex];
        }

        GUILayout.EndHorizontal();

        // Need a Collider
        if (baseLifeCycle.intValue >= (int)AD_WwiseMonoBase.LifeCycleType.TriggerEnterExit
            && baseLifeCycle.intValue <= (int)AD_WwiseMonoBase.LifeCycleType.TriggerExit)
        {
            if (ownGo.GetComponent<Collider>() == null)
            {
                // Default
                var bC = ownGo.gameObject.AddComponent<BoxCollider>();
                bC.isTrigger = true;
            }

            // Set isTrigger
            var cS = ownGo.GetComponents<Collider>();
            for (int j = 0; j < cS.Length; j += 1)
            {
                if (cS[j].GetType() != typeof(BoxCollider) && cS[j].GetType() != typeof(SphereCollider))
                {
                    DestroyImmediate(cS[j]);
                    continue;
                }
                cS[j].isTrigger = true;
            }
        }

        // Select Bank
        for (int i = 0; i < bankCount; i += 1)
        {
            // List Bank Names
            int Index = 0;
            List<string> bankNames = new List<string>();
            Dictionary<string, int> bankNamesInv = new Dictionary<string, int>();
            foreach (var Item in AK.WwiseDefine.dataBanks)
            {
                if (Item.Value.Name == "Init") continue;

                bankNames.Add(Item.Value.Name);
                bankNamesInv[Item.Value.Name] = Index;

                Index += 1;
            }

            var bankData = bankProperty.GetArrayElementAtIndex(i);

            int Selected = bankNamesInv.ContainsKey(bankData.stringValue) ?
                bankNamesInv[bankData.stringValue] : -1;

            // Select Error Name
            bool SelectedIsError = Selected < 0;
            if (SelectedIsError && bankData.stringValue.Length > 0)
            {
                bankNames.Add(bankData.stringValue);
                bankNamesInv[bankData.stringValue] = Index;
                Selected = Index;
            }

            GUILayout.BeginHorizontal();

            GUILayout.Label($"Bank# {i}", GUILayout.Width(64));

            GUI.color = SelectedIsError ? Color.red : Color.green;
            Selected = EditorGUILayout.Popup(Selected, bankNames.ToArray());
            GUI.color = Color.white;

            GUILayout.EndHorizontal();

            if (Selected >= 0 && Selected < bankNames.Count)
            {
                bankData.stringValue = bankNames[Selected];
            }
        }

        if (clickedAdd)
        {
            bankProperty.InsertArrayElementAtIndex(bankCount);
            bankProperty.GetArrayElementAtIndex(bankCount).stringValue = "";
        }

        if (clickedRemove)
        {
            if (bankCount > 0)
            {
                bankProperty.DeleteArrayElementAtIndex(bankCount - 1);
            }
        }

        if (serializedObject.hasModifiedProperties)
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif // UNITY_EDITOR