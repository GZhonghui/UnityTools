// Checked

using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

namespace DtAnimation
{
    [System.Serializable]
    public class DtSequence
    {
        // Deprecation
        public enum TriggerType
        {
            None = 0,
            OnOpen,
            OnClose,
            OnClick,
            OnValid,
            OnInvalid,
            ByManual,
        }

        // Deprecation 
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

        // Deprecation 
        public TriggerType m_TriggerType = TriggerType.None;

        public string m_TriggerName = "";
        public string m_SequenceKey = "";

        // Deprecation 
        public bool m_AutoReset = false;
    }

    public class DtAnimation : MonoBehaviour
    {
        [SerializeField]
        private List<DtSequence> m_Sequences = new List<DtSequence>();

        public List<DtSequence> Sequences { get { return m_Sequences; } }

        private bool m_Running = false;
        private Sequence m_RunningSequence = null;

        // Runtime
        public bool TryPlay(int triggerTypeInt, string triggerName = "", System.Action<GameObject> Callback = null)
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
                        // Deprecation
                        // if (autoReset) m_RunningSequence.Rewind(false);

                        Callback?.Invoke(this.gameObject);
                    });
                    m_RunningSequence.Play();
                    return true;
                }
                break;
            }
            return false;
        }

        public bool IsLinkdSequence(string sequenceKey)
        {
            for (int i = 0; i < Sequences.Count; i += 1)
            {
                if (Sequences[i].m_SequenceKey == sequenceKey) return true;
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
    } // class DtAnimation
} // namespace DtAnimation
