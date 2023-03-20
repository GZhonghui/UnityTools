using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TL_SignalParticle : UnityEngine.Timeline.Marker, INotification
{
    public PropertyName id { get; }

    public bool m_bEnable;
}
