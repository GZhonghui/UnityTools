using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(1, 1, 0)]
[TrackClipType(typeof(TL_ClipEffect))]
public class TL_TrackEffect : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<TL_MixerEffect>.Create(graph, inputCount);
    }
}
