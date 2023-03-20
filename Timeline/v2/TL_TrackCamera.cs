using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Cinemachine;

// 相机移动
[TrackColor(0,1,0)]
[TrackClipType(typeof(TL_ClipCamera))]
[TrackBindingType(typeof(GameObject))]
public class TL_TrackCamera : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<TL_MixerCamera>.Create(graph, inputCount);
    }
}
