using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TL_ReceiverParticle : MonoBehaviour, INotificationReceiver
{
    public void OnNotify(Playable origin, INotification notification, object context)
    {
        // ֻ����Wwise�¼�����һ��3D����
        if (notification.GetType() == typeof(TL_SignalParticle))
        {
            var kSignal = notification as TL_SignalParticle;
        }
    }
}
