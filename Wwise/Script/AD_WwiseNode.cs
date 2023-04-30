using System;
using UnityEngine;

public class AD_WwiseNode : MonoBehaviour
{
    public void PostEvent2D(uint eventId, int? delayPost = null, int? Duration = null, int? stopTransition = null, Action Callback = null)
    {
        AD_WwiseManager.Instance.PostEvent(eventId, null, delayPost, Duration, stopTransition, false, true, Callback);
    }

    // Not Safe via String
    public void PostEvent2D(string eventName, int? delayPost = null, int? Duration = null, int? stopTransition = null, Action Callback = null)
    {
        AD_WwiseManager.Instance.PostEvent(eventName, null, delayPost, Duration, stopTransition, false, true, Callback);
    }

    public void StopEvent2D(uint eventId, int stopTransition = 0)
    {
        AD_WwiseManager.Instance.StopEvent(eventId, null, stopTransition);
    }

    public void StopEvent2D(string eventName, int stopTransition = 0)
    {
        AD_WwiseManager.Instance.StopEvent(eventName, null, stopTransition);
    }

    public void StopAllEvent2D()
    {
        AD_WwiseManager.Instance.StopAllEventOnGo(null);
    }

    public void PostEvent3D(uint eventId, GameObject Go, int? delayPost = null, int? Duration = null, int? stopTransition = null, Action Callback = null)
    {
        AD_WwiseManager.Instance.PostEvent(eventId, Go, delayPost, Duration, stopTransition, false, true, Callback);
    }

    public void PostEvevt3D(string eventName, GameObject Go, int? delayPost = null, int? Duration = null, int? stopTransition = null, Action Callback = null)
    {
        AD_WwiseManager.Instance.PostEvent(eventName, Go, delayPost, Duration, stopTransition, false, true, Callback);
    }

    public void StopEvent3D(uint eventId, GameObject Go, int stopTransition = 0)
    {
        AD_WwiseManager.Instance.StopEvent(eventId, Go, stopTransition);
    }

    public void StopEvent3D(string eventName, GameObject Go, int stopTransition = 0)
    {
        AD_WwiseManager.Instance.StopEvent(eventName, Go, stopTransition);
    }

    public void StopAllEvent3D(GameObject Go)
    {
        AD_WwiseManager.Instance.StopAllEventOnGo(Go);
    }

    public void StopAllEvent()
    {
        AD_WwiseManager.Instance.StopAll();
    }

    public void SetState(uint stateGroup, uint stateTarget)
    {
        AD_WwiseManager.Instance.SetState(stateGroup, stateTarget);
    }

    public void SetSwitch(uint switchGroup, uint switchTarget, GameObject Go = null)
    {
        AD_WwiseManager.Instance.SetSwitch(switchGroup, switchTarget, Go);
    }

    public void SetSwitch(string switchGroup, string switchTarget, GameObject Go = null)
    {
        AD_WwiseManager.Instance.SetSwitch(switchGroup, switchTarget, Go);
    }

    public void SetRtpc(uint rtpcId, float rtpcValue)
    {
        AD_WwiseManager.Instance.SetRtpc(rtpcId, rtpcValue);
    }

    public void SetLanguage(string languageName)
    {
        AD_WwiseManager.Instance.SetLanguage(languageName);
    }

    public void LoadBank(string bankName)
    {
        AD_WwiseManager.Instance.LoadBank(bankName);
    }

    public void UnloadBank(string bankName)
    {
        AD_WwiseManager.Instance.UnloadBank(bankName);
    }

    public void RegisterGameObject(GameObject Go)
    {
        if (Go == null) return;
        AkSoundEngine.RegisterGameObj(Go);
    }

    // TODO When to Unregister
    public void UnregisterGameObject(GameObject Go)
    {
        if (Go == null) return;
        // AkSoundEngine.UnregisterGameObj(Go);
    }

    public void LoadMapAmbient(string mapName = null)
    {
        AD_WwiseManager.Instance.LoadMapAmbient(mapName);
    }
}
