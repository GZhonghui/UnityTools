// Checked

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DtAnimation
{
    [CustomEditor(typeof(DtAnimationNode))]
    public class DtAnimationNodeEditor : Editor
    {
        private SortedDictionary<string, DtAnimationReference.RefList> sequenceReference;
        private Dictionary<string, bool> Fold = new Dictionary<string, bool>();

        [MenuItem("Tools/Ë¢ÐÂ¶¯Ð§¿â", false, 601)]
        public static void DoRefresh()
        {
            DtAnimationNode.Refresh();
        }

        public override void OnInspectorGUI()
        {
            // Get Reference
            DtAnimationManager.Instance.GetReference(out DtAnimationReference dtAnimationReference);
            if (dtAnimationReference == null)
            {
                GUILayout.Label("Play Mode");
                return;
            }

            sequenceReference = dtAnimationReference.Data;

            #region Title
            GUI.color = Color.green;
            GUILayout.Label("DoTween Animation for RoMeta:");
            GUI.color = Color.white;
            #endregion

            GUI.color = Color.green;
            if (GUILayout.Button("Refresh"))
            {
                DtAnimationNode.Refresh(); // Write to Asset
            }
            GUI.color = Color.white;

            var animAsset = DtAnimationManager.Instance.GetAsset();
            List<string> markToRemove = new List<string>();

            foreach (var Item in animAsset.Data)
            {
                if (DrawSequence(Item.Key))
                {
                    markToRemove.Add(Item.Key);
                }
            }

            foreach (var Item in sequenceReference)
            {
                if (animAsset.Data.ContainsKey(Item.Key)) continue;

                DrawSequence(Item.Key);
            }

            foreach (var Item in markToRemove)
            {
                if(animAsset.Data.ContainsKey(Item))
                {
                    animAsset.Data.Remove(Item);
                }
            }

            // Apply Remove
            if (markToRemove.Count > 0)
            {
                DtAnimationManager.Instance.SaveToAsset();
            }
        }

        private bool DrawSequence(string sequenceKey)
        {
            if (sequenceKey == null || sequenceKey.Length <= 0) return false;

            bool needDelete = false;
            var animAsset = DtAnimationManager.Instance.GetAsset();

            bool Exist = animAsset.Data.ContainsKey(sequenceKey);

            if (!Fold.ContainsKey(sequenceKey))
            {
                Fold[sequenceKey] = false;
            }

            GUILayout.BeginHorizontal();

            if (Exist) GUI.color = Color.green;
            else GUI.color = Color.red;

            int refCount = sequenceReference.ContainsKey(sequenceKey) ? sequenceReference[sequenceKey].Data.Count : 0;
            Fold[sequenceKey] = EditorGUILayout.Foldout(Fold[sequenceKey], "Sequence: " + sequenceKey + " [Reference = " + refCount.ToString() + "]");

            GUI.color = Color.white;

            GUILayout.FlexibleSpace();

            if (Exist)
            {
                GUI.color = Color.red;
                if (GUILayout.Button("Remove"))
                {
                    needDelete = true;
                }
                GUI.color = Color.white;
            }

            GUILayout.EndHorizontal();

            if (Fold[sequenceKey])
            {
                DrawReference(sequenceKey);
            }

            return needDelete;
        }

        private void DrawReference(string sequenceKey)
        {
            if (!sequenceReference.ContainsKey(sequenceKey)) return;

            for (int i = 0; i < sequenceReference[sequenceKey].Data.Count; i++)
            {
                GUILayout.BeginHorizontal();

                GUI.color = Color.yellow;
                var Ref = sequenceReference[sequenceKey].Data[i];
                string refName = Ref.PrefabPath.Substring(DtAnimationManager.UiPrefabPath.Length) + "#" + Ref.Go.name + " [" + Ref.Count + "]";
                GUILayout.Label(refName);
                GUI.color = Color.white;

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Open Prefab"))
                {
                    if (Ref.Go)
                    {
                        UnityEditor.AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<GameObject>(Ref.PrefabPath));
                        // UnityEditor.Selection.activeGameObject = Ref.Go;
                    }
                }
                GUILayout.EndHorizontal();
            }
        }
    } // class DtAnimationNodeEditor
} // namespace DtAnimation