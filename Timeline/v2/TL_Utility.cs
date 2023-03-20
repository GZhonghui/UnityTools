using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Cinemachine;
using System.IO;

public static class TL_Utility
{
    private static CinemachineVirtualCamera m_CacheTimelineCamera = null;

    // 描述一个轨道绑定的目标
    public struct BindingTarget
    {
        public bool m_bValid;
        public GameObject m_kGoInScene;
        public GameObject m_kPrefabRootInScene;
        public string m_kPrefabAssetPath;
        public string m_kPrefabAssetGuid;
        public List<string> m_kPathInPrefab;
    }

    // 相机轨道使用
    public static CinemachineVirtualCamera FindTimelineCamera()
    {
        if (m_CacheTimelineCamera != null) return m_CacheTimelineCamera;

        CinemachineVirtualCamera kResult = null;
        var kCameras = GameObject.FindObjectsOfType<CinemachineVirtualCamera>();
        for (int i = 0; i < kCameras.Length; i++)
        {
            if (kCameras[i].name == TL_Const.kTimelineCameraName)
            {
                kResult = kCameras[i];
                break;
            }
        }

        if (kResult == null)
        {
            var kGo = new GameObject(TL_Const.kTimelineCameraName);
            kResult = kGo.AddComponent<CinemachineVirtualCamera>();
        }

        m_CacheTimelineCamera = kResult; // Save
        return kResult;
    }

    // 找到主相机，用于修改Cull Mask
    public static Camera FindMainCamera()
    {
        return GameObject.Find("Main Camera").GetComponent<Camera>();
    }

    // 递归设置Layer
    public static void SetLayer(GameObject kGo, int iLayer)
    {
        kGo.layer = iLayer;
        foreach (Transform kChild in kGo.transform)
        {
            SetLayer(kChild.gameObject, iLayer);
        }
    }

    // 按照路径找到子物体
    public static GameObject FindChildThroughPath(GameObject kRoot, List<string> kPath)
    {
        if (kRoot == null) return null;
        if (kPath == null) return kRoot;

        var kNow = kRoot.transform;
        for (int i = 0; i < kPath.Count; i++)
        {
            if (kNow == null) break;
            kNow = kNow.Find(kPath[i]);
        }
        return kNow == null ? null : kNow.gameObject;
    }

#if UNITY_EDITOR
    // 找到场景中的PlayDirector，由这个函数唯一确定
    // Runtime中不会使用到
    public static GameObject FindMainTimelineEditorInScene()
    {
        var kDirectors = GameObject.FindObjectsOfType<PlayableDirector>();

        // if (kDirectors.Length == 0)
        {
            // 场景中没有Director，创建新的
            // var kEditor = new GameObject(TL_Const.kTimelineEditorName);
            // kEditor.AddComponent<PlayableDirector>();
            // return kEditor;
        }
        // else
        {
            // 删除其余的Director
            for (int i = 0; i < kDirectors.Length; i += 1)
            {
                if (kDirectors[i].name == TL_Const.kTimelineEditorName)
                {
                    return kDirectors[i].gameObject;
                }
                // GameObject.DestroyImmediate(kDirectors[i]);
            }

            // 返回第一个Director
            // kDirectors[0].enabled = true;
            // return kDirectors[0].gameObject;
        }

        var kEditor = new GameObject(TL_Const.kTimelineEditorName);
        kEditor.AddComponent<PlayableDirector>();
        return kEditor;
    }

    // 创建路径
    public static void TryCreateFolder(string[] kFolderPath)
    {
        string kNowDepth = "Assets";
        for (int i = 0; i < kFolderPath.Length; i += 1)
        {
            if (!UnityEditor.AssetDatabase.IsValidFolder($"{kNowDepth}/{kFolderPath[i]}"))
            {
                UnityEditor.AssetDatabase.CreateFolder(kNowDepth, kFolderPath[i]);
            }
            kNowDepth = $"{kNowDepth}/{kFolderPath[i]}";
        }
    }

    // 导出一个轨道绑定的目标
    // kTarget是场景中的物体
    public static BindingTarget ExportBinding(GameObject kTarget)
    {
        BindingTarget kBinding = new BindingTarget();
        kBinding.m_bValid = false;

        // 绑定的对象需要在场景中
        if (kTarget != null && kTarget.scene.name != null)
        {
            // 绑定的对象需要在Prefab中
            if (UnityEditor.PrefabUtility.IsPartOfAnyPrefab(kTarget))
            {
                kBinding.m_bValid = true; // 是一个有效绑定

                kBinding.m_kGoInScene = kTarget;

                // Prefab的根节点，需要加载的内容
                var kPrefabRoot = UnityEditor.PrefabUtility.GetOutermostPrefabInstanceRoot(kTarget);
                kBinding.m_kPrefabRootInScene = kPrefabRoot;

                // Prefab的路径
                var kRootPath = UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(kPrefabRoot);
                kBinding.m_kPrefabAssetPath = kRootPath;

                // Prefab的Guid
                var kRootGuid = UnityEditor.AssetDatabase.AssetPathToGUID(kRootPath);
                kBinding.m_kPrefabAssetGuid = kRootGuid;

                FindPathFromRootPrefab(kTarget, out kBinding.m_kPathInPrefab);
            }
        }

        return kBinding;
    }

    // 绝对路径转为相对路径
    public static string SystemToAssetPath(string kSystemPath)
    {
        if (kSystemPath.StartsWith(Application.dataPath))
        {
            return "Assets" + kSystemPath.Substring(Application.dataPath.Length);
        }
        return null;
    }

    // 目标相对于Prefab根节点的相对路径
    public static bool FindPathFromRootPrefab(GameObject kTarget, out List<string> kPath)
    {
        kPath = new List<string>();

        // 不在场景中，或者不在Prefab中
        if (kTarget == null || kTarget.scene.name == null || !UnityEditor.PrefabUtility.IsPartOfAnyPrefab(kTarget))
        {
            return false;
        }

        var kNow = kTarget;
        var kRoot = UnityEditor.PrefabUtility.GetOutermostPrefabInstanceRoot(kTarget);
        for (int i = 0; i < 32; i += 1)
        {
            if (kNow == kRoot) break;
            kPath.Add(kNow.name);
            kNow = kNow.transform.parent.gameObject;
        }
        kPath.Reverse();

        return true;
    }
#endif
}
