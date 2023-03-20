using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TL_SignalWwise : UnityEngine.Timeline.Marker, INotification
{
    public PropertyName id { get; }

    [SerializeField]
    public string m_kWwiseEvent;
}

#if UNITY_EDITOR

[UnityEditor.CustomEditor(typeof(TL_SignalWwise))]
public class TL_SignalWwiseEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        UnityEditor.EditorGUILayout.HelpBox("Wwise Event Only Trigger in Play Mode", UnityEditor.MessageType.Info);

        base.OnInspectorGUI();
    }
}

#endif