using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TL_SignalCommon : UnityEngine.Timeline.Marker, INotification
{
    // INotification
    public PropertyName id { get; }

    [SerializeField]
    public string m_kSignalTag;
}

#if UNITY_EDITOR

[UnityEditor.CustomEditor(typeof(TL_SignalCommon))]
public class TL_SignalCommonEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        // 只有Marker轨道会处理这个信号
        UnityEditor.EditorGUILayout.HelpBox("Common Signal Only Hold in Marker", UnityEditor.MessageType.Info);

        base.OnInspectorGUI();
    }
}

#endif
