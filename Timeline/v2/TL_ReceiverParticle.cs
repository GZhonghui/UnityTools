using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TL_ReceiverParticle : MonoBehaviour, INotificationReceiver
{
    public void OnNotify(Playable origin, INotification notification, object context)
    {
        // 只处理Wwise事件，是一个3D声音
        if (notification.GetType() == typeof(TL_SignalParticle))
        {
            var kSignal = notification as TL_SignalParticle;
        }
    }
}
