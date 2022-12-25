using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AD_WwiseImport))]
public class WwiseImportEditor : Editor
{
    private WwiseEventSelection eventSelection = new WwiseEventSelection();

    private bool needSeek = false;
    private bool autoStop = false;

    private int Duration = 0;
    private int stopTransition = 0;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var ownGo = target as AD_WwiseImport;

        WwiseEventPicker.WwiseEventProperty(eventSelection);

        needSeek = EditorGUILayout.Toggle("Need Seek", needSeek);
        autoStop = EditorGUILayout.Toggle("Auto Stop", autoStop);

        GUILayout.BeginHorizontal();
        GUI.color = Color.green;
        if (GUILayout.Button("Post"))
        {
            AD_WwiseManager.Instance.PostEvent(eventSelection.eventName, ownGo.AudioSource, needSeek: needSeek);
        }
        GUI.color = Color.red;
        if (GUILayout.Button("Stop"))
        {
            AD_WwiseManager.Instance.StopEvent(eventSelection.eventName, ownGo.AudioSource);
        }
        GUI.color = Color.white;
        GUILayout.EndHorizontal();

        GUI.color = Color.cyan;
        if (GUILayout.Button("Reset All Event"))
        {
            AD_WwiseManager.Instance.StopAll();
        }
        if (GUILayout.Button("Reset Sound Engine"))
        {
            AD_WwiseImport.DoInitWwise();
        }
        GUI.color = Color.white;
    }
}