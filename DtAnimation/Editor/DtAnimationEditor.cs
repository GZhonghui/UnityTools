using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DG.Tweening;
using DG.DOTweenEditor;

namespace DtAnimation
{
    [CustomEditor(typeof(DtAnimation))]
    public class DtAnimationEditor : Editor
    {
        private class SequenceNode
        {
            public bool m_UnfoldSequence = false;
            public List<bool> m_UnfoldClips = new List<bool>();
            public bool m_Override = false;
            public DtSerializableSequence m_EditingSequenceCache = null;
            public Sequence m_PreviewSequence = null;

            public void LoadCache(string sequenceKey)
            {
                var animAsset = DtAnimationManager.Instance.GetAsset();

                // Use Present
                if (animAsset.Data.ContainsKey(sequenceKey))
                {
                    m_EditingSequenceCache = animAsset.Data[sequenceKey].DeepClone();
                }
                // New Sequence
                else
                {
                    m_EditingSequenceCache = new DtSerializableSequence();
                    m_EditingSequenceCache.m_Key = sequenceKey;
                }
            }

            public SequenceNode(string sequenceKey)
            {
                LoadCache(sequenceKey);
            }
        }

        private List<SequenceNode> m_SequenceNodes = new List<SequenceNode>();

        private LA_LuaComponent m_Lua = null;
        private HashSet<string> m_ExportAnimNames = new HashSet<string>();

        private bool m_NeedRefresh = false;

        public override void OnInspectorGUI()
        {
            DtAnimation ownGameObject = target as DtAnimation;
            if (ownGameObject == null) return;

            // Delay One Frame
            if (m_NeedRefresh)
            {
                DtAnimationNode.Refresh();
                m_NeedRefresh = false;
            }

            GUI.color = Color.yellow;
            bool needReloadLua = GUILayout.Button("Reload Lua Script (Slow)");
            GUI.color = Color.white;

            serializedObject.Update();

            #region Get Export in Lua
            // if (m_Lua == null)
            {
                m_Lua = ownGameObject.GetComponentInParent<LA_LuaComponent>();
                if (m_Lua != null)
                {
                    if (needReloadLua)
                    {
                        m_Lua.ReloadLuaScript();
                    }

                    // Require
                    if (m_Lua.GetComponent<DtAnimationRoot>() == null)
                    {
                        m_Lua.gameObject.AddComponent<DtAnimationRoot>();
                    }

                    var luaTable = m_Lua.GetLuaInstanceTable();
                    if (luaTable != null)
                    {
                        var exportAnims = luaTable.Get<XLua.LuaTable>("ExportAnims");

                        if (exportAnims != null)
                        {
                            m_ExportAnimNames.Clear();
                            foreach (var Key in exportAnims.GetKeys())
                            {
                                exportAnims.Get(Key, out string animName);
                                if (!m_ExportAnimNames.Contains(animName))
                                {
                                    m_ExportAnimNames.Add(animName);
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region Title
            GUI.color = Color.green;
            GUILayout.Label("DoTween Animation for RoMeta:");
            GUI.color = Color.white;
            #endregion

            var seqList = serializedObject.FindProperty("m_Sequences");
            ShowSequences(ownGameObject, seqList);

            #region Options
            GUILayout.BeginHorizontal();
            GUI.color = Color.green;
            if (GUILayout.Button("Add Sequence"))
            {
                ClickAddSequence(ownGameObject);
            }
            GUI.color = Color.red;
            if (GUILayout.Button("Remove Sequence"))
            {
                ClickRemoveSequence(ownGameObject);
            }
            GUILayout.EndHorizontal();
            GUI.color = Color.cyan;

            bool clickedApply = false;
            if (GUILayout.Button("Apply"))
            {
                ClickApply(ownGameObject);
                clickedApply = true;
            }
            GUI.color = Color.white;
            #endregion

            if (serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
            }
            
            // Refresh Root
            if (clickedApply)
            {
                var animRoot = ownGameObject.GetComponentInParent<DtAnimationRoot>();
                if (animRoot) animRoot.Refresh();
            }

            return;
        }

        private void ShowSequences(DtAnimation ownGameObject, SerializedProperty seqList)
        {
            // Same Size
            while (m_SequenceNodes.Count != seqList.arraySize)
            {
                if (m_SequenceNodes.Count < seqList.arraySize)
                {
                    var Seq = seqList.GetArrayElementAtIndex(m_SequenceNodes.Count);
                    var sequenceKey = Seq.FindPropertyRelative("m_SequenceKey");

                    m_SequenceNodes.Add(new SequenceNode(sequenceKey.stringValue));
                }

                else m_SequenceNodes.RemoveAt(m_SequenceNodes.Count - 1);
            }

            // Load Cache
            for (int i = 0; i < m_SequenceNodes.Count; i += 1)
            {
                if (m_SequenceNodes[i].m_EditingSequenceCache == null)
                {
                    var Seq = seqList.GetArrayElementAtIndex(i);
                    var sequenceKey = Seq.FindPropertyRelative("m_SequenceKey");

                    m_SequenceNodes[i].LoadCache(sequenceKey.stringValue);
                }
            }

            for (int i = 0; i < seqList.arraySize; i++)
            {
                #region Init
                var Seq = seqList.GetArrayElementAtIndex(i);
                var triggerType = Seq.FindPropertyRelative("m_TriggerType");
                var triggerName = Seq.FindPropertyRelative("m_TriggerName");
                var sequenceKey = Seq.FindPropertyRelative("m_SequenceKey");
                var autoReset   = Seq.FindPropertyRelative("m_AutoReset");

                var animAsset = DtAnimationManager.Instance.GetAsset();

                GUI.color = Color.yellow;
                m_SequenceNodes[i].m_UnfoldSequence = EditorGUILayout.Foldout(
                    m_SequenceNodes[i].m_UnfoldSequence, "Sequences: " + i.ToString()
                );
                GUI.color = Color.white;

                if (!m_SequenceNodes[i].m_UnfoldSequence) continue; // Hide This Sequence
                #endregion

                EditorGUI.indentLevel += 1;

                #region Sequence Option
                GUILayout.BeginHorizontal();
                GUI.color = Color.green;
                if (GUILayout.Button("Preview"))
                {
                    DOTweenEditorPreview.Stop();

                    if (m_SequenceNodes[i].m_PreviewSequence != null)
                    {
                        m_SequenceNodes[i].m_PreviewSequence.Rewind(false);
                    }

                    m_SequenceNodes[i].m_PreviewSequence = m_SequenceNodes[i].m_EditingSequenceCache.CreateSequence(ownGameObject.transform);
                    DOTweenEditorPreview.PrepareTweenForPreview(m_SequenceNodes[i].m_PreviewSequence);
                    DOTweenEditorPreview.Start();
                }
                GUI.color = Color.gray;
                if (GUILayout.Button("Stop"))
                {
                    DOTweenEditorPreview.Stop();
                    if (m_SequenceNodes[i].m_PreviewSequence != null)
                    {
                        m_SequenceNodes[i].m_PreviewSequence.Rewind(false);
                    }
                }
                GUI.color = Color.white;
                GUILayout.EndHorizontal();

                // Trigger Type
                triggerType.intValue = (int)DtSequence.TriggerType.ByManual;

                // Trigger Name
                int selectedIndex = -1;
                List<string> exportNameList = new List<string>();
                foreach (var Item in m_ExportAnimNames)
                {
                    exportNameList.Add(Item);
                    if (Item == triggerName.stringValue)
                    {
                        selectedIndex = exportNameList.Count - 1;
                    }
                }

                selectedIndex = EditorGUILayout.Popup("Trigger Name", selectedIndex, exportNameList.ToArray());
                if (selectedIndex > -1 && selectedIndex < exportNameList.Count)
                {
                    triggerName.stringValue = exportNameList[selectedIndex];
                }
                else
                {
                    triggerName.stringValue = ""; // Reset to Empty String
                }

                // Sequence Present
                // By Select
                int Selected = 0, Index = 0;
                List<string> presentOptions = new List<string>();

                presentOptions.Add("Create New");
                foreach (var Item in animAsset.Data)
                {
                    Index += 1;
                    presentOptions.Add(Item.Key);
                    if (Item.Key == sequenceKey.stringValue)
                    {
                        Selected = Index; // Already Had
                    }
                }

                int updatedSelected = EditorGUILayout.Popup("Select From", Selected, presentOptions.ToArray());

                if (updatedSelected != Selected) // Select Changed
                {
                    if (updatedSelected != 0) // Select Present
                    {
                        // Load Present
                        sequenceKey.stringValue = presentOptions[updatedSelected];
                        m_SequenceNodes[i].m_EditingSequenceCache = animAsset.Data[sequenceKey.stringValue].DeepClone();

                        m_NeedRefresh = true;
                    }
                    else // Create New
                    {
                        // Clear Key
                        sequenceKey.stringValue = "";
                    }
                }

                // By Input
                sequenceKey.stringValue = EditorGUILayout.TextField("Sequence Key", sequenceKey.stringValue);

                // Auto Reset
                // Deprecation
                // autoReset.boolValue = EditorGUILayout.Toggle("Auto Reset", autoReset.boolValue);

                int refCount = DtAnimationNode.FindReference(sequenceKey.stringValue);
                bool safeToEdit = refCount <= 1;
                GUI.color = safeToEdit ? Color.green : Color.red;

                string toggleText = "Override [Sequence Reference Count = " + refCount.ToString() + "]";
                m_SequenceNodes[i].m_Override = GUILayout.Toggle(m_SequenceNodes[i].m_Override, toggleText);

                GUI.color = Color.white;

                // Keep the Sequence
                if (!safeToEdit && !m_SequenceNodes[i].m_Override) continue;
                #endregion

                #region Edit Sequence
                var EditingCache = m_SequenceNodes[i].m_EditingSequenceCache;

                GUILayout.BeginHorizontal();
                GUI.color = Color.green;
                if (GUILayout.Button("Add Clip"))
                {
                    EditingCache.AddClip(new DtSerializableSequence.DtSerializableClip());
                }
                GUI.color = Color.gray;
                if (GUILayout.Button("Remove Clip"))
                {
                    EditingCache.RemoveClip();
                }
                GUI.color = Color.white;
                GUILayout.EndHorizontal();

                // Show Clips
                ShowClips(i, sequenceKey.stringValue);
                #endregion

                EditorGUI.indentLevel -= 1;

                EditorGUILayout.Space();
            }
        }

        private void ShowClips(int Index, string seqKey)
        {
            if (m_SequenceNodes.Count <= Index) return;

            var EditingCache = m_SequenceNodes[Index].m_EditingSequenceCache;
            EditingCache.m_Key = seqKey; // Update Key

            var clipUnfold = m_SequenceNodes[Index].m_UnfoldClips;
            while (clipUnfold.Count != EditingCache.m_Clips.Count)
            {
                if (clipUnfold.Count < EditingCache.m_Clips.Count)
                {
                    clipUnfold.Add(false);
                }
                else
                {
                    clipUnfold.RemoveAt(clipUnfold.Count - 1);
                }
            }

            for (int i = 0; i < EditingCache.m_Clips.Count; i += 1)
            {
                GUI.color = Color.yellow;
                m_SequenceNodes[Index].m_UnfoldClips[i] = EditorGUILayout.Foldout(
                    m_SequenceNodes[Index].m_UnfoldClips[i], "Clips: " + i.ToString()
                );
                GUI.color = Color.white;

                // Hide the Clip
                if (!m_SequenceNodes[Index].m_UnfoldClips[i]) continue;

                var clipCache = EditingCache.m_Clips[i];

                // ClipType
                var beforeUpdate = clipCache.m_ClipType;
                clipCache.m_ClipType = (DtSerializableSequence.DtAnimationClipType)EditorGUILayout.Popup("Clip Type",
                    (int)clipCache.m_ClipType, DtSerializableSequence.DtAnimationClipTypeText
                );

                // Select New
                if (beforeUpdate != clipCache.m_ClipType)
                {
                    clipCache.ResetValues(clipCache.m_ClipType);
                }

                // Common Properties
                clipCache.m_Delay    = EditorGUILayout.FloatField("Delay", clipCache.m_Delay);
                clipCache.m_Loop     = EditorGUILayout.IntField("Loop", clipCache.m_Loop);
                clipCache.m_Duration = EditorGUILayout.FloatField("Duration", clipCache.m_Duration);
                clipCache.m_Curve    = EditorGUILayout.CurveField("Ease", clipCache.m_Curve);

                // Draw Properties
                switch (clipCache.m_ClipType)
                {
                    case DtSerializableSequence.DtAnimationClipType.Move:
                        PropertyVector3(clipCache, "Move Target");
                        PropertySnapping(clipCache);
                        break;

                    case DtSerializableSequence.DtAnimationClipType.LocalMove:
                        PropertyVector3(clipCache, "Move Target");
                        PropertySnapping(clipCache);
                        break;

                    case DtSerializableSequence.DtAnimationClipType.Rotate:
                        PropertyVector3(clipCache, "Rotate Target");
                        break;

                    case DtSerializableSequence.DtAnimationClipType.LocalRotate:
                        PropertyVector3(clipCache, "Rotate Target");
                        break;

                    case DtSerializableSequence.DtAnimationClipType.Scale:
                        PropertyUniformScale(clipCache);
                        if (clipCache.m_UniformScale) PropertyScale(clipCache);
                        else PropertyVector3(clipCache, "Scale Target");
                        break;

                    case DtSerializableSequence.DtAnimationClipType.Jump:
                        PropertyVector3(clipCache, "Jump Target");
                        PropertyJumpPower(clipCache);
                        PropertyJumpNumber(clipCache);
                        PropertySnapping(clipCache);
                        break;

                    case DtSerializableSequence.DtAnimationClipType.LocalJump:
                        PropertyVector3(clipCache, "Jump Target");
                        PropertyJumpPower(clipCache);
                        PropertyJumpNumber(clipCache);
                        PropertySnapping(clipCache);
                        break;

                    case DtSerializableSequence.DtAnimationClipType.ShakeLocation:
                        PropertyUniformStrength(clipCache);
                        if (clipCache.m_UniformStrength) PropertyStrength(clipCache);
                        else PropertyVector3(clipCache, "Strength");
                        PropertyVibrato(clipCache);
                        PropertyRandomness(clipCache);
                        PropertySnapping(clipCache);
                        PropertyFadeout(clipCache);
                        break;

                    case DtSerializableSequence.DtAnimationClipType.ShakeRotation:
                        PropertyUniformStrength(clipCache);
                        if (clipCache.m_UniformStrength) PropertyStrength(clipCache);
                        else PropertyVector3(clipCache, "Strength");
                        PropertyVibrato(clipCache);
                        PropertyRandomness(clipCache);
                        PropertyFadeout(clipCache);
                        break;

                    case DtSerializableSequence.DtAnimationClipType.ShakeScale:
                        PropertyUniformStrength(clipCache);
                        if (clipCache.m_UniformStrength) PropertyStrength(clipCache);
                        else PropertyVector3(clipCache, "Strength");
                        PropertyVibrato(clipCache);
                        PropertyRandomness(clipCache);
                        PropertyFadeout(clipCache);
                        break;

                    case DtSerializableSequence.DtAnimationClipType.PunchLocation:
                        PropertyVector3(clipCache, "Punch");
                        PropertyVibrato(clipCache);
                        PropertyElasticity(clipCache);
                        PropertySnapping(clipCache);
                        break;

                    case DtSerializableSequence.DtAnimationClipType.PunchRotation:
                        PropertyVector3(clipCache, "Punch");
                        PropertyVibrato(clipCache);
                        PropertyElasticity(clipCache);
                        break;

                    case DtSerializableSequence.DtAnimationClipType.PunchScale:
                        PropertyVector3(clipCache, "Punch");
                        PropertyVibrato(clipCache);
                        PropertyElasticity(clipCache);
                        break;
                }
            }
        }

        private void PropertyVector3(DtSerializableSequence.DtSerializableClip clipCache, string Tips = "Vector3")
        {
            clipCache.m_Values = EditorGUILayout.Vector3Field(Tips, clipCache.m_Values);
        }

        private void PropertyScale(DtSerializableSequence.DtSerializableClip clipCache)
        {
            clipCache.m_Scale = EditorGUILayout.FloatField("Scale Target", clipCache.m_Scale);
        }

        private void PropertyStrength(DtSerializableSequence.DtSerializableClip clipCache)
        {
            clipCache.m_Strength = EditorGUILayout.FloatField("Strength", clipCache.m_Strength);
        }

        private void PropertySnapping(DtSerializableSequence.DtSerializableClip clipCache)
        {
            clipCache.m_Snapping = EditorGUILayout.Toggle("Snapping", clipCache.m_Snapping);
        }

        private void PropertyJumpPower(DtSerializableSequence.DtSerializableClip clipCache)
        {
            clipCache.m_JumpPower = EditorGUILayout.FloatField("Jump Power", clipCache.m_JumpPower);
        }

        private void PropertyJumpNumber(DtSerializableSequence.DtSerializableClip clipCache)
        {
            clipCache.m_JumpNumber = EditorGUILayout.IntField("Jump Number", clipCache.m_JumpNumber);
        }

        private void PropertyUniformScale(DtSerializableSequence.DtSerializableClip clipCache)
        {
            clipCache.m_UniformScale = EditorGUILayout.Toggle("Uniform Scale", clipCache.m_UniformScale);
        }

        private void PropertyUniformStrength(DtSerializableSequence.DtSerializableClip clipCache)
        {
            clipCache.m_UniformStrength = EditorGUILayout.Toggle("Uniform Strength", clipCache.m_UniformStrength);
        }

        private void PropertyVibrato(DtSerializableSequence.DtSerializableClip clipCache)
        {
            clipCache.m_Vibrato = EditorGUILayout.IntField("Vibrato", clipCache.m_Vibrato);
        }

        private void PropertyRandomness(DtSerializableSequence.DtSerializableClip clipCache)
        {
            clipCache.m_Randomness = EditorGUILayout.FloatField("Randomness", clipCache.m_Randomness);
        }

        private void PropertyElasticity(DtSerializableSequence.DtSerializableClip clipCache)
        {
            clipCache.m_Elasticity = EditorGUILayout.FloatField("Elasticity", clipCache.m_Elasticity);
        }

        private void PropertyFadeout(DtSerializableSequence.DtSerializableClip clipCache)
        {
            clipCache.m_Fadeout = EditorGUILayout.Toggle("Fadeout", clipCache.m_Fadeout);
        }

        private void ClickAddSequence(DtAnimation ownGameObject)
        {
            ownGameObject.Sequences.Add(new DtSequence());
        }

        private void ClickRemoveSequence(DtAnimation ownGameObject)
        {
            if (ownGameObject.Sequences.Count > 0)
            {
                ownGameObject.Sequences.RemoveAt(ownGameObject.Sequences.Count - 1);
            }
        }

        private void ClickApply(DtAnimation ownGameObject)
        {
            // Save GameObject
            EditorUtility.SetDirty(target);

            // Save Asset
            var animAsset = DtAnimationManager.Instance.GetAsset();

            foreach (var Node in m_SequenceNodes)
            {
                var cacheItem = Node.m_EditingSequenceCache;
                if (cacheItem.m_Key.Length > 0)
                {
                    animAsset.Data[cacheItem.m_Key] = cacheItem.DeepClone();
                }
            }

            DtAnimationManager.Instance.SaveToAsset();

            // Reload Cache
            // m_SequenceNodes.Clear();
            for (int i = 0; i < m_SequenceNodes.Count; i++)
            {
                m_SequenceNodes[i].m_EditingSequenceCache = null;
                m_SequenceNodes[i].m_PreviewSequence = null;
            }
        }
    }
} // namespace DtAnimation
