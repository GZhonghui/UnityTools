using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TL_ClipCamera : PlayableAsset
{
    public enum CameraRotationType
    {
        Cinemachine = 0,
        Lookat = 1,
        Manual = 2,
    }

    public double m_fStart;
    public double m_fEnd;
    // 相机的旋转如何驱动
    [HideInInspector]
    public CameraRotationType m_uCameraRotation = CameraRotationType.Cinemachine;
    public List<string> m_kInsidePath;
    public List<string> m_kTargetPath;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var kPlay = ScriptPlayable<TL_BehaviourCamera>.Create(graph);

        var kBehaviour = kPlay.GetBehaviour() as TL_BehaviourCamera;
        kBehaviour.m_fStart = m_fStart;
        kBehaviour.m_fEnd = m_fEnd;
        kBehaviour.m_uCameraRotation = m_uCameraRotation;
        kBehaviour.m_kInsidePath = m_kInsidePath;
        kBehaviour.m_kTargetPath = m_kTargetPath;

        return kPlay;
    }
}

#if UNITY_EDITOR

[UnityEditor.CustomEditor(typeof(TL_ClipCamera))]
public class TL_ClipCameraEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        var ownGo = target as TL_ClipCamera;

        UnityEditor.EditorGUILayout.HelpBox("Only Select the GameObject in Your Binding Prefab", UnityEditor.MessageType.Warning);

        serializedObject.Update();
        // 选择相机的旋转方式
        var uRot = serializedObject.FindProperty("m_uCameraRotation");
        if (uRot != null)
        {
            // Sync enum CameraRotationType
            uRot.intValue = UnityEditor.EditorGUILayout.Popup("Camera Rotation Type", uRot.intValue, new string[]
            {
                "Cinemachine", "Lookat", "Manual"
            });
            serializedObject.ApplyModifiedProperties();
        }

        GUI.color = Color.green;
        UnityEngine.Object kSelect = UnityEditor.EditorGUILayout.ObjectField("Cinemachine Path", null, typeof(Cinemachine.CinemachinePathBase), true);
        UnityEngine.Object kLookat = null;
        if (uRot.intValue == (int)TL_ClipCamera.CameraRotationType.Lookat)
        {
            kLookat = UnityEditor.EditorGUILayout.ObjectField("Lookat Target", null, typeof(GameObject), true);
        }
        GUI.color = Color.white;

        base.OnInspectorGUI();

        Action<string, List<string>> FillStringArray = (kPropertyName, kArray) =>
        {
            serializedObject.Update();

            var sPath = serializedObject.FindProperty(kPropertyName);
            if (sPath != null)
            {
                sPath.arraySize = kArray.Count;
                for (int i = 0; i < kArray.Count; i++)
                {
                    sPath.GetArrayElementAtIndex(i).stringValue = kArray[i];
                }

                serializedObject.ApplyModifiedProperties();
            }
        };

        if (kSelect != null)
        {
            if (TL_Utility.FindPathFromRootPrefab(((CinemachinePathBase)kSelect).gameObject, out List<string> kPaths))
            {
                FillStringArray("m_kInsidePath", kPaths);
            }
        }

        if (kLookat != null)
        {
            if (TL_Utility.FindPathFromRootPrefab((GameObject)kLookat, out List<string> kPaths))
            {
                FillStringArray("m_kTargetPath", kPaths);
            }
        }
    }
}

#endif
