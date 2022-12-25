// Checked

using System.Collections.Generic;
using UnityEngine;

namespace DtAnimation
{
    [System.Serializable]
    public class DtObjectCount
    {
        [SerializeField]
        public string PrefabPath;

        [SerializeField]
        public GameObject Go;

        [SerializeField]
        public int Count;

        public DtObjectCount(string PrefabPath, GameObject Go, int Count)
        {
            this.PrefabPath = PrefabPath;
            this.Go = Go;
            this.Count = Count;
        }
    }

    public class DtAnimationNode : MonoBehaviour
    {
#if UNITY_EDITOR
        public static void Refresh()
        {
            string[] prefabGuids = UnityEditor.AssetDatabase.FindAssets("t:Prefab", new string[] { DtAnimationManager.UiPrefabPath });

            if (prefabGuids == null) return;

            // Read Data
            DtAnimationManager.Instance.GetReference(out DtAnimationReference dtAnimationReference);
            if (dtAnimationReference == null) return;

            SortedDictionary<string, DtAnimationReference.RefList> sequenceReference = dtAnimationReference.Data;
            sequenceReference.Clear();

            // Prefab
            for (int i = 0; i < prefabGuids.Length; i++)
            {
                var targetPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(UnityEditor.AssetDatabase.GUIDToAssetPath(prefabGuids[i]));
                if (targetPrefab == null) continue;

                string prefabPath = UnityEditor.AssetDatabase.GUIDToAssetPath(prefabGuids[i]);

                // DtAnimation
                DtAnimation[] Anims = targetPrefab.GetComponentsInChildren<DtAnimation>();
                for (int j = 0; j < Anims.Length; j++)
                {
                    DtAnimation Anim = Anims[j];
                    Dictionary<string, int> localCount = new Dictionary<string, int>();

                    // Sequence
                    for (int k = 0; k < Anims[j].Sequences.Count; k += 1)
                    {
                        var Sequence = Anims[j].Sequences[k];
                        if (Sequence.m_SequenceKey == null || Sequence.m_SequenceKey.Length <= 0) continue;

                        if (!localCount.ContainsKey(Sequence.m_SequenceKey))
                        {
                            localCount[Sequence.m_SequenceKey] = 0;
                        }

                        localCount[Sequence.m_SequenceKey] += 1;
                    } // Sequence

                    foreach (var Key in localCount.Keys)
                    {
                        if (!sequenceReference.ContainsKey(Key))
                        {
                            sequenceReference.Add(Key, new DtAnimationReference.RefList());
                        }

                        sequenceReference[Key].Data.Add(new DtObjectCount(prefabPath, Anim.gameObject, localCount[Key]));
                    }
                } // DtAnimation
            } // Prefab

            // Save Data
            DtAnimationManager.Instance.SaveReference();

        } // void Refresh()

        public static int FindReference(string sequenceKey)
        {
            // Read Data
            DtAnimationManager.Instance.GetReference(out DtAnimationReference dtAnimationReference);
            if (dtAnimationReference == null) return 0;

            SortedDictionary<string, DtAnimationReference.RefList> sequenceReference = dtAnimationReference.Data;

            return sequenceReference.ContainsKey(sequenceKey) ? sequenceReference[sequenceKey].Data.Count : 0;
        }
#endif
    } // class DtAnimationNode
} // namespace DtAnimation
