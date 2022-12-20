using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class SubtitleController : MonoBehaviour
{
    
    void Start()
    {
        string trackName = "Player Subtitle";

        var playableDirector = GetComponent<PlayableDirector>();
        if (playableDirector != null)
        {
            foreach (var Output in playableDirector.playableAsset.outputs)
            {
                // Get Binding Track Name
                if (Output.sourceObject != null &&
                    Output.sourceObject.name == trackName &&
                    Output.outputTargetType == typeof(TextMeshProUGUI))
                {
                    var textUi = FindObjectOfType<TextMeshProUGUI>();
                    if (textUi != null)
                    {
                        playableDirector.SetGenericBinding(Output.sourceObject, textUi);
                        playableDirector.initialTime = 2.0f;
                        playableDirector.Play();

                        Debug.Log("Start Play!");
                        break;
                    }
                }
            }
        }
    }
}
