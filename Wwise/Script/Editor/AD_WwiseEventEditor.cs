using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[UnityEditor.CustomEditor(typeof(AD_WwiseEvent))]
public class AD_WwiseEventEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        /*
        var ownGo = target as AD_WwiseEvent;

        serializedObject.Update();

        for (int i = 0; i < ownGo.Events.Count; i++)
        {
            WwiseEventPicker.WwiseEventProperty(ownGo.Events[i], $"Event# {i}", serializedObject);
        }

        GUILayout.BeginHorizontal();
        GUI.color = Color.green;
        if (GUILayout.Button("Add"))
        {
            ownGo.Events.Add(new WwiseEventSelection()); 
        }
        GUI.color = Color.red;
        if (GUILayout.Button("Remove"))
        {
            if (ownGo.Events.Count > 0)
            {
                ownGo.Events.RemoveAt(ownGo.Events.Count - 1);
            }
        }
        GUI.color = Color.white;
        GUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
        */
    }
}