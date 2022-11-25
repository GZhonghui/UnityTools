using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AD_WwiseNode : MonoBehaviour
{
    public void PostEvent2D(uint eventId, Action Callbank = null)
    {
        AD_WwiseManager.Instance.PostEvent(eventId, null, false, Callbank);
    }

    // Not Safe via String
    public void PostEvent2D(string eventName, Action Callbank = null)
    {
        AD_WwiseManager.Instance.PostEvent(eventName, null, false, Callbank);
    }

    public void PostEvent3D(uint eventId, GameObject Go, Action Callbank = null)
    {
        AD_WwiseManager.Instance.PostEvent(eventId, Go, false, Callbank);
    }

    public void PostEvevt3D(string eventName, GameObject Go, Action Callbank = null)
    {
        AD_WwiseManager.Instance.PostEvent(eventName, Go, false, Callbank);
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
