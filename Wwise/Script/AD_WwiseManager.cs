using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using AK.Wwise.Unity.WwiseAddressables;

public class AD_WwiseManager
{
    public enum AD_WwiseCode
    {
        SUCCESS = 0,
        DELAY,
        QUEUE_FULL,
        ENGINE_FAILED,
        BAD_NAME,
        BAD_ID,
        CANT_LOAD_ASSET,
        CANT_FIND_CAMERA,
        LOAD_INIT_BANK,
    }

    private bool m_bLog = true;
    private int m_iQueueMaxSize = 128;

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

    private void Init()
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
            m_BankId = bankId;
            m_BankName = null; // Don't Use
            ConstructCommon(Load, ignoreRefCount);
        }

        public LoadBankCall(bool Load, string bankName, bool ignoreRefCount = false)
        {
            m_BankId = null; // Don't Use
            m_BankName = bankName;
            ConstructCommon(Load, ignoreRefCount);
        }

        private void ConstructCommon(bool Load, bool ignoreRefCount)
        {
            m_Load = Load;
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
        private bool m_NeedSeek;
        private bool m_NeedCache;
        private int? m_Duration;
        private int? m_StopTransition;
        private AkMIDIPostArray m_MidiArray;
        private Action m_EventDoneCallback;
        private bool m_Stop;
        private bool m_AutoExecute; // Only Count while Stop == true

        public PostEventCall(
            uint eventId, GameObject Go, bool needSeek = false, bool needCache = true,
            int? Duration = null, int? StopTransition = null,
            AkMIDIPostArray midiArray = null, Action eventDoneCallback = null,
            bool Stop = false, bool autoExecute = false
        )
        {
            m_EventId = eventId;
            m_EventName = null;
            ConstructCommon(Go, needSeek, needCache, Duration, StopTransition, midiArray, eventDoneCallback, Stop, autoExecute);
        }

        public PostEventCall(
            string eventName, GameObject Go, bool needSeek = false, bool needCache = true,
            int? Duration = null, int? StopTransition = null,
            AkMIDIPostArray midiArray = null, Action eventDoneCallback = null,
            bool Stop = false, bool autoExecute = false
        )
        {
            m_EventId = null;
            m_EventName = eventName;
            ConstructCommon(Go, needSeek, needCache, Duration, StopTransition, midiArray, eventDoneCallback, Stop, autoExecute);
        }

        private void ConstructCommon(
            GameObject Go, bool needSeek, bool needCache,
            int? Duration, int? StopTransition,
            AkMIDIPostArray midiArray, Action eventDoneCallback,
            bool Stop, bool autoExecute
        )
        {
            m_Go = Go;
            m_NeedSeek = needSeek;
            m_NeedCache = needCache;
            m_Duration = Duration;
            m_StopTransition = StopTransition;
            m_MidiArray = midiArray;
            m_EventDoneCallback = eventDoneCallback;
            m_Stop = Stop;
            m_AutoExecute = autoExecute;
        }

        public void Call(AD_WwiseManager Manager)
        {
            // Stop
            if (m_Stop)
            {
                // Stop Event
                if (m_MidiArray == null)
                {
                    if (m_EventId.HasValue) Manager.StopEvent(m_EventId.Value, m_Go, m_StopTransition, m_AutoExecute);
                    else Manager.StopEvent(m_EventName, m_Go, m_StopTransition, m_AutoExecute);
                }
            }
            // Post
            else
            {
                // Post Event
                if (m_MidiArray == null)
                {
                    if (m_EventId.HasValue) Manager.PostEvent(m_EventId.Value, m_Go, m_Duration, m_StopTransition, m_NeedSeek, m_NeedCache, m_EventDoneCallback);
                    else Manager.PostEvent(m_EventName, m_Go, m_Duration, m_StopTransition, m_NeedSeek, m_NeedCache, m_EventDoneCallback);
                }
                // Post Midi
                else
                {
                    if (m_EventId.HasValue) Manager.PostEventMidi(m_EventId.Value, m_MidiArray, m_Go);
                    else Manager.PostEventMidi(m_EventName, m_MidiArray, m_Go);
                }
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

    public static string GetLuaIdTableSourceFilePath()
    {
        return System.IO.Path.Combine(UnityEngine.Application.dataPath, "ArtRes/Bundle/Audio/Wwise/Define/Lua_AD_WwiseDefine.lua");
    }

    public static string GetLuaIdTableTargetFilePath()
    {
        return System.IO.Path.Combine(UnityEngine.Application.dataPath, "Script/LuaScript/Audio/Lua_AD_WwiseDefine.lua");
    }

    public static string GetBanksPath()
    {
        return "Assets/ArtRes/Bundle/Audio/Wwise/Banks";
    }

    // Init in Game Main
    public int InitWwise(GameObject Attach = null, bool createWwise = true)
    {
        if (m_bInitedWwise) return 1;

        m_bInitedWwise = true;

        if (m_bLog) Debug.Log("Wwise Initing");

        // Important
        AK.WwiseDefine.LoadDefine();
        if (m_bLog) Debug.Log("Wwise Define Load Done");

        AkAddressableBankManager.Instance.useCarefullyRegOnBankLoaded(OnBankLoaded);
        AkAddressableBankManager.Instance.useCarefullyRegOnBankUnloaded(OnBankUnloaded);

        if (createWwise)
        {
            if (Attach != null)
            {
                m_kGlobal = Attach;
            }
            else
            {
                m_kGlobal = new GameObject("Wwise");
            }
        }

        if (m_kGlobal)
        {

#if Art_Editor
#else
            GameObject.DontDestroyOnLoad(m_kGlobal);
#endif

            m_kGlobal.SetActive(false);

            if (m_kGlobal.GetComponent<AkInitializer>() == null)
            {
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
            }

            if (m_kGlobal.GetComponent<InitBankHolder>() == null)
            {
                InitBankHolder initHolder = m_kGlobal.AddComponent<InitBankHolder>();

                WwiseAddressableSoundBank initBank =
                    Addressables.LoadAssetAsync<WwiseAddressableSoundBank>(
                    GetAddressableBankPath("Init")
                ).WaitForCompletion();

                if (initHolder != null && initBank != null)
                {
                    initHolder.InitBank = initBank;
                }
            }

            if (m_kGlobal.GetComponent<AkTerminator>() == null)
            {
                m_kGlobal.AddComponent<AkTerminator>();
            }

            m_kGlobal.SetActive(true);
        }

        // Find Main Camera
#if Art_Editor
        m_MainCamera = GameObject.Find("Main Camera");
#else
        m_MainCamera = LC_WorldManager.Instance.m_kMainCamera.gameObject;
#endif

        if (m_MainCamera)
        {
            AkSoundEngine.RegisterGameObj(m_MainCamera);

            if (m_MainCamera.GetComponent<AD_WwiseListener>() == null)
            {
                m_MainCamera.AddComponent<AD_WwiseListener>();
            }

            if (m_MainCamera.GetComponent<AkAudioListener>() == null)
            {
                m_MainCamera.AddComponent<AkAudioListener>();
            }
        }
        else
        {
            // if (m_bLog)
            Debug.LogError("Wwise Cant Create Audio Listener");
        }

        return 0;
    }

    // Uninit
    public int UninitWise()
    {
        if (!m_bInitedWwise) return 1;

        m_bInitedWwise = false;

        if (m_bLog) Debug.Log("Wwise Uniniting");

        m_MainCamera = null;

        if(m_kGlobal)
        {
            // GameObject.DestroyImmediate(m_kGlobal, true);
            m_kGlobal = null;
        }

        m_LoadBankCallQueue.Clear();
        m_PostEventCallQueue.Clear();
        m_SetStateCallQueue.Clear();

        // Clear Banks Data
        m_LoadedBankRef.Clear();

        // So
        m_UsedEvents.Clear();

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
    private AD_WwiseCode PushQueueWithLimit<T>(T pushObject, Queue<T> targetQueue)
    {
        if (targetQueue.Count < m_iQueueMaxSize)
        {
            targetQueue.Enqueue(pushObject);
            return AD_WwiseCode.SUCCESS;
        }
        else
        {
            if (m_bLog) Debug.Log("Wwise Too Much Calls in Delay Queue");
            return AD_WwiseCode.QUEUE_FULL;
        }
    }

    // Not Recommend
    public AD_WwiseCode LoadBank(string bankName)
    {
        if (bankName == null) return AD_WwiseCode.BAD_NAME;

        if (InitBankLoaded())
        {
            if (AK.WwiseDefine.dataRevBanks.ContainsKey(bankName))
            {
                uint bankId = AK.WwiseDefine.dataRevBanks[bankName];
                return LoadBank(bankId);
            }
            return AD_WwiseCode.BAD_NAME;
        }

        PushQueueWithLimit<LoadBankCall>(new LoadBankCall(true, bankName), m_LoadBankCallQueue);

        return AD_WwiseCode.DELAY;
    }

    // Don't Load Init Bank Manually
    public AD_WwiseCode LoadBank(uint bankId)
    {
        // Real Load
        if (InitBankLoaded())
        {
            if (AK.WwiseDefine.dataBanks.ContainsKey(bankId))
            {
                if (AK.WwiseDefine.dataBanks[bankId].Name == "Init")
                {
                    // Don't Load Init Bank Manually
                    return AD_WwiseCode.LOAD_INIT_BANK;
                }
            }

            TryLoadAddressableBankRef(bankId);

            // Real Load Data
            if (m_LoadedBankRef.ContainsKey(bankId))
            {
                AkAddressableBankManager.Instance.LoadBank(m_LoadedBankRef[bankId]);
                return AD_WwiseCode.SUCCESS;
            }

            return AD_WwiseCode.CANT_LOAD_ASSET;
        }

        PushQueueWithLimit<LoadBankCall>(new LoadBankCall(true, bankId), m_LoadBankCallQueue);

        return AD_WwiseCode.DELAY;
    }

    // Not Recommend
    public AD_WwiseCode UnloadBank(string bankName, bool ignoreRefCount = false)
    {
        if (bankName == null) return AD_WwiseCode.BAD_NAME;

        if (InitBankLoaded())
        {
            if (AK.WwiseDefine.dataRevBanks.ContainsKey(bankName))
            {
                uint bankId = AK.WwiseDefine.dataRevBanks[bankName];
                return UnloadBank(bankId, ignoreRefCount);
            }

            return AD_WwiseCode.BAD_NAME;
        }

        PushQueueWithLimit<LoadBankCall>(new LoadBankCall(false, bankName, ignoreRefCount), m_LoadBankCallQueue);

        return AD_WwiseCode.DELAY;
    }

    // Don't Unload Init Bank Manually
    public AD_WwiseCode UnloadBank(uint bankId, bool ignoreRefCount = false)
    {
        if (InitBankLoaded())
        {
            TryLoadAddressableBankRef(bankId);

            if (m_LoadedBankRef.ContainsKey(bankId))
            {
                AkAddressableBankManager.Instance.UnloadBank(m_LoadedBankRef[bankId], ignoreRefCount);
                return AD_WwiseCode.SUCCESS;
            }

            return AD_WwiseCode.CANT_LOAD_ASSET;
        }

        PushQueueWithLimit<LoadBankCall>(new LoadBankCall(false, bankId, ignoreRefCount), m_LoadBankCallQueue);

        return AD_WwiseCode.DELAY;
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
    public AD_WwiseCode PostEvent(string eventName, GameObject Go = null, int? Duration = null, int? stopTransition = null, bool needSeek = false, bool needCache = true, Action Callback = null)
    {
        if (eventName == null) return AD_WwiseCode.BAD_NAME;

        if (InitBankLoaded())
        {
            if (AK.WwiseDefine.dataRevEvents.ContainsKey(eventName))
            {
                uint eventId = AK.WwiseDefine.dataRevEvents[eventName];
                return PostEvent(eventId, Go, Duration, stopTransition, needSeek, needCache, Callback);
            }
            else
            {
                if (m_bLog) Debug.Log("Wwise Wrong Event Name: " + eventName);
                return AD_WwiseCode.BAD_NAME;
            }
        }

        if (m_bLog) Debug.Log("Wwise Event Will be Delayed: " + eventName);
        PushQueueWithLimit<PostEventCall>(
            new PostEventCall(eventName, Go, needSeek, needCache, Duration, stopTransition, eventDoneCallback: Callback), m_PostEventCallQueue
        );

        return AD_WwiseCode.DELAY;
    }

    // Use Carefully with Cache Event! You don't need to cache event at most time
    public AD_WwiseCode PostEvent(uint eventId, GameObject Go = null, int? Duration = null, int? stopTransition = null, bool needSeek = false, bool needCache = true, Action Callback = null)
    {
        if (InitBankLoaded())
        {
            uint bankId = 0;

            AK.Wwise.Event Event = TryGetEvent(eventId, out bankId);

            if (Event == null)
            {
                if (m_bLog) Debug.Log("Wwise Wrong Event Id: " + eventId.ToString());
                return AD_WwiseCode.BAD_ID;
            }

            // Play 2D Sound, Default Way
            if (Go == null) Go = m_MainCamera;

            double? callTime = Time.timeAsDouble;
            // if (m_bLog) Debug.Log("Wwise Call Event at: " + callTime.ToString());

            // Load Bank Before Post Event
            LoadBank(bankId);

            if (Go != null)
            {
                Debug.Log($"Wwise Play: {AK.WwiseDefine.dataEvents[eventId].Name}");

                AkSoundEngine.RegisterGameObj(Go);
                uint playingId = Event.Post(Go, (uint)AkCallbackType.AK_EndOfEvent, (in_cookie, in_type, in_info) =>
                {
                    // Unload After Event Done
                    UnloadBank(bankId);
                    Callback?.Invoke(); // Lua
                }, needSeek: needSeek, needCache: true, callTime: callTime); // HARD CODE: Set needCache to True to Mirror Load & Unload

                if (Duration.HasValue)
                {
                    // Thread Pool
                    // System.Threading.Tasks.Task.Run(() => StopEventWithDelay(Duration.Value, eventId, Go, stopTransition));

                    // Main Thread
                    StopEventWithDelay(Duration.Value, eventId, Go, stopTransition);
                }

                return AD_WwiseCode.SUCCESS;
            }

            return AD_WwiseCode.CANT_FIND_CAMERA;
        }

        if (m_bLog) Debug.Log("Wwise Event Will be Delayed: " + eventId.ToString());
        PushQueueWithLimit<PostEventCall>(
            new PostEventCall(eventId, Go, needSeek, needCache, Duration, stopTransition, eventDoneCallback: Callback), m_PostEventCallQueue
        );

        return AD_WwiseCode.DELAY;
    }

    // Helper
    // TODO
    private async void StopEventWithDelay(int Delay, uint eventId, GameObject Go = null, int? stopTransition = null)
    {
        await System.Threading.Tasks.Task.Delay(UT_Math.ABS(Delay));
        AD_WwiseManager.Instance.StopEvent(eventId, Go, stopTransition, Delay > 0); // Auto Stop
    }

    // TODO
    public AD_WwiseCode StopAllEventOnGo(GameObject Go = null)
    {
        // Stop All 2D Sound
        if (Go == null) Go = m_MainCamera;

        if (Go == null) return AD_WwiseCode.CANT_FIND_CAMERA;

        // Post a Stop All Event
        Debug.Log("Do not use StopAllEventOnGo for now~");

        return AD_WwiseCode.SUCCESS;
    }

    // Stop by eventName
    public AD_WwiseCode StopEvent(string eventName, GameObject Go = null, int? stopTransition = null, bool autoExecute = false)
    {
        if (eventName == null) return AD_WwiseCode.BAD_NAME;

        if (InitBankLoaded())
        {
            if (AK.WwiseDefine.dataRevEvents.ContainsKey(eventName))
            {
                uint eventId = AK.WwiseDefine.dataRevEvents[eventName];

                return StopEvent(eventId, Go, stopTransition, autoExecute);
            }

            return AD_WwiseCode.BAD_NAME;
        }

        PushQueueWithLimit<PostEventCall>(
            new PostEventCall(eventName, Go, StopTransition: stopTransition, Stop: true, autoExecute: autoExecute), m_PostEventCallQueue
        );
        return AD_WwiseCode.DELAY;
    }

    // Stop by eventId
    public AD_WwiseCode StopEvent(uint eventId, GameObject Go = null, int? stopTransition = null, bool autoExecute = false)
    {
        if (InitBankLoaded())
        {
            // Debug.Log("Wwise Stop Event at Thread: " + System.Threading.Thread.CurrentThread.ManagedThreadId);

            AK.Wwise.Event Event = TryGetEvent(eventId, out uint bankId);

            if (Event == null) return AD_WwiseCode.CANT_LOAD_ASSET;

            if (Go == null) Go = m_MainCamera;

            if (Go != null)
            {
                AkSoundEngine.RegisterGameObj(Go);
                Event.Stop(Go, stopTransition.HasValue ? stopTransition.Value : 0);
                if (m_bLog) Debug.Log($"Stop Event: {eventId} at Go: {Go.GetInstanceID()}");

                // HARD CODE
                // Auto Post a End Event if Auto Stop a Loop Event
                if (autoExecute)
                {
                    const string strLoop = "_Loop";
                    const string strEnd = "_End";

                    string eventName = AK.WwiseDefine.dataEvents[eventId].Name;
                    if (eventName.EndsWith(strLoop))
                    {
                        string endEventName = eventName.Substring(0, eventName.Length - strLoop.Length) + strEnd;
                        PostEvent(endEventName, Go); // Try Post
                    }
                }

                return AD_WwiseCode.SUCCESS;
            }

            return AD_WwiseCode.CANT_FIND_CAMERA;
        }

        PushQueueWithLimit<PostEventCall>(
            new PostEventCall(eventId, Go, StopTransition: stopTransition, Stop: true, autoExecute: autoExecute), m_PostEventCallQueue
        );
        return AD_WwiseCode.DELAY;
    }

    // Reset All Event
    public AD_WwiseCode StopAll()
    {
        m_PostEventCallQueue.Clear();
        AkSoundEngine.StopAll();
        return AD_WwiseCode.SUCCESS;
    }

    // Not Recommend
    public AD_WwiseCode PostEventMidi(string eventName, AkMIDIPostArray midiArray, GameObject Go = null)
    {
        if (eventName == null) return AD_WwiseCode.BAD_NAME;

        if (InitBankLoaded())
        {
            if (AK.WwiseDefine.dataRevEvents.ContainsKey(eventName))
            {
                uint eventId = AK.WwiseDefine.dataRevEvents[eventName];
                return PostEventMidi(eventId, midiArray, Go);
            }
            return AD_WwiseCode.BAD_NAME;
        }

        PushQueueWithLimit<PostEventCall>(new PostEventCall(eventName, Go, midiArray: midiArray), m_PostEventCallQueue);

        return AD_WwiseCode.DELAY;
    }

    // Midi
    public AD_WwiseCode PostEventMidi(uint eventId, AkMIDIPostArray midiArray, GameObject Go = null)
    {
        if (InitBankLoaded())
        {
            uint bankId = 0;

            AK.Wwise.Event Event = TryGetEvent(eventId, out bankId);

            if (Event == null) return AD_WwiseCode.BAD_ID;

            if (Go == null) Go = m_MainCamera;

            LoadBank(bankId);

            Event.PostMIDI(Go, midiArray);

            UnloadBank(bankId); // Work?

            return AD_WwiseCode.SUCCESS;
        }

        PushQueueWithLimit<PostEventCall>(new PostEventCall(eventId, Go, midiArray: midiArray), m_PostEventCallQueue);

        return AD_WwiseCode.DELAY;
    }

    // Delay Queue
    public AD_WwiseCode SetState(uint stateGroup, uint stateTarget)
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
                return AD_WwiseCode.ENGINE_FAILED;
            }

            return AD_WwiseCode.SUCCESS;
        }

        PushQueueWithLimit<SetStateCall>(new SetStateCall(stateGroup, stateTarget), m_SetStateCallQueue);

        return AD_WwiseCode.DELAY;
    }

    // No Delay Queue
    public AD_WwiseCode SetSwitch(uint switchGroup, uint switchTarget, GameObject Go = null)
    {
        if (Go == null) Go = m_MainCamera;

        AKRESULT Res = AkSoundEngine.SetSwitch(switchGroup, switchTarget, Go);

        if (Res != AKRESULT.AK_Success)
        {
            if (m_bLog) Debug.Log("Wwise Set Switch Failed, " + Res.ToString());
            return AD_WwiseCode.ENGINE_FAILED;
        }

        return AD_WwiseCode.SUCCESS;
    }

    // No Delay Queue
    public AD_WwiseCode SetRtpc(uint rtpcId, float rtpcValue)
    {
        AKRESULT Res = AkSoundEngine.SetRTPCValue(rtpcId, rtpcValue);

        if (Res != AKRESULT.AK_Success)
        {
            if (m_bLog) Debug.Log("Wwise Set Rtpc Failed, " + Res.ToString());
            return AD_WwiseCode.ENGINE_FAILED;
        }

        return AD_WwiseCode.SUCCESS;
    }

    // Reload Banks
    public AD_WwiseCode SetLanguage(string languageName)
    {
        if (languageName == null) return AD_WwiseCode.BAD_NAME;

        // TODO
        AkAddressableBankManager.Instance.SetLanguageAndReloadLocalizedBanks(languageName);
        return AD_WwiseCode.SUCCESS;
    }
}
