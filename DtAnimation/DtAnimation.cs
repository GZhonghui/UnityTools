using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;
using UnityEditor;
using DG.Tweening;

[System.Serializable]
public class DtSequence
{
    public enum TriggerType
    {
        None,
        OnOpen,
        OnClose,
        OnClick,
        OnValid,
        OnInvalid,
        ByManual,
    }

    public static string[] TriggerTypeText =
    {
        "None",
        "OnOpen",
        "OnClose",
        "OnClick",
        "OnValid",
        "OnInvalid",
        "ByManual",
    };

    public TriggerType m_TriggerType = TriggerType.None;
    public string m_TriggerName = "";
    public string m_SequenceKey = "";
    public bool m_AutoReset = true;
}

public class DtAnimation : MonoBehaviour
{
    [SerializeField] public List<DtSequence> m_Sequences = new List<DtSequence>();

    private bool m_Running = false;
    private Sequence m_RunningSequence = null;

    // Runtime
    public bool TryPlay(int triggerTypeInt, string triggerName = "")
    {
        DtSequence.TriggerType triggerType = (DtSequence.TriggerType)triggerTypeInt;

        if (m_Running) return false;

        foreach (var Item in m_Sequences)
        {
            if (Item.m_TriggerType != triggerType) continue;
            if (Item.m_TriggerType == DtSequence.TriggerType.ByManual && Item.m_TriggerName != triggerName) continue;

            var Asset = DtAnimationManager.Instance.GetAsset();
            if (Asset && Asset.Data.ContainsKey(Item.m_SequenceKey))
            {
                bool autoReset = Item.m_AutoReset;

                m_Running = true;
                m_RunningSequence = Asset.Data[Item.m_SequenceKey].CreateSequence(transform);

                m_RunningSequence.OnComplete(() =>
                {
                    m_Running = false;
                    if (autoReset) m_RunningSequence.Rewind(false);
                });
                m_RunningSequence.Play();
                return true;
            }
            break;
        }
        return false;
    }

    public bool IsPlaying()
    {
        return m_Running;
    }

    public void ForceComplate()
    {
        if (!IsPlaying()) return;

        if (m_RunningSequence != null)
        {
            m_RunningSequence.Complete(true);
        }
    }
}
