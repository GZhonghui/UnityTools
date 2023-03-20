using Cinemachine;
using HedgehogTeam.EasyTouch;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TL_Manager
{
    #region Signleton
    protected static TL_Manager _instance = new TL_Manager();

    private bool m_bInitialized = false;

    public static TL_Manager Instance
    {
        get
        {
            _instance.Init();
            return _instance;
        }
    }

    private void Init()
    {
        if (m_bInitialized) return;

        m_PrefabInstance = new Dictionary<uint, Dictionary<string, Dictionary<int, GameObject>>>();
        m_Asset = new Dictionary<uint, Dictionary<string, GameObject>>();
        m_Binding = new Dictionary<uint, TL_Binding>();
        m_Timeline = new Dictionary<uint, TimelineAsset>();
        m_States = new Dictionary<uint, PlayStatus>();
        m_bInitialized = true;
    }
    #endregion

    public enum PlayStatus
    {
        WAITING = 1, // 从来没有申请过这个ID
        APPLIED, // ID已经分配好，等待加载
        LOADING, // 正在加载
        LOADED, // 加载完成
        INSTANTIATED, // 实例化完成
        PLAYING, // 正在播放
        PAUSED, // 暂停中
        FINISHED, // 生命周期结束
    }

    public enum ErrorCode
    {
        SUCCESS = 0,
        BAD_ID,
        BAD_NAME,
        ASSET_FAILED,
        NOT_READY,
        STATUS_ERROR,
    }

    // 播放信息
    public struct PlayInfo
    {
        public uint m_uId;
        public string m_kName;
        public PlayStatus m_uPlayStatus;
    }

    // Binding最先加载
    // PlayId, Binding
    Dictionary<uint, TL_Binding> m_Binding;
    // PlayId, Asset Guid, Index, Go
    Dictionary<uint, Dictionary<string, Dictionary<int, GameObject>>> m_PrefabInstance;
    // PlayId, Asset Guid, Asset
    Dictionary<uint, Dictionary<string, GameObject>> m_Asset;
    // PlayId, Timeline
    Dictionary<uint, TimelineAsset> m_Timeline;
    // PlayId, PlayStatus
    Dictionary<uint, PlayStatus> m_States;

    private GameObject m_kManagerRoot = null;

    private uint m_uPlayId = 0;

    private int m_uCacheCullingMask = 0;
    private int m_uPlayStatusCounter = 0;

    // TODO
    private GameObject _cacheUiCamera = null;

    private uint ApplyPlayId()
    {
        m_uPlayId += 1;
        m_States[m_uPlayId] = PlayStatus.LOADING;
        return m_uPlayId;
    }

    private uint TotalPlayCount()
    {
        return m_uPlayId;
    }

    private string GetPlayDirectorName(uint playId)
    {
        return $"Timeline_{playId}";
    }

    private GameObject GetTimelineRoot()
    {
        if (m_kManagerRoot != null) return m_kManagerRoot;

        GameObject managerRoot = GameObject.Find(TL_Const.kTimelineManagerName);
        if (managerRoot == null)
        {
            managerRoot = new GameObject(TL_Const.kTimelineManagerName);
        }

        m_kManagerRoot = managerRoot;
        return managerRoot;
    }

    private PlayableDirector GetPlayDirector(uint playId)
    {
        var findResult = GetTimelineRoot().transform.Find(GetPlayDirectorName(playId));
        return findResult == null ? null : findResult.gameObject.GetComponent<PlayableDirector>();
    }

    // 分配ID
    private void OnApply(uint playId)
    {
        SetStatus(playId, PlayStatus.APPLIED);
    }

    // 开始加载
    private void OnLoadStart(uint playId)
    {
        SetStatus(playId, PlayStatus.LOADING);
    }

    // 加载结束
    private void OnLoadFinish(uint playId)
    {
        SetStatus(playId, PlayStatus.LOADED);
    }

    // 实例化
    private void OnInstantiate(uint playId)
    {
        SetStatus(playId, PlayStatus.INSTANTIATED);
    }

    // 播放
    private void OnPlay(uint playId)
    {
        SetStatus(playId, PlayStatus.PLAYING);
    }

    // 暂停
    private void OnPause(uint playId)
    {
        SetStatus(playId, PlayStatus.PAUSED);
    }

    // 播放结束
    private void OnFinish(uint playId)
    {
        SetStatus(playId, PlayStatus.FINISHED);

        var kDirector = GetPlayDirector(playId);
        if (kDirector != null)
        {
            GameObject.Destroy(kDirector.gameObject);
        }

        ReleaseAsset(playId);
    }

    // 处理相机
    private void EnterPlayStatus()
    {
        m_uPlayStatusCounter += 1;
        if (m_uPlayStatusCounter == 1)
        {
            var kCamera = TL_Utility.FindMainCamera();
            m_uCacheCullingMask = kCamera.cullingMask;
            kCamera.cullingMask = 0 | (1 << LayerMask.NameToLayer(TL_Const.kTimelineInstanceLayerName));

            // TODO
            if (_cacheUiCamera == null) _cacheUiCamera = GameObject.Find("UI Camera");
            if (_cacheUiCamera != null) _cacheUiCamera.SetActive(false);
        }
    }

    // 处理相机
    private void ExitPlayStatus()
    {
        m_uPlayStatusCounter -= 1;
        if (m_uPlayStatusCounter == 0)
        {
            var kCamera = TL_Utility.FindMainCamera();
            kCamera.cullingMask = m_uCacheCullingMask;

            // TODO
            if (_cacheUiCamera != null) _cacheUiCamera.SetActive(true);
        }
    }

    // TODO
    private GameObject LoadApi(string kName)
    {
        // await
        
        return null;
    }

    // TODO
    private void UnloadApi(GameObject kAsset)
    {

    }

    // 加载动画所需的资源，包括TimlineAsset和Prefab
    private async Task<bool> LoadAsset(uint playId)
    {
        if (!m_Binding.ContainsKey(playId)) return false;
        TL_Binding kBinding = m_Binding[playId];

        // Timeline
        var kLoader = Addressables.LoadAssetAsync<TimelineAsset>(kBinding.m_kTimelineAssetPath);
        await kLoader.Task;

        if (kLoader.Result == null)
        {
            return false;
        }

        m_Timeline[playId] = kLoader.Result;

        // Prefab
        if (!m_Asset.ContainsKey(playId)) m_Asset[playId] = new Dictionary<string, GameObject>();
        var kPlayMap = m_Asset[playId];
        foreach (var kItem in kBinding.m_kAssets)
        {
            if (!kPlayMap.ContainsKey(kItem.m_kPrefabGuid))
            {
                var kPrefabLoader = Addressables.LoadAssetAsync<GameObject>(kItem.m_kPrefabPath);
                await kPrefabLoader.Task;

                if(kPrefabLoader.Result == null)
                {
                    return false;
                }

                kPlayMap[kItem.m_kPrefabGuid] = kPrefabLoader.Result;
            }
        }

        return true;
    }

    // 实例化动画需要的GameObject，并且恢复绑定关系
    public bool InstantiateAsset(uint playId, bool autoPlay = false, Action playFinish = null)
    {
        if (GetStatus(playId) != PlayStatus.LOADED) return false;

        var kBindingAsset = m_Binding[playId];
        if (kBindingAsset == null) return false;

        var kPlayAsset = m_Timeline[playId];
        if (kPlayAsset == null) return false;

        Func<uint, string, GameObject> GetAsset = (_playId, _assetGuid) =>
        {
            if (!m_Asset.ContainsKey(_playId)) return null;
            if (!m_Asset[_playId].ContainsKey(_assetGuid)) return null;

            return m_Asset[_playId][_assetGuid];
        };

        Func<uint, string, int, GameObject> GetInstance = (_playId, _assetGuid, _index) =>
        {
            if (!m_PrefabInstance.ContainsKey(_playId)) return null;
            if (!m_PrefabInstance[_playId].ContainsKey(_assetGuid)) return null;
            if (!m_PrefabInstance[_playId][_assetGuid].ContainsKey(_index)) return null;

            return m_PrefabInstance[_playId][_assetGuid][_index];
        };

        // 实例化
        if (!m_PrefabInstance.ContainsKey(playId))
        {
            m_PrefabInstance[playId] = new Dictionary<string, Dictionary<int, GameObject>>();
        }
        // 这个动画使用到的GameObject
        // PlayId
        var kPlayMap = m_PrefabInstance[playId];
        foreach (var kItem in kBindingAsset.m_kAssets)
        {
            if (!kPlayMap.ContainsKey(kItem.m_kPrefabGuid)) kPlayMap[kItem.m_kPrefabGuid] = new Dictionary<int, GameObject>();

            // Guid
            var kGuidMap = kPlayMap[kItem.m_kPrefabGuid];

            var kPrefab = GetAsset(playId, kItem.m_kPrefabGuid);

            if (kPrefab != null)
            {
                var kTarget = UnityEngine.Object.Instantiate(kPrefab);

                // 更改Layer
                TL_Utility.SetLayer(kTarget, LayerMask.NameToLayer(TL_Const.kTimelineInstanceLayerName));

                // 创建到对应位置
                kGuidMap[kItem.m_iPrefabIndex] = kTarget;

                // 还原位移
                var kT = kItem.m_kTransform;
                kTarget.transform.position = new Vector3(kT[0], kT[1], kT[2]);
                kTarget.transform.rotation = Quaternion.Euler(kT[3], kT[4], kT[5]);
                kTarget.transform.localScale = new Vector3(kT[6], kT[7], kT[8]);
            }
        }

        // 创建代理
        var kPlayDirector = new GameObject(GetPlayDirectorName(playId));
        kPlayDirector.transform.parent = GetTimelineRoot().transform;
        kPlayDirector.SetActive(false);

        var kDirectorComponent = kPlayDirector.AddComponent<PlayableDirector>();
        kDirectorComponent.playableAsset = kPlayAsset; // 关联Timeline资源
        kDirectorComponent.playOnAwake = false;

        // 恢复绑定关系，以kPlayAsset为准
        foreach (var k in kPlayAsset.outputs)
        {
            bool bFoundTrack = false;
            var kBinding = new TL_Binding.BindingItem();

            // 找到对应轨道的绑定信息
            // kBindingAsset是我们保存的绑定关系
            foreach (var v in kBindingAsset.m_kBindings)
            {
                // 是同一个轨道
                if (v.m_kKeyTrack == k.sourceObject)
                {
                    bFoundTrack = true;
                    kBinding = v;
                    break;
                }
            }

            Type kTrackType = k.sourceObject != null ? k.sourceObject.GetType() : null;

            // 找到了绑定关系，并且需要恢复
            if (bFoundTrack)
            {
                var kPrefabRoot = GetInstance(playId, kBinding.m_kBindTargetPrefabGuid, kBinding.m_iBindTargetPrefabIndex);
                if (kPrefabRoot != null)
                {
                    // 绑定目标
                    var kTarget = TL_Utility.FindChildThroughPath(kPrefabRoot, kBinding.m_kBindTargetInsidePath);

                    if (kTarget != null)
                    {
                        if (kTrackType == typeof(AnimationTrack))
                        {
                            if (null == kTarget.GetComponent<Animator>()) kTarget.AddComponent<Animator>();
                            kDirectorComponent.SetGenericBinding(k.sourceObject, kTarget.GetComponent<Animator>());
                        }
                        else if (kTrackType == typeof(ActivationTrack))
                        {
                            kDirectorComponent.SetGenericBinding(k.sourceObject, kTarget);
                        }
                        else if (kTrackType == typeof(TL_TrackCamera))
                        {
                            kDirectorComponent.SetGenericBinding(k.sourceObject, kTarget);
                        }
                        else if (kTrackType == typeof(TL_TrackParticle))
                        {
                            // TODO
                        }
                        else if (kTrackType == typeof(TL_TrackWwise))
                        {
                            // TODO
                        }
                    }
                }
            }
            // 没有找到绑定关系，但是不需要恢复
            else
            {
                if (kTrackType == typeof(CinemachineTrack))
                {
                    // 绑定摄像机
                    var kMainCamera = TL_Utility.FindMainCamera();
                    if (kMainCamera != null)
                    {
                        kDirectorComponent.SetGenericBinding(k.sourceObject, kMainCamera.GetComponent<CinemachineBrain>());
                    }
                }
                else if (kTrackType == typeof(TL_TrackEffect))
                {

                }
                else if (kTrackType == typeof(TL_TrackSubtitle))
                {

                }
            }
        }

        // 恢复Reference
        foreach (var kReference in kBindingAsset.m_kReferences)
        {
            var kPrefabRoot = GetInstance(playId, kReference.m_kReferenceTargetPrefabGuid, kReference.m_iReferenceTargetPrefabIndex);
            if (kPrefabRoot == null) continue;

            var kTarget = TL_Utility.FindChildThroughPath(kPrefabRoot, kReference.m_kReferenceTargetInsidePath);

            if (kTarget != null)
            {
                // TODO
                var kCam = kTarget.GetComponent<CinemachineVirtualCamera>();
                if (kCam != null)
                {
                    kDirectorComponent.SetReferenceValue(kReference.m_kName, kCam);
                }
            }
        }

        kDirectorComponent.stopped += (kDirector) =>
        {
            ExitPlayStatus();
            OnFinish(playId);
            playFinish?.Invoke();
        };

        kPlayDirector.SetActive(true);
        OnInstantiate(playId);

        if (autoPlay)
        {
            Play(playId);
        }

        return true;
    }

    // 清理动画的所有资源
    // 可能在任何状态失败或者结束，清理的时候注意安全
    private bool ReleaseAsset(uint playId)
    {
        // Instance
        if (m_PrefabInstance.ContainsKey(playId))
        {
            foreach (var k in m_PrefabInstance[playId])
            {
                foreach (var v in k.Value)
                {
                    // 销毁由Timeline创建的对象
                    GameObject.Destroy(v.Value);
                }
            }

            // 从PlayId层清空一次表
            m_PrefabInstance.Remove(playId);
        }

        // Prefab Asset
        if (m_Asset.ContainsKey(playId))
        {
            foreach (var k in m_Asset[playId])
            {
                Addressables.Release<GameObject>(k.Value);
            }

            m_Asset.Remove(playId);
        }

        // Timeline
        if (m_Timeline.ContainsKey(playId))
        {
            Addressables.Release<TimelineAsset>(m_Timeline[playId]);
            m_Timeline.Remove(playId);
        }

        // Binding
        if (m_Binding.ContainsKey(playId))
        {
            Addressables.Release<TL_Binding>(m_Binding[playId]);
            m_Binding.Remove(playId);
        }

        return true;
    }

    // 设置播放状态
    private void SetStatus(uint playId, PlayStatus playStatus)
    {
        m_States[playId] = playStatus;
    }

    // 获取播放状态
    public PlayStatus GetStatus(uint playId)
    {
        if (m_States.ContainsKey(playId)) return m_States[playId]; return PlayStatus.WAITING;
    }

    // TODO
    // 获取所有正在播放的Timeline
    public ErrorCode GetAllPlaying(out List<uint> kAllPlaying)
    {
        kAllPlaying = new List<uint>();

        return ErrorCode.SUCCESS;
    }

    // 获取和Id相关的所有信息，也就是这个Timeline实例
    public ErrorCode GetInfo(uint playId, out PlayInfo kPlayInfo)
    {
        kPlayInfo.m_uId = playId;
        kPlayInfo.m_kName = "";
        kPlayInfo.m_uPlayStatus = GetStatus(playId);

        return ErrorCode.SUCCESS;
    }

    public uint Apply()
    {
        uint iId = ApplyPlayId();
        OnApply(iId);
        return iId;
    }

    public async void Load(uint playId, string bindingAssetName, bool autoPlay = false, Action playFinish = null)
    {
        // 状态错误
        if (GetStatus(playId) != PlayStatus.APPLIED) return;

        OnLoadStart(playId);

        // 加载Binding资源
        var kLoadHandler = Addressables.LoadAssetAsync<TL_Binding>(bindingAssetName);
        await kLoadHandler.Task;

        var kBindingAsset = kLoadHandler.Result;
        if (kBindingAsset == null)
        {
            // 加载失败，直接结束
            OnFinish(playId);
            return;
        }

        m_Binding[playId] = kBindingAsset;

        var Loader = LoadAsset(playId);
        await Loader;

        if (!Loader.Result)
        {
            OnFinish(playId);
            return;
        }

        OnLoadFinish(playId);

        if (autoPlay)
        {
            // 自动实例化
            InstantiateAsset(playId, true, playFinish);
        }
    }

    public ErrorCode Play(uint playId)
    {
        if (GetStatus(playId) == PlayStatus.PAUSED || GetStatus(playId) == PlayStatus.INSTANTIATED)
        {
            var kDirector = GetPlayDirector(playId);
            if (kDirector != null)
            {
                // 首次开始播放
                if (GetStatus(playId) == PlayStatus.INSTANTIATED)
                {
                    EnterPlayStatus();
                }

                kDirector.Play();
                OnPlay(playId);

                return ErrorCode.SUCCESS;
            }

            return ErrorCode.NOT_READY;
        }
        else return ErrorCode.STATUS_ERROR;
    }

    public ErrorCode Pause(uint playId)
    {
        if (GetStatus(playId) == PlayStatus.PLAYING)
        {
            var kDirector = GetPlayDirector(playId);
            if (kDirector != null)
            {
                kDirector.Pause();
                OnPause(playId);

                return ErrorCode.SUCCESS;
            }

            return ErrorCode.NOT_READY;
        }
        else return ErrorCode.STATUS_ERROR;
    }

    public ErrorCode Stop(uint playId)
    {
        // 开始播放，先接受后清理
        if (GetStatus(playId) == PlayStatus.PLAYING || GetStatus(playId) == PlayStatus.PAUSED)
        {
            var kDirector = GetPlayDirector(playId);
            if (kDirector != null)
            {
                kDirector.Stop();
                OnFinish(playId);

                return ErrorCode.SUCCESS;
            }

            return ErrorCode.NOT_READY;
        }
        // 加载了但是没有开始播放，直接清理
        else if (GetStatus(playId) == PlayStatus.LOADED || GetStatus(playId) == PlayStatus.INSTANTIATED)
        {
            OnFinish(playId);
        }

        return ErrorCode.STATUS_ERROR;
    }

    // TODO
    public ErrorCode Seek(uint playId)
    {
        return ErrorCode.SUCCESS;
    }

    // TODO
    public ErrorCode Step(uint playId)
    {
        return ErrorCode.SUCCESS;
    }
}
