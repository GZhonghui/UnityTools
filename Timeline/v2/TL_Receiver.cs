using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

// 通用信号处理
// 挂在每一个Play Director上面
public class TL_Receiver : MonoBehaviour, INotificationReceiver
{
    public void OnNotify(Playable origin, INotification notification, object context)
    {
        Type incomeType = notification.GetType();

        // 通用事件
        if (incomeType == typeof(TL_SignalCommon))
        {
            var kSignal = notification as TL_SignalCommon;
        }
        // Wwise事件
        else if (incomeType == typeof(TL_SignalWwise))
        {
            var kSignal = notification as TL_SignalWwise;
        }
    }
}
