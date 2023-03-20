using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

// 保存一份完整可用的动画片段
// 将Timeline和Track Binding都捆绑存储在一起
// 由此Timeline作为Binding的一个组成部分，可以重用
public class TL_Binding : ScriptableObject
{
    // 保存资源的依赖关系
    [Serializable]
    public struct AssetItem
    {
        public string m_kPrefabPath;
        public string m_kPrefabGuid;
        public int m_iPrefabIndex;
        public List<float> m_kTransform;
    }

    // 保存一条轨道的绑定关系
    [Serializable]
    public struct BindingItem
    {
        // Key
        public TrackAsset m_kKeyTrack;
        public long m_iKeyTrackFileId;
        // Value
        public int m_iBindTargetPrefabIndex;
        public string m_kBindTargetPrefabPath;
        public string m_kBindTargetPrefabGuid;
        public List<string> m_kBindTargetInsidePath;
    }

    // 保存引用
    [Serializable]
    public struct ReferenceItem
    {
        public string m_kName;
        public int m_iReferenceTargetPrefabIndex;
        public string m_kReferenceTargetPrefabPath;
        public string m_kReferenceTargetPrefabGuid;
        public List<string> m_kReferenceTargetInsidePath;
    }

    // Timeline数据
    [SerializeField]
    public string m_kTimelineAssetPath;
    [SerializeField]
    public string m_kTimelineAssetGuid;

    // 依赖的外部资源
    [SerializeField]
    public List<AssetItem> m_kAssets;

    // 轨道绑定关系
    [SerializeField]
    public List<BindingItem> m_kBindings;

    [SerializeField]
    public List<ReferenceItem> m_kReferences;
}