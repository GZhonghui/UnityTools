using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor; // Not For Build
using DG.Tweening;
using DG.DOTweenEditor;

[CustomEditor(typeof(DtAnimation))]
public class DtAnimationEditor : Editor
{
    private List<bool> m_UnfoldSequence = new List<bool>();
    private List<List<bool>> m_UnfoldClips = new List<List<bool>>();
    private List<DtSerializableSequence> m_EditingSequenceCache = new List<DtSerializableSequence>();
    private List<Sequence> m_PreviewSequence = new List<Sequence>();

    public override void OnInspectorGUI()
    {
        DtAnimation ownGameObject = target as DtAnimation;
        if (ownGameObject == null) return;

        serializedObject.Update();

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
        if (GUILayout.Button("Clear All"))
        {
            ClickClearAll(ownGameObject);
        }
        GUILayout.EndHorizontal();
        GUI.color = Color.cyan;
        if (GUILayout.Button("Apply"))
        {
            ClickApply(ownGameObject);
        }
        GUI.color = Color.white;
        #endregion

        serializedObject.ApplyModifiedProperties();

        return;
    }

    private void ShowSequences(DtAnimation ownGameObject, SerializedProperty seqList)
    {
        for (int i = 0; i < seqList.arraySize; i++)
        {
            #region Init
            var Seq = seqList.GetArrayElementAtIndex(i);
            var triggerType = Seq.FindPropertyRelative("m_TriggerType");
            var triggerName = Seq.FindPropertyRelative("m_TriggerName");
            var sequenceKey = Seq.FindPropertyRelative("m_SequenceKey");
            var autoReset   = Seq.FindPropertyRelative("m_AutoReset");

            var Asset = DtAnimationManager.Instance.GetAsset();

            // Init Foldout State
            while (m_UnfoldSequence.Count <= i)
            {
                m_UnfoldSequence.Add(false);
            }
            while (m_UnfoldClips.Count <= i)
            {
                m_UnfoldClips.Add(new List<bool>());
            }

            // Init Cache
            while (m_EditingSequenceCache.Count <= i)
            {
                if (m_EditingSequenceCache.Count == i)
                {
                    // Use Present
                    if (Asset.Data.ContainsKey(sequenceKey.stringValue))
                    {
                        m_EditingSequenceCache.Add(Asset.Data[sequenceKey.stringValue].DeepClone());
                    }
                    // New Sequence
                    else
                    {
                        m_EditingSequenceCache.Add(new DtSerializableSequence());
                        m_EditingSequenceCache[i].m_Key = sequenceKey.stringValue;
                    }
                }
                else
                {
                    m_EditingSequenceCache.Add(new DtSerializableSequence());
                }
            }

            // Init Preview
            while (m_PreviewSequence.Count <= i)
            {
                m_PreviewSequence.Add(null);
            }

            GUI.color = Color.yellow;
            m_UnfoldSequence[i] = EditorGUILayout.Foldout(m_UnfoldSequence[i], "Sequences: " + i.ToString());
            GUI.color = Color.white;

            if (!m_UnfoldSequence[i]) continue; // Hide
            #endregion

            EditorGUI.indentLevel += 1;

            #region Sequence Option
            GUILayout.BeginHorizontal();
            GUI.color = Color.green;
            if (GUILayout.Button("Preview"))
            {
                DOTweenEditorPreview.Stop();
                if (m_PreviewSequence[i] != null)
                {
                    m_PreviewSequence[i].Rewind(false);
                }

                m_PreviewSequence[i] = m_EditingSequenceCache[i].CreateSequence(ownGameObject.transform);
                DOTweenEditorPreview.PrepareTweenForPreview(m_PreviewSequence[i]);
                DOTweenEditorPreview.Start();
            }
            GUI.color = Color.gray;
            if (GUILayout.Button("Stop & Reset"))
            {
                DOTweenEditorPreview.Stop();
                if (m_PreviewSequence[i] != null)
                {
                    m_PreviewSequence[i].Rewind(false);
                }
            }
            GUI.color = Color.red;
            if (GUILayout.Button("Remove This Sequence"))
            {
                ownGameObject.m_Sequences.RemoveAt(i);
                m_UnfoldSequence.RemoveAt(i);
                m_UnfoldClips.RemoveAt(i);
                m_EditingSequenceCache.RemoveAt(i);
                m_PreviewSequence.RemoveAt(i);
            }
            GUI.color = Color.white;
            GUILayout.EndHorizontal();

            // Trigger Event
            triggerType.intValue = EditorGUILayout.Popup("Trigger Type",
                triggerType.intValue, DtSequence.TriggerTypeText
            );

            if (triggerType.intValue == (int)DtSequence.TriggerType.ByManual)
            {
                triggerName.stringValue = EditorGUILayout.TextField("Trigger Name", triggerName.stringValue);
            }

            // Sequence Present
            // By Select
            int Selected = 0, Index = 0;
            List<string> presentOptions = new List<string>();

            presentOptions.Add("Create New");
            foreach (var Item in Asset.Data)
            {
                Index += 1;
                presentOptions.Add(Item.Key);
                if (Item.Key == sequenceKey.stringValue)
                {
                    Selected = Index; // Already Had
                }
            }

            int updatedSelected = EditorGUILayout.Popup("Select From", Selected, presentOptions.ToArray());

            if (updatedSelected != Selected) // Select New Present
            {
                if (updatedSelected != 0) // Not Create New
                {
                    // Load Present
                    sequenceKey.stringValue = presentOptions[updatedSelected];
                    m_EditingSequenceCache[i] = Asset.Data[sequenceKey.stringValue].DeepClone();
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
            autoReset.boolValue = EditorGUILayout.Toggle("Auto Reset", autoReset.boolValue);
            #endregion

            #region Edit Sequence
            var EditingCache = m_EditingSequenceCache[i];

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
        if (m_EditingSequenceCache.Count <= Index) return;

        var EditingCache = m_EditingSequenceCache[Index];
        EditingCache.m_Key = seqKey; // Update Key

        for (int i = 0; i < EditingCache.m_Clips.Count; i += 1)
        {
            // Draw a Clip
            while (m_UnfoldClips[Index].Count <= i)
            {
                m_UnfoldClips[Index].Add(false);
            }

            GUI.color = Color.yellow;
            m_UnfoldClips[Index][i] = EditorGUILayout.Foldout(m_UnfoldClips[Index][i], "Clips: " + i.ToString());
            GUI.color = Color.white;

            if (!m_UnfoldClips[Index][i]) continue;

            if (EditingCache.m_Clips.Count <= i) break;

            var clipCache = EditingCache.m_Clips[i];

            var beforeUpdate = clipCache.m_ClipType;
            clipCache.m_ClipType = (DtSerializableSequence.DtAnimationClipType)EditorGUILayout.Popup("Clip Type",
                (int)clipCache.m_ClipType, DtSerializableSequence.DtAnimationClipTypeText
            );

            // Select New
            if (beforeUpdate != clipCache.m_ClipType)
            {
                clipCache.ResetValues(clipCache.m_ClipType);
            }

            // Draw Properties
            switch (clipCache.m_ClipType)
            {
                case DtSerializableSequence.DtAnimationClipType.Move:
                    clipCache.m_Delay    = EditorGUILayout.FloatField("Delay", clipCache.m_Delay);
                    clipCache.m_Loop     = EditorGUILayout.IntField("Loop", clipCache.m_Loop);
                    clipCache.m_Duration = EditorGUILayout.FloatField("Duration", clipCache.m_Duration);
                    clipCache.m_Values   = EditorGUILayout.Vector3Field("Move Target", clipCache.m_Values);
                    clipCache.m_Snapping = EditorGUILayout.Toggle("Snapping", clipCache.m_Snapping);
                    clipCache.m_Curve    = EditorGUILayout.CurveField("Ease", clipCache.m_Curve);
                    break;

                case DtSerializableSequence.DtAnimationClipType.LocalMove:
                    clipCache.m_Delay    = EditorGUILayout.FloatField("Delay", clipCache.m_Delay);
                    clipCache.m_Loop     = EditorGUILayout.IntField("Loop", clipCache.m_Loop);
                    clipCache.m_Duration = EditorGUILayout.FloatField("Duration", clipCache.m_Duration);
                    clipCache.m_Values   = EditorGUILayout.Vector3Field("Move Target", clipCache.m_Values);
                    clipCache.m_Snapping = EditorGUILayout.Toggle("Snapping", clipCache.m_Snapping);
                    clipCache.m_Curve    = EditorGUILayout.CurveField("Ease", clipCache.m_Curve);
                    break;

                case DtSerializableSequence.DtAnimationClipType.Rotate:
                    clipCache.m_Delay    = EditorGUILayout.FloatField("Delay", clipCache.m_Delay);
                    clipCache.m_Loop     = EditorGUILayout.IntField("Loop", clipCache.m_Loop);
                    clipCache.m_Duration = EditorGUILayout.FloatField("Duration", clipCache.m_Duration);
                    clipCache.m_Values   = EditorGUILayout.Vector3Field("Rotate Target", clipCache.m_Values);
                    clipCache.m_Curve    = EditorGUILayout.CurveField("Ease", clipCache.m_Curve);
                    break;

                case DtSerializableSequence.DtAnimationClipType.LocalRotate:
                    clipCache.m_Delay    = EditorGUILayout.FloatField("Delay", clipCache.m_Delay);
                    clipCache.m_Loop     = EditorGUILayout.IntField("Loop", clipCache.m_Loop);
                    clipCache.m_Duration = EditorGUILayout.FloatField("Duration", clipCache.m_Duration);
                    clipCache.m_Values   = EditorGUILayout.Vector3Field("Rotate Target", clipCache.m_Values);
                    clipCache.m_Curve    = EditorGUILayout.CurveField("Ease", clipCache.m_Curve);
                    break;

                case DtSerializableSequence.DtAnimationClipType.Scale:
                    clipCache.m_Delay           = EditorGUILayout.FloatField("Delay", clipCache.m_Delay);
                    clipCache.m_Loop            = EditorGUILayout.IntField("Loop", clipCache.m_Loop);
                    clipCache.m_Duration        = EditorGUILayout.FloatField("Duration", clipCache.m_Duration);
                    clipCache.m_UniformScale    = EditorGUILayout.Toggle("Uniform Scale", clipCache.m_UniformScale);
                    if (clipCache.m_UniformScale)
                    {
                        clipCache.m_Scale       = EditorGUILayout.FloatField("Scale Target", clipCache.m_Scale);
                    }else
                    {
                        clipCache.m_Values      = EditorGUILayout.Vector3Field("Scale Target", clipCache.m_Values);
                    }
                    clipCache.m_Curve           = EditorGUILayout.CurveField("Ease", clipCache.m_Curve);
                    break;

                case DtSerializableSequence.DtAnimationClipType.Jump:
                    clipCache.m_Delay      = EditorGUILayout.FloatField("Delay", clipCache.m_Delay);
                    clipCache.m_Loop       = EditorGUILayout.IntField("Loop", clipCache.m_Loop);
                    clipCache.m_Duration   = EditorGUILayout.FloatField("Duration", clipCache.m_Duration);
                    clipCache.m_Values     = EditorGUILayout.Vector3Field("Jump Target", clipCache.m_Values);
                    clipCache.m_JumpPower  = EditorGUILayout.FloatField("Jump Power", clipCache.m_JumpPower);
                    clipCache.m_JumpNumber = EditorGUILayout.IntField("Jump Number", clipCache.m_JumpNumber);
                    clipCache.m_Snapping   = EditorGUILayout.Toggle("Snapping", clipCache.m_Snapping);
                    clipCache.m_Curve      = EditorGUILayout.CurveField("Ease", clipCache.m_Curve);
                    break;

                case DtSerializableSequence.DtAnimationClipType.LocalJump:
                    clipCache.m_Delay      = EditorGUILayout.FloatField("Delay", clipCache.m_Delay);
                    clipCache.m_Loop       = EditorGUILayout.IntField("Loop", clipCache.m_Loop);
                    clipCache.m_Duration   = EditorGUILayout.FloatField("Duration", clipCache.m_Duration);
                    clipCache.m_Values     = EditorGUILayout.Vector3Field("Jump Target", clipCache.m_Values);
                    clipCache.m_JumpPower  = EditorGUILayout.FloatField("Jump Power", clipCache.m_JumpPower);
                    clipCache.m_JumpNumber = EditorGUILayout.IntField("Jump Number", clipCache.m_JumpNumber);
                    clipCache.m_Snapping   = EditorGUILayout.Toggle("Snapping", clipCache.m_Snapping);
                    clipCache.m_Curve      = EditorGUILayout.CurveField("Ease", clipCache.m_Curve);
                    break;

                case DtSerializableSequence.DtAnimationClipType.ShakeLocation:
                    clipCache.m_Delay           = EditorGUILayout.FloatField("Delay", clipCache.m_Delay);
                    clipCache.m_Loop            = EditorGUILayout.IntField("Loop", clipCache.m_Loop);
                    clipCache.m_Duration        = EditorGUILayout.FloatField("Duration", clipCache.m_Duration);
                    clipCache.m_UniformStrength = EditorGUILayout.Toggle("Uniform Strength", clipCache.m_UniformStrength);
                    if(clipCache.m_UniformStrength)
                    {
                        clipCache.m_Strength    = EditorGUILayout.FloatField("Strength", clipCache.m_Strength);
                    }else
                    {
                        clipCache.m_Values      = EditorGUILayout.Vector3Field("Strength", clipCache.m_Values);
                    }
                    clipCache.m_Vibrato         = EditorGUILayout.IntField("Vibrato", clipCache.m_Vibrato);
                    clipCache.m_Randomness      = EditorGUILayout.FloatField("Randomness", clipCache.m_Randomness);
                    clipCache.m_Snapping        = EditorGUILayout.Toggle("Snapping", clipCache.m_Snapping);
                    clipCache.m_Fadeout         = EditorGUILayout.Toggle("Fadeout", clipCache.m_Fadeout);
                    clipCache.m_Curve           = EditorGUILayout.CurveField("Ease", clipCache.m_Curve);
                    break;

                case DtSerializableSequence.DtAnimationClipType.ShakeRotation:
                    clipCache.m_Delay           = EditorGUILayout.FloatField("Delay", clipCache.m_Delay);
                    clipCache.m_Loop            = EditorGUILayout.IntField("Loop", clipCache.m_Loop);
                    clipCache.m_Duration        = EditorGUILayout.FloatField("Duration", clipCache.m_Duration);
                    clipCache.m_UniformStrength = EditorGUILayout.Toggle("Uniform Strength", clipCache.m_UniformStrength);
                    if (clipCache.m_UniformStrength)
                    {
                        clipCache.m_Strength    = EditorGUILayout.FloatField("Strength", clipCache.m_Strength);
                    }else
                    {
                        clipCache.m_Values      = EditorGUILayout.Vector3Field("Strength", clipCache.m_Values);
                    }
                    clipCache.m_Vibrato         = EditorGUILayout.IntField("Vibrato", clipCache.m_Vibrato);
                    clipCache.m_Randomness      = EditorGUILayout.FloatField("Randomness", clipCache.m_Randomness);
                    clipCache.m_Fadeout         = EditorGUILayout.Toggle("Fadeout", clipCache.m_Fadeout);
                    clipCache.m_Curve           = EditorGUILayout.CurveField("Ease", clipCache.m_Curve);
                    break;

                case DtSerializableSequence.DtAnimationClipType.ShakeScale:
                    clipCache.m_Delay           = EditorGUILayout.FloatField("Delay", clipCache.m_Delay);
                    clipCache.m_Loop            = EditorGUILayout.IntField("Loop", clipCache.m_Loop);
                    clipCache.m_Duration        = EditorGUILayout.FloatField("Duration", clipCache.m_Duration);
                    clipCache.m_UniformStrength = EditorGUILayout.Toggle("Uniform Strength", clipCache.m_UniformStrength);
                    if (clipCache.m_UniformStrength)
                    {
                        clipCache.m_Strength    = EditorGUILayout.FloatField("Strength", clipCache.m_Strength);
                    }else
                    {
                        clipCache.m_Values      = EditorGUILayout.Vector3Field("Strength", clipCache.m_Values);
                    }
                    clipCache.m_Vibrato         = EditorGUILayout.IntField("Vibrato", clipCache.m_Vibrato);
                    clipCache.m_Randomness      = EditorGUILayout.FloatField("Randomness", clipCache.m_Randomness);
                    clipCache.m_Fadeout         = EditorGUILayout.Toggle("Fadeout", clipCache.m_Fadeout);
                    clipCache.m_Curve           = EditorGUILayout.CurveField("Ease", clipCache.m_Curve);
                    break;

                case DtSerializableSequence.DtAnimationClipType.PunchLocation:
                    clipCache.m_Delay      = EditorGUILayout.FloatField("Delay", clipCache.m_Delay);
                    clipCache.m_Loop       = EditorGUILayout.IntField("Loop", clipCache.m_Loop);
                    clipCache.m_Duration   = EditorGUILayout.FloatField("Duration", clipCache.m_Duration);
                    clipCache.m_Values     = EditorGUILayout.Vector3Field("Punch", clipCache.m_Values);
                    clipCache.m_Vibrato    = EditorGUILayout.IntField("Vibrato", clipCache.m_Vibrato);
                    clipCache.m_Elasticity = EditorGUILayout.FloatField("Elasticity", clipCache.m_Elasticity);
                    clipCache.m_Snapping   = EditorGUILayout.Toggle("Snapping", clipCache.m_Snapping);
                    clipCache.m_Curve      = EditorGUILayout.CurveField("Ease", clipCache.m_Curve);
                    break;

                case DtSerializableSequence.DtAnimationClipType.PunchRotation:
                    clipCache.m_Delay      = EditorGUILayout.FloatField("Delay", clipCache.m_Delay);
                    clipCache.m_Loop       = EditorGUILayout.IntField("Loop", clipCache.m_Loop);
                    clipCache.m_Duration   = EditorGUILayout.FloatField("Duration", clipCache.m_Duration);
                    clipCache.m_Values     = EditorGUILayout.Vector3Field("Punch", clipCache.m_Values);
                    clipCache.m_Vibrato    = EditorGUILayout.IntField("Vibrato", clipCache.m_Vibrato);
                    clipCache.m_Elasticity = EditorGUILayout.FloatField("Elasticity", clipCache.m_Elasticity);
                    clipCache.m_Curve      = EditorGUILayout.CurveField("Ease", clipCache.m_Curve);
                    break;

                case DtSerializableSequence.DtAnimationClipType.PunchScale:
                    clipCache.m_Delay      = EditorGUILayout.FloatField("Delay", clipCache.m_Delay);
                    clipCache.m_Loop       = EditorGUILayout.IntField("Loop", clipCache.m_Loop);
                    clipCache.m_Duration   = EditorGUILayout.FloatField("Duration", clipCache.m_Duration);
                    clipCache.m_Values     = EditorGUILayout.Vector3Field("Punch", clipCache.m_Values);
                    clipCache.m_Vibrato    = EditorGUILayout.IntField("Vibrato", clipCache.m_Vibrato);
                    clipCache.m_Elasticity = EditorGUILayout.FloatField("Elasticity", clipCache.m_Elasticity);
                    clipCache.m_Curve      = EditorGUILayout.CurveField("Ease", clipCache.m_Curve);
                    break;
            }
        }
    }

    private void ClickAddSequence(DtAnimation ownGameObject)
    {
        ownGameObject.m_Sequences.Add(new DtSequence());
    }

    private void ClickClearAll(DtAnimation ownGameObject)
    {
        ownGameObject.m_Sequences.Clear();
        m_UnfoldSequence.Clear();
        m_UnfoldClips.Clear();
        m_EditingSequenceCache.Clear();
        m_PreviewSequence.Clear();
    }

    private void ClickApply(DtAnimation ownGameObject)
    {
        // Save GameObject
        EditorUtility.SetDirty(target);

        // Save Asset
        var Asset = DtAnimationManager.Instance.GetAsset();

        foreach (var cacheItem in m_EditingSequenceCache)
        {
            if(cacheItem.m_Key.Length > 0)
            {
                Asset.Data[cacheItem.m_Key] = cacheItem.DeepClone();
            }
        }

        DtAnimationManager.Instance.SaveToAsset();

        // Reload Cache
        m_EditingSequenceCache.Clear();
        /*
        for (int i = 0; i < m_EditingSequenceCache.Count; i++)
        {
            if (m_EditingSequenceCache[i].m_Key.Length > 0)
            {
                m_EditingSequenceCache[i] = null;
            }
        }
        */
    }
}
