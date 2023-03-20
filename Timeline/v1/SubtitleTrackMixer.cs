using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class SubtitleTrackMixer : PlayableBehaviour
{
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        base.ProcessFrame(playable, info, playerData);

        TextMeshProUGUI textUi = playerData as TextMeshProUGUI;
        if (textUi == null) return;

        string currentText = "";
        float currentAlpha = 0.0f;

        int inputCount = playable.GetInputCount();
        for (int i = 0; i < inputCount; i += 1)
        {
            float inputWeight = playable.GetInputWeight(i);
            if (inputWeight > 0.0f)
            {
                var inputBehaviour = ((ScriptPlayable<SubtitleBehaviour>)playable.GetInput(i)).GetBehaviour();

                currentText = currentText + inputBehaviour.subtitleText;
                currentAlpha = inputWeight;

            }
        }

        textUi.text = currentText;
        textUi.alpha = currentAlpha;
    }
}
