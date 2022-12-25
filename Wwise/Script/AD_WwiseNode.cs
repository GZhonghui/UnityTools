using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AD_WwiseNode : MonoBehaviour
{
    public void PostEvent2D(uint eventId, int? Duration = null, int? stopTransition = null, Action Callback = null)
    {
        AD_WwiseManager.Instance.PostEvent(eventId, null, Duration, stopTransition, false, true, Callback);
    }

    // Not Safe via String
    public void PostEvent2D(string eventName, int? Duration = null, int? stopTransition = null, Action Callback = null)
    {
        AD_WwiseManager.Instance.PostEvent(eventName, null, Duration, stopTransition, false, true, Callback);
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

    public void PostEvent3D(uint eventId, GameObject Go, int? Duration = null, int? stopTransition = null, Action Callback = null)
    {
        AD_WwiseManager.Instance.PostEvent(eventId, Go, Duration, stopTransition, false, true, Callback);
    }

    public void PostEvevt3D(string eventName, GameObject Go, int? Duration = null, int? stopTransition = null, Action Callback = null)
    {
        AD_WwiseManager.Instance.PostEvent(eventName, Go, Duration, stopTransition, false, true, Callback);
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

    public void SetRtpc(uint rtpcId, float rtpcValue)
    {
        AD_WwiseManager.Instance.SetRtpc(rtpcId, rtpcValue);
    }

    public void SetLanguage(string languageName)
    {
        AD_WwiseManager.Instance.SetLanguage(languageName);
    }
}
