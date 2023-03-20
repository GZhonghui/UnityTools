using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TL_MixerEffect : PlayableBehaviour
{
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        base.ProcessFrame(playable, info, playerData);

        var kCam = TL_Utility.FindTimelineCamera();

        var kStoryboard = kCam.GetComponent<CinemachineStoryboard>();
        if (kStoryboard == null)
        {
            kStoryboard = kCam.gameObject.AddComponent<CinemachineStoryboard>();
        }

        float fStoryboardAlpha = 0.0f;

        var inputCount = playable.GetInputCount();
        for (int i = 0; i < inputCount; i++)
        {
            if (playable.GetInputWeight(i) <= 0) continue;

            var uPlayableType = playable.GetInput(i).GetPlayableType();
            if (uPlayableType == typeof(TL_BehaviourEffect))
            {
                var kEffectClip = playable.GetInput(i);
                var kEffectBehaviour = ((ScriptPlayable<TL_BehaviourEffect>)kEffectClip).GetBehaviour();

                if (kEffectBehaviour != null)
                {
                    if (kEffectBehaviour.m_uEffectType == TL_ClipEffect.EffectType.PureColor)
                    {
                        fStoryboardAlpha = playable.GetInputWeight(i);
                    }
                }
            }
        }

        kStoryboard.m_Alpha = fStoryboardAlpha;
    }
}