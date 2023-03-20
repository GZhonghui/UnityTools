using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UIElements;

public static class TL_AssetHandler
{
#if UNITY_EDITOR
    public static void Export(GameObject kDirectorGo)
    {
        var kDirector = kDirectorGo.GetComponent<PlayableDirector>();
        if (kDirector == null) return;

        // Timeline的数据
        var kPlayAsset = kDirector.playableAsset as TimelineAsset;
        if (kPlayAsset == null) return;

        // 不再使用Json保存Binding，转为使用Unity Asset
        // 因为在Runtime没有办法拿到FileId，还是要靠Unity内部的序列化

        // 用于导出的数据
        var kBindingExport = ScriptableObject.CreateInstance<TL_Binding>();
        kBindingExport.m_kAssets = new List<TL_Binding.AssetItem>();
        kBindingExport.m_kBindings = new List<TL_Binding.BindingItem>();
        kBindingExport.m_kReferences = new List<TL_Binding.ReferenceItem>();

        // Step 1: 保存PlayableAsset
        var kPlayAssetPath = UnityEditor.AssetDatabase.GetAssetPath(kPlayAsset);
        var kPlayAssetGuid = UnityEditor.AssetDatabase.AssetPathToGUID(kPlayAssetPath);
        kBindingExport.m_kTimelineAssetPath = kPlayAssetPath;
        kBindingExport.m_kTimelineAssetGuid = kPlayAssetGuid;

        // 维护外部资源的依赖关系
        // <Prefab Guid, <Instance Id, Index>>
        var kRefMap = new Dictionary<string, Dictionary<GameObject, int>>();
        // 计算引用关系
        Func<string, GameObject, int> _calcTargetPrefabIndex = (kGuid, kGo) =>
        {
            if (!kRefMap.ContainsKey(kGuid))
            {
                kRefMap[kGuid] = new Dictionary<GameObject, int>();
            }
            var kPrefabMap = kRefMap[kGuid];
            if (!kPrefabMap.ContainsKey(kGo))
            {
                int iIndex = kPrefabMap.Count;
                kPrefabMap[kGo] = iIndex;
            }
            return kPrefabMap[kGo];
        };

        // Step 2: 迭代时间轴的输出轨道
        foreach (var kOut in kPlayAsset.outputs)
        {
            var kBind = kDirector.GetGenericBinding(kOut.sourceObject);
            if (kBind == null) continue; // 没有设置绑定就不保存

            var kTrackAsset = kOut.sourceObject as TrackAsset;
            UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(kTrackAsset, out string kGuid, out long iFileId);

            // 获取的是绑定的外部资源
            var kBinding = new TL_Utility.BindingTarget();

            // 分轨道类型获取绑定目标
            // 动画轨道Animator
            if (kOut.sourceObject.GetType() == typeof(AnimationTrack))
            {
                var kBindAnimator = kBind as Animator;
                kBinding = TL_Utility.ExportBinding(kBindAnimator.gameObject);
            }
            // Activation轨道
            else if (kOut.sourceObject.GetType() == typeof(ActivationTrack))
            {
                var kBindGameObject = kBind as GameObject;
                kBinding = TL_Utility.ExportBinding(kBindGameObject);
            }
            // Cinemachine轨道不绑定，运行时直接绑定Main Camera
            else if (kOut.sourceObject.GetType() == typeof(CinemachineTrack))
            {
                
            }
            else if (kOut.sourceObject.GetType() == typeof(TL_TrackCamera))
            {
                var kBindCameraPath = kBind as GameObject;
                if (UnityEditor.PrefabUtility.IsPartOfAnyPrefab(kBindCameraPath))
                {
                    kBinding = TL_Utility.ExportBinding(UnityEditor.PrefabUtility.GetOutermostPrefabInstanceRoot(kBindCameraPath));
                }
            }
            else if (kOut.sourceObject.GetType() == typeof(TL_TrackEffect))
            {

            }
            else if (kOut.sourceObject.GetType() == typeof(TL_TrackParticle))
            {
                var kBindGameObject = kBind as GameObject;
                kBinding = TL_Utility.ExportBinding(kBindGameObject);
            }
            else if (kOut.sourceObject.GetType() == typeof(TL_TrackSubtitle))
            {

            }
            else if (kOut.sourceObject.GetType() == typeof(TL_TrackWwise))
            {
                var kBindGameObject = kBind as GameObject;
                kBinding = TL_Utility.ExportBinding(kBindGameObject);
            }

            if (kBinding.m_bValid)
            {
                var kThisBinding = new TL_Binding.BindingItem();

                // 轨道
                kThisBinding.m_kKeyTrack = kTrackAsset;
                kThisBinding.m_iKeyTrackFileId = iFileId;

                // 轨道目标
                // 保存引用的是第几个Prefab，在同一个Prefab有多个引用时用于区分
                kThisBinding.m_iBindTargetPrefabIndex = _calcTargetPrefabIndex(kBinding.m_kPrefabAssetGuid, kBinding.m_kPrefabRootInScene);
                kThisBinding.m_kBindTargetPrefabPath = kBinding.m_kPrefabAssetPath;
                kThisBinding.m_kBindTargetPrefabGuid = kBinding.m_kPrefabAssetGuid;
                // 绑定目标在Prefab内部的路径
                kThisBinding.m_kBindTargetInsidePath = kBinding.m_kPathInPrefab;

                kBindingExport.m_kBindings.Add(kThisBinding);
            }
        }

        // Step 3: 保存引用
        UnityEditor.SerializedObject kSo = new UnityEditor.SerializedObject(kDirector);
        var kRefList = kSo.FindProperty("m_ExposedReferences").FindPropertyRelative("m_References");
        for (int i = 0; i < kRefList.arraySize; i++)
        {
            var kRef = kRefList.GetArrayElementAtIndex(i);
            var kO = kDirector.GetReferenceValue(kRef.displayName, out bool bIs);
            if (bIs)
            {
                var kMono = kO as CinemachineVirtualCamera;
                if (kMono != null && kMono.gameObject.scene.name != null && UnityEditor.PrefabUtility.IsPartOfAnyPrefab(kMono.gameObject))
                {
                    var kPrefab = UnityEditor.PrefabUtility.GetOutermostPrefabInstanceRoot(kMono.gameObject);
                    var kPath = UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(kPrefab);
                    var kGuid = UnityEditor.AssetDatabase.GUIDFromAssetPath(kPath).ToString();

                    var kReference = new TL_Binding.ReferenceItem();

                    // Key
                    kReference.m_kName = kRef.displayName;
                    // Value
                    kReference.m_kReferenceTargetPrefabGuid = kGuid;
                    kReference.m_kReferenceTargetPrefabPath = kPath;
                    kReference.m_iReferenceTargetPrefabIndex = _calcTargetPrefabIndex(kGuid, kPrefab);
                    TL_Utility.FindPathFromRootPrefab(kMono.gameObject, out kReference.m_kReferenceTargetInsidePath);

                    kBindingExport.m_kReferences.Add(kReference);
                }
            }
        }

        // Step 4: 所有的轨道遍历结束后，保存资源的依赖关系
        foreach (var kItem in kRefMap)
        {
            string kPrefabGuid = kItem.Key;
            string kPrefabPath = UnityEditor.AssetDatabase.GUIDToAssetPath(kPrefabGuid);

            foreach (var kInner in kItem.Value)
            {
                var kT = kInner.Key.transform;

                var kAssetRef = new TL_Binding.AssetItem();
                kAssetRef.m_kPrefabPath = kPrefabPath;
                kAssetRef.m_kPrefabGuid = kPrefabGuid;
                kAssetRef.m_iPrefabIndex = kInner.Value;
                kAssetRef.m_kTransform = new List<float>
                {
                    kT.position.x, kT.position.y, kT.position.z,
                    kT.rotation.x, kT.rotation.y, kT.rotation.z,
                    kT.localScale.x, kT.localScale.y, kT.localScale.z
                };

                kBindingExport.m_kAssets.Add(kAssetRef);
            }
        }

        // 保存的默认目录
        TL_Utility.TryCreateFolder(TL_Const.kTimelineBindingAssetPath);
        var kSavePath = UnityEditor.EditorUtility.SaveFilePanel("Export Timeline Binding", TL_Const.kTimelineBindingAssetPathStr, "Export", "asset");
        if (kSavePath != null && kSavePath.Length > 0)
        {
            var kSaveAssetPath = TL_Utility.SystemToAssetPath(kSavePath);
            if (kSaveAssetPath != null)
            {
                // TODO
                UnityEditor.AssetDatabase.CreateAsset(kBindingExport, kSaveAssetPath);
                UnityEditor.EditorUtility.SetDirty(kBindingExport);
                UnityEditor.AssetDatabase.SaveAssets();
                UnityEditor.AssetDatabase.Refresh();
            }
        }
    }

    public static void Import()
    {

    }
#endif

    // 加载配置文件，可以在运行时调用
    public static void Load()
    {

    }
}
