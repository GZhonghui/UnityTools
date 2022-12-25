// Checked

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DtAnimation
{
    [System.Serializable]
    public class GoList
    {
        [SerializeField]
        public List<GameObject> Data = new List<GameObject>();
    }

    public class DtAnimationRoot : MonoBehaviour
    {
        [SerializeField]
        private DtSerializationDict<string, GoList> animTargets = new DtSerializationDict<string, GoList>();

        public SortedDictionary<string, GoList> Data { get { return animTargets.m_Dict; } }

        public void TryPlay(string animKey, System.Action Callback = null)
        {
            if (!Data.ContainsKey(animKey)) return;

            // Running Status
            HashSet<GameObject> runningObject = new HashSet<GameObject>();
            List<DtAnimation> willPlay = new List<DtAnimation>();

            for (int i = 0; i < Data[animKey].Data.Count; i += 1)
            {
                GameObject animTarget = Data[animKey].Data[i];
                if (animTarget == null) continue;

                var dtAnim = animTarget.GetComponent<DtAnimation>();
                if (dtAnim != null)
                {
                    if (runningObject.Contains(animTarget)) continue;

                    runningObject.Add(animTarget);
                    willPlay.Add(dtAnim);
                }
            }

            for (int i = 0; i < willPlay.Count; i += 1)
            {
                willPlay[i].TryPlay((int)DtSequence.TriggerType.ByManual, animKey, (Go) =>
                {
                    if (runningObject.Contains(Go))
                    {
                        runningObject.Remove(Go);
                        if (runningObject.Count <= 0)
                        {
                            Callback?.Invoke();
                        }
                    }
                });
            }
        }

#if UNITY_EDITOR
        public void Refresh()
        {
            var So = new UnityEditor.SerializedObject(this);

            Data.Clear();

            DtAnimation[] Anims = GetComponentsInChildren<DtAnimation>();

            for (int i = 0; i < Anims.Length; i++)
            {
                DtAnimation Anim = Anims[i];

                for (int j = 0; j < Anim.Sequences.Count; j++)
                {
                    DtSequence dtSequence = Anim.Sequences[j];

                    if (dtSequence.m_TriggerName == null || dtSequence.m_TriggerName.Length == 0) continue;

                    if (!Data.ContainsKey(dtSequence.m_TriggerName))
                    {
                        Data.Add(dtSequence.m_TriggerName, new GoList());
                    }

                    Data[dtSequence.m_TriggerName].Data.Add(Anim.gameObject);
                }
            }

            UnityEditor.EditorUtility.SetDirty(this);
            So.ApplyModifiedProperties();

            // Slow...
            DtAnimationNode.Refresh();
        }
#endif
    } // class DtAnimationRoot
} // namespace DtAnimation