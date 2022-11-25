using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using AK.Wwise.Unity.WwiseAddressables;

public class AD_WwiseManager
{
    private bool m_bLog = true;
    private int m_iQueueMaxSize = 64;

    #region Signleton
    protected static AD_WwiseManager _instance = new AD_WwiseManager();

    private bool m_bInitialized = false;

    public static AD_WwiseManager Instance
    {
        get
        {
            _instance.Init();
            return _instance;
        }
    }

    public void Init()
    {
        if (m_bInitialized) return;

        m_bInitialized = true;
    }
    #endregion

    private bool m_bInitedWwise = false;

    // Initer
    private GameObject m_kGlobal;

    // Default Listener
    private GameObject m_MainCamera;

    // Cache
    private Dictionary<uint, WwiseAddressableSoundBank> m_LoadedBankRef = new Dictionary<uint, WwiseAddressableSoundBank>();

    private Dictionary<uint, AK.Wwise.Event> m_UsedEvents = new Dictionary<uint, AK.Wwise.Event>();

    // Delay Calls
    private class LoadBankCall
    {
        private bool m_Load;
        private uint? m_BankId;
        private string m_BankName;
        private bool m_IgnoreRefCount; // Only Count while Unloading Bank

        public LoadBankCall(bool Load, uint bankId, bool ignoreRefCount = false)
        {
            m_Load = Load;
            m_BankId = bankId;
            m_BankName = null; // Don't Use
            m_IgnoreRefCount = ignoreRefCount;
        }

        public LoadBankCall(bool Load, string bankName, bool ignoreRefCount = false)
        {
            m_Load = Load;
            m_BankId = null; // Don't Use
            m_BankName = bankName;
            m_IgnoreRefCount = ignoreRefCount;
        }

        public void Call(AD_WwiseManager Manager)
        {
            if (m_Load)
            {
                if (m_BankId.HasValue) Manager.LoadBank(m_BankId.Value);
                else Manager.LoadBank(m_BankName);
            }
            else
            {
                if (m_BankId.HasValue) Manager.UnloadBank(m_BankId.Value, m_IgnoreRefCount);
                else Manager.UnloadBank(m_BankName, m_IgnoreRefCount);
            }
        }
    }

    private class PostEventCall
    {
        private uint? m_EventId;
        private string m_EventName;
        private GameObject m_Go;
        private bool m_NeedCache;
        private AkMIDIPostArray m_MidiArray;
        private Action m_EventDoneCallback;

        public PostEventCall(
            uint eventId, GameObject Go, bool needCache = false,
            AkMIDIPostArray midiArray = null, Action eventDoneCallback = null
        )
        {
            m_EventId = eventId;
            m_EventName = null;
            m_Go = Go;
            m_NeedCache = needCache;
            m_MidiArray = midiArray;
            m_EventDoneCallback = eventDoneCallback;
        }

        public PostEventCall(
            string eventName, GameObject Go, bool needCache = false,
            AkMIDIPostArray midiArray = null, Action eventDoneCallback = null
        )
        {
            m_EventId = null;
            m_EventName = eventName;
            m_Go = Go;
            m_NeedCache = needCache;
            m_MidiArray = midiArray;
            m_EventDoneCallback = eventDoneCallback;
        }

        public void Call(AD_WwiseManager Manager)
        {
            // Post Event
            if (m_MidiArray == null)
            {
                if (m_EventId.HasValue) Manager.PostEvent(m_EventId.Value, m_Go, m_NeedCache, m_EventDoneCallback);
                else Manager.PostEvent(m_EventName, m_Go, m_NeedCache, m_EventDoneCallback);
            }
            // Post Midi
            else
            {
                if (m_EventId.HasValue) Manager.PostEventMidi(m_EventId.Value, m_MidiArray, m_Go);
                else Manager.PostEventMidi(m_EventName, m_MidiArray, m_Go);
            }
        }
    }

    private class SetStateCall
    {
        private uint m_Group;
        private uint m_Target;

        public SetStateCall(uint Group, uint Target)
        {
            m_Group = Group;
            m_Target = Target;
        }

        public void Call(AD_WwiseManager Manager)
        {
            Manager.SetState(m_Group, m_Target);
        }
    }

    // Until Init Bank Loaded
    private Queue<LoadBankCall> m_LoadBankCallQueue = new Queue<LoadBankCall>();
    private Queue<PostEventCall> m_PostEventCallQueue = new Queue<PostEventCall>();
    private Queue<SetStateCall> m_SetStateCallQueue = new Queue<SetStateCall>();

    public static string GetAddressableBankPath(string bankName)
    {
        return "Assets/ArtRes/Bundle/Audio/Wwise/Banks/" + bankName + ".asset";
    }

    public static string GetWwiseIdJsonPath()
    {
        return "Assets/ArtRes/Bundle/Audio/Wwise/Define/WwiseDefine.json";
    }

    public static string GetAddressableInitializationSettingsPath()
    {
        return "Assets/Wwise/ScriptableObjects/AkWwiseInitializationSettings.asset";
    }

    // Init in Game Main
    public int InitWwise(GameObject Attach = null)
    {
        if (m_bInitedWwise) return 1;

        m_bInitedWwise = true;

        if (m_bLog) Debug.Log("Wwise Initing");

        // Important
        AK.WwiseDefine.LoadDefine();
        if (m_bLog) Debug.Log("Wwise Define Load Done");

        AkAddressableBankManager.Instance.useCarefullyRegOnBankLoaded(OnBankLoaded);
        AkAddressableBankManager.Instance.useCarefullyRegOnBankUnloaded(OnBankUnloaded);

        if (Attach != null)
        {
            m_kGlobal = Attach;
        }
        else
        {
            m_kGlobal = new GameObject("Wwise");
        }

        GameObject.DontDestroyOnLoad(m_kGlobal);
        m_kGlobal.SetActive(false);

        AkInitializer akInitializer = m_kGlobal.AddComponent<AkInitializer>();

        AkWwiseAddressablesInitializationSettings akInitializationSettings =
            Addressables.LoadAssetAsync<AkWwiseInitializationSettings>(
            GetAddressableInitializationSettingsPath()
        ).WaitForCompletion() as AkWwiseAddressablesInitializationSettings;

        if (akInitializer != null && akInitializationSettings != null)
        {
            if (m_bLog) Debug.Log("Wwise Load Init Setting Done");
            akInitializer.InitializationSettings = akInitializationSettings;
        }

        InitBankHolder initHolder = m_kGlobal.AddComponent<InitBankHolder>();

        WwiseAddressableSoundBank initBank =
            Addressables.LoadAssetAsync<WwiseAddressableSoundBank>(
            GetAddressableBankPath("Init")
        ).WaitForCompletion();

        if (initHolder != null && initBank != null)
        {
            initHolder.InitBank = initBank;
        }

        m_kGlobal.AddComponent<AkTerminator>();

        m_kGlobal.SetActive(true);

        // Find Main Camera
        //m_MainCamera = GameObject.Find("Main Camera");
        m_MainCamera = LC_WorldManager.Instance.m_kMainCamera.gameObject;
        
        if (m_MainCamera)
        {
            AkSoundEngine.RegisterGameObj(m_MainCamera);

            m_MainCamera.AddComponent<AD_WwiseListener>();
            m_MainCamera.AddComponent<AkAudioListener>();
        }
        else
        {
            // if (m_bLog)
            Debug.LogError("Wwise Cant Create Audio Listener");
        }

        return 0;
    }

    // Check Init Bank State
    public bool InitBankLoaded()
    {
        return AkAddressableBankManager.Instance.useCarefullyInitBankLoaded();
    }

    // Invoke when load done
    private void OnBankLoaded(uint bankId)
    {
        // Unsafe
        // if (bankId == AK.BANKS.INIT)

        uint initBankId = 0;
        // Init Bank
        if (AK.WwiseDefine.dataRevBanks.TryGetValue("Init", out initBankId) && bankId == initBankId)
        {
            OnInitBankLoaded();
        }
        else
        {
            if (m_bLog) Debug.Log("Wwise Bank Loaded: " + bankId.ToString());
        }
    }

    // Invoke when init bank load done
    private void OnInitBankLoaded()
    {
        if (m_bLog) Debug.Log("Wwise Manager: Init Bank Loaded");

        // Banks
        while (m_LoadBankCallQueue.Count > 0)
        {
            m_LoadBankCallQueue.Peek().Call(this);
            m_LoadBankCallQueue.Dequeue();
        }

        // Events
        while (m_PostEventCallQueue.Count > 0)
        {
            m_PostEventCallQueue.Peek().Call(this);
            m_PostEventCallQueue.Dequeue();
        }

        // States
        while (m_SetStateCallQueue.Count > 0)
        {
            m_SetStateCallQueue.Peek().Call(this);
            m_SetStateCallQueue.Dequeue();
        }
    }

    // Invoke when start unloading
    private void OnBankUnloaded(uint bankId)
    {
        uint initBankId = 0;
        // Init Bank
        if (AK.WwiseDefine.dataRevBanks.TryGetValue("Init", out initBankId) && bankId == initBankId)
        {
            if (m_bLog) Debug.Log("Wwise Init Bank Unloading");
        }
        else
        {
            TryReleaseAddressableBankRef(bankId);
        }
    }

    // Helper
    private void TryLoadAddressableBankRef(uint bankId)
    {
        if (!AK.WwiseDefine.dataBanks.ContainsKey(bankId))
        {
            if (m_bLog) Debug.Log("Wwise Trying Load Bank with a Error Id, " + bankId.ToString());
        }

        if (!m_LoadedBankRef.ContainsKey(bankId))
        {
            // Load
            string bankName = AK.WwiseDefine.dataBanks[bankId].Name;

            // Just a Ref
            /*
            WwiseAddressableSoundBank loadingBank =
                Addressables.LoadAssetAsync<WwiseAddressableSoundBank>(
                    GetAddressableBankPath(bankName)
                ).WaitForCompletion();
            */

            WwiseAddressableSoundBank loadingBank = UT_ResourceLoadManager.Instance.LoadUObject(
                GetAddressableBankPath(bankName)
            ) as WwiseAddressableSoundBank;

            if (loadingBank != null)
            {
                m_LoadedBankRef[bankId] = loadingBank;
            }
        }
    }

    // Helper
    private void TryReleaseAddressableBankRef(uint bankId)
    {
        if (m_LoadedBankRef.ContainsKey(bankId))
        {
            // Addressables.Release<WwiseAddressableSoundBank>(m_LoadedBankRef[bankId]);
            UT_ResourceLoadManager.Instance.UnLoadUOject(m_LoadedBankRef[bankId], false);

            m_LoadedBankRef.Remove(bankId);
        }
    }

    // Helper
    private void PushQueueWithLimit<T>(T pushObject, Queue<T> targetQueue)
    {
        if (targetQueue.Count < m_iQueueMaxSize)
        {
            targetQueue.Enqueue(pushObject);
        }
        else
        {
            if (m_bLog) Debug.Log("Wwise Too Much Calls in Delay Queue");
        }
    }

    // Not Recommend
    public int LoadBank(string bankName)
    {
        if (InitBankLoaded())
        {
            if (AK.WwiseDefine.dataRevBanks.ContainsKey(bankName))
            {
                uint bankId = AK.WwiseDefine.dataRevBanks[bankName];
                return LoadBank(bankId);
            }
            return 1;
        }

        PushQueueWithLimit<LoadBankCall>(new LoadBankCall(true, bankName), m_LoadBankCallQueue);

        return 2;
    }

    // Don't Load Init Bank Manually
    public int LoadBank(uint bankId)
    {
        if (InitBankLoaded())
        {
            TryLoadAddressableBankRef(bankId);

            // Real Load
            if (m_LoadedBankRef.ContainsKey(bankId))
            {
                AkAddressableBankManager.Instance.LoadBank(m_LoadedBankRef[bankId]);
            }

            return 0;
        }

        PushQueueWithLimit<LoadBankCall>(new LoadBankCall(true, bankId), m_LoadBankCallQueue);

        return 1;
    }

    // Not Recommend
    public int UnloadBank(string bankName, bool ignoreRefCount = false)
    {
        if (InitBankLoaded())
        {
            if (AK.WwiseDefine.dataRevBanks.ContainsKey(bankName))
            {
                uint bankId = AK.WwiseDefine.dataRevBanks[bankName];
                return UnloadBank(bankId, ignoreRefCount);
            }

            return 1;
        }

        PushQueueWithLimit<LoadBankCall>(new LoadBankCall(false, bankName, ignoreRefCount), m_LoadBankCallQueue);

        return 2;
    }

    // Don't Unload Init Bank Manually
    public int UnloadBank(uint bankId, bool ignoreRefCount = false)
    {
        if (InitBankLoaded())
        {
            TryLoadAddressableBankRef(bankId);

            if (m_LoadedBankRef.ContainsKey(bankId))
            {
                AkAddressableBankManager.Instance.UnloadBank(m_LoadedBankRef[bankId], ignoreRefCount);
            }

            return 0;
        }

        PushQueueWithLimit<LoadBankCall>(new LoadBankCall(false, bankId, ignoreRefCount), m_LoadBankCallQueue);

        return 1;
    }

    // Require Define
    private AK.Wwise.Event ConstructEvent(uint eventId)
    {
        AK.Wwise.Event triggeringEvent = new AK.Wwise.Event();
        triggeringEvent.ObjectReference = ScriptableObject.CreateInstance<WwiseEventReference>();

        string eventName = AK.WwiseDefine.dataEvents[eventId].Name;
        string eventGuid = AK.WwiseDefine.dataEvents[eventId].Guid;

        triggeringEvent.ObjectReference.useCarefullySetId(eventId);
        triggeringEvent.ObjectReference.useCarefullySetObjectName(eventName);
        triggeringEvent.ObjectReference.useCarefullySetGuid(eventGuid);

        return triggeringEvent;
    }

    // With Cache
    private AK.Wwise.Event TryGetEvent(uint eventId, out uint bankId)
    {
        if (!AK.WwiseDefine.dataEvents.ContainsKey(eventId))
        {
            bankId = 0;
            return null;
        }

        if (!m_UsedEvents.ContainsKey(eventId))
        {
            m_UsedEvents[eventId] = ConstructEvent(eventId);
        }

        bankId = AK.WwiseDefine.dataEvents[eventId].Bank;

        return m_UsedEvents[eventId];
    }

    // Not Recommend
    public int PostEvent(string eventName, GameObject Go = null, bool needCache = false, Action Callback = null)
    {
        if (InitBankLoaded())
        {
            if (AK.WwiseDefine.dataRevEvents.ContainsKey(eventName))
            {
                uint eventId = AK.WwiseDefine.dataRevEvents[eventName];
                return PostEvent(eventId, Go, needCache, Callback);
            }
            return 1;
        }

        PushQueueWithLimit<PostEventCall>(
            new PostEventCall(eventName, Go, needCache, eventDoneCallback: Callback), m_PostEventCallQueue
        );

        return 2;
    }

    // Use Carefully with Cache Event! You don't need to cache event at most time
    public int PostEvent(uint eventId, GameObject Go = null, bool needCache = false, Action Callback = null)
    {
        if (InitBankLoaded())
        {
            uint bankId = 0;

            AK.Wwise.Event Event = TryGetEvent(eventId, out bankId);

            if (Event == null) return 1;

            // Play 2D Sound, Default Way
            if (Go == null) Go = m_MainCamera;

            double callTime = Time.timeAsDouble;
            if (m_bLog) Debug.Log("Wwise Call Event at: " + callTime.ToString());

            // Load Bank Before Post Event
            LoadBank(bankId);

            uint playingId = Event.Post(Go, (uint)AkCallbackType.AK_EndOfEvent, (in_cookie, in_type, in_info) =>
            {
                // Unload After Event Done
                UnloadBank(bankId);
                Callback?.Invoke(); // Lua
            }, needCache: true, callTime: callTime); // Set needCache to True to Mirror Load & Unload

            return 0;
        }

        PushQueueWithLimit<PostEventCall>(
            new PostEventCall(eventId, Go, needCache, eventDoneCallback: Callback), m_PostEventCallQueue
        );

        return 1;
    }

    // Not Recommend
    public int PostEventMidi(string eventName, AkMIDIPostArray midiArray, GameObject Go = null)
    {
        if (InitBankLoaded())
        {
            if (AK.WwiseDefine.dataRevEvents.ContainsKey(eventName))
            {
                uint eventId = AK.WwiseDefine.dataRevEvents[eventName];
                return PostEventMidi(eventId, midiArray, Go);
            }
            return 1;
        }

        PushQueueWithLimit<PostEventCall>(new PostEventCall(eventName, Go, midiArray: midiArray), m_PostEventCallQueue);

        return 2;
    }

    // Midi
    public int PostEventMidi(uint eventId, AkMIDIPostArray midiArray, GameObject Go = null)
    {
        if (InitBankLoaded())
        {
            uint bankId = 0;

            AK.Wwise.Event Event = TryGetEvent(eventId, out bankId);

            if (Event == null) return 1;

            if (Go == null) Go = m_MainCamera;

            LoadBank(bankId);

            Event.PostMIDI(Go, midiArray);

            UnloadBank(bankId); // Work?

            return 0;
        }

        PushQueueWithLimit<PostEventCall>(new PostEventCall(eventId, Go, midiArray: midiArray), m_PostEventCallQueue);

        return 2;
    }

    // Delay Queue
    public int SetState(uint stateGroup, uint stateTarget)
    {
        // TODO: Inplace AkSoundEngine.SetState with AK.Wwise.State
        // AK.Wwise.State triggeringState = new AK.Wwise.State();
        // triggeringState.SetValue();

        if (m_bLog) Debug.Log("Wwise Set State: " + stateGroup.ToString() + "; " + stateTarget.ToString());

        if (InitBankLoaded())
        {
            AKRESULT Res = AkSoundEngine.SetState(stateGroup, stateTarget);

            if (Res != AKRESULT.AK_Success)
            {
                if (m_bLog) Debug.Log("Wwise Set State Failed, " + Res.ToString());
                return 1;
            }

            return 0;
        }

        PushQueueWithLimit<SetStateCall>(new SetStateCall(stateGroup, stateTarget), m_SetStateCallQueue);

        return 2;
    }

    // No Delay Queue
    public int SetSwitch(uint switchGroup, uint switchTarget, GameObject Go = null)
    {
        if (Go == null) Go = m_MainCamera;

        AKRESULT Res = AkSoundEngine.SetSwitch(switchGroup, switchTarget, Go);

        if (Res != AKRESULT.AK_Success)
        {
            if (m_bLog) Debug.Log("Wwise Set Switch Failed, " + Res.ToString());
            return 1;
        }

        return 0;
    }

    // No Delay Queue
    public int SetRtpc(uint rtpcId, float rtpcValue)
    {
        AKRESULT Res = AkSoundEngine.SetRTPCValue(rtpcId, rtpcValue);

        if (Res != AKRESULT.AK_Success)
        {
            if (m_bLog) Debug.Log("Wwise Set Rtpc Failed, " + Res.ToString());
            return 1;
        }

        return 0;
    }

    // Reload Banks
    public int SetLanguage(string languageName)
    {
        AkAddressableBankManager.Instance.SetLanguageAndReloadLocalizedBanks(languageName);
        return 0;
    }
}
