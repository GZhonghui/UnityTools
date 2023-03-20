using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TL_ClipEffect : PlayableAsset
{
    public enum EffectType
    {
        PureColor = 0,
    }

    // 相机效果的类型
    [HideInInspector]
    public EffectType m_uEffectType = EffectType.PureColor;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        return ScriptPlayable<TL_BehaviourEffect>.Create(graph);
    }
}

#if UNITY_EDITOR

[UnityEditor.CustomEditor(typeof(TL_ClipEffect))]
public class TL_ClipEffectEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var uEffect = serializedObject.FindProperty("m_uEffectType");
        if (uEffect != null)
        {
            uEffect.intValue = UnityEditor.EditorGUILayout.Popup("Camera Effect Type", uEffect.intValue, new string[]
            {
                "PureColor"
            });

            serializedObject.ApplyModifiedProperties();
        }

        base.OnInspectorGUI();
    }
}

#endif