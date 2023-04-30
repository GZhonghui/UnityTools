#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AD_WwiseImport))]
public class WwiseImportEditor : Editor
{
    private string eventSelection = "";

    private bool needSeek = false;
    private bool autoStop = false;

    private int Duration = 0;
    private int stopTransition = 0;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var ownGo = target as AD_WwiseImport;

        WwiseEventPicker.WwiseEventProperty(eventSelection, selectedCallback: (selectedEventName) =>
        {
            eventSelection = selectedEventName;
        });

        // needSeek = EditorGUILayout.Toggle("Need Seek", needSeek);
        // autoStop = EditorGUILayout.Toggle("Auto Stop", autoStop);

        GUILayout.BeginHorizontal();
        GUI.color = Color.green;
        if (GUILayout.Button("Post"))
        {
            AD_WwiseManager.Instance.PostEvent(eventSelection, ownGo.AudioSource, needSeek: needSeek);
        }
        GUI.color = Color.red;
        if (GUILayout.Button("Stop"))
        {
            AD_WwiseManager.Instance.StopEvent(eventSelection, ownGo.AudioSource);
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

        // Debug Sphere
        if (GUILayout.Button((AD_WwiseMonoBase.m_bDrawGizmos ? "Hide" : "Show") + " Source Gizmos"))
        {
            AD_WwiseMonoBase.m_bDrawGizmos = !AD_WwiseMonoBase.m_bDrawGizmos;
        }
    }
}

#endif // UNITY_EDITOR