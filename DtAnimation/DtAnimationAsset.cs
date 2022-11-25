using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class DtSerializableSequence
{
    public enum DtAnimationClipType
    {
        None,

        Move,
        LocalMove,

        Rotate,
        LocalRotate,

        Scale,

        Jump,
        LocalJump,

        ShakeLocation,
        ShakeRotation,
        ShakeScale,

        PunchLocation,
        PunchRotation,
        PunchScale,
    }

    public static string[] DtAnimationClipTypeText =
    {
        "None",

        "Move",
        "LocalMove",

        "Rotate",
        "LocalRotate",

        "Scale",

        "Jump",
        "LocalJump",

        "ShakeLocation",
        "ShakeRotation",
        "ShakeScale",

        "PunchLocation",
        "PunchRotation",
        "PunchScale",
    };

    [System.Serializable]
    public class DtSerializableClip
    {
        public DtAnimationClipType m_ClipType = DtAnimationClipType.None;
        // public List<int>   m_IntValues   = new List<int>();
        // public List<float> m_FloatValues = new List<float>();

        public bool m_To              = true;
        public bool m_Snapping        = true;
        public bool m_Fadeout         = true;
        public bool m_UniformStrength = true;
        public bool m_UniformScale    = true;

        public int m_Loop       = 0;
        public int m_Vibrato    = 0;
        public int m_JumpNumber = 0;

        public float m_Delay      = 0;
        public float m_Duration   = 0;
        public float m_Strength   = 0;
        public float m_Scale      = 0;
        public float m_Randomness = 0;
        public float m_Elasticity = 0;
        public float m_JumpPower  = 0;

        public Vector3 m_Values = new Vector3();

        public AnimationCurve m_Curve = new AnimationCurve();

        // Reset By Clip Type
        public void ResetValues(DtAnimationClipType resetType)
        {            
            switch (resetType)
            {
                case DtAnimationClipType.Move:
                    m_Delay    = 0;
                    m_Loop     = 1;
                    m_Duration = 1;
                    m_Values   = new Vector3(0, 0, 0);
                    m_Snapping = false;
                    m_Curve    = AnimationCurve.Linear(0, 0, 1, 1);
                    break;

                case DtAnimationClipType.LocalMove:
                    m_Delay    = 0;
                    m_Loop     = 1;
                    m_Duration = 1;
                    m_Values   = new Vector3(0, 0, 0);
                    m_Snapping = false;
                    m_Curve    = AnimationCurve.Linear(0, 0, 1, 1);
                    break;

                case DtAnimationClipType.Rotate:
                    m_Delay    = 0;
                    m_Loop     = 1;
                    m_Duration = 1;
                    m_Values   = new Vector3(0, 0, 0);
                    m_Curve    = AnimationCurve.Linear(0, 0, 1, 1);
                    break;

                case DtAnimationClipType.LocalRotate:
                    m_Delay    = 0;
                    m_Loop     = 1;
                    m_Duration = 1;
                    m_Values   = new Vector3(0, 0, 0);
                    m_Curve    = AnimationCurve.Linear(0, 0, 1, 1);
                    break;

                case DtAnimationClipType.Scale:
                    m_Delay        = 0;
                    m_Loop         = 1;
                    m_Duration     = 1;
                    m_UniformScale = true;
                    m_Scale        = 1;
                    m_Values       = new Vector3(1, 1, 1);
                    m_Curve        = AnimationCurve.Linear(0, 0, 1, 1);
                    break;

                case DtAnimationClipType.Jump:
                    m_Delay      = 0;
                    m_Loop       = 1;
                    m_Duration   = 1;
                    m_Values     = new Vector3(0, 0, 0);
                    m_JumpPower  = 25;
                    m_JumpNumber = 3;
                    m_Snapping   = false;
                    m_Curve      = AnimationCurve.Linear(0, 0, 1, 1);
                    break;

                case DtAnimationClipType.LocalJump:
                    m_Delay      = 0;
                    m_Loop       = 1;
                    m_Duration   = 1;
                    m_Values     = new Vector3(0, 0, 0);
                    m_JumpPower  = 25;
                    m_JumpNumber = 3;
                    m_Snapping   = false;
                    m_Curve      = AnimationCurve.Linear(0, 0, 1, 1);
                    break;

                case DtAnimationClipType.ShakeLocation:
                    m_Delay           = 0;
                    m_Loop            = 1;
                    m_Duration        = 1;
                    m_UniformStrength = true;
                    m_Strength        = 1;
                    m_Values          = new Vector3(1, 1, 1);
                    m_Vibrato         = 10;
                    m_Randomness      = 90;
                    m_Snapping        = false;
                    m_Fadeout         = true;
                    m_Curve           = AnimationCurve.Linear(0, 0, 1, 1);
                    break;

                case DtAnimationClipType.ShakeRotation:
                    m_Delay           = 0;
                    m_Loop            = 1;
                    m_Duration        = 1;
                    m_UniformStrength = true;
                    m_Strength        = 90;
                    m_Values          = new Vector3(90, 90, 90);
                    m_Vibrato         = 10;
                    m_Randomness      = 90;
                    m_Fadeout         = true;
                    m_Curve           = AnimationCurve.Linear(0, 0, 1, 1);
                    break;

                case DtAnimationClipType.ShakeScale:
                    m_Delay           = 0;
                    m_Loop            = 1;
                    m_Duration        = 1;
                    m_UniformStrength = true;
                    m_Strength        = 1;
                    m_Values          = new Vector3(1, 1, 1);
                    m_Vibrato         = 10;
                    m_Randomness      = 90;
                    m_Fadeout         = true;
                    m_Curve           = AnimationCurve.Linear(0, 0, 1, 1);
                    break;

                case DtAnimationClipType.PunchLocation:
                    m_Delay      = 0;
                    m_Loop       = 1;
                    m_Duration   = 1;
                    m_Values     = new Vector3(1, 1, 1);
                    m_Vibrato    = 10;
                    m_Elasticity = 1;
                    m_Snapping   = false;
                    m_Curve      = AnimationCurve.Linear(0, 0, 1, 1);
                    break;

                case DtAnimationClipType.PunchRotation:
                    m_Delay      = 0;
                    m_Loop       = 1;
                    m_Duration   = 1;
                    m_Values     = new Vector3(1, 1, 1);
                    m_Vibrato    = 10;
                    m_Elasticity = 1;
                    m_Curve      = AnimationCurve.Linear(0, 0, 1, 1);
                    break;

                case DtAnimationClipType.PunchScale:
                    m_Delay      = 0;
                    m_Loop       = 1;
                    m_Duration   = 1;
                    m_Values     = new Vector3(1, 1, 1);
                    m_Vibrato    = 10;
                    m_Elasticity = 1;
                    m_Curve      = AnimationCurve.Linear(0, 0, 1, 1);
                    break;
            }
        }

        public Tween CreateTweener(Transform Tran)
        {
            switch (m_ClipType)
            {
                case DtAnimationClipType.Move:
                    return Tran.DOMove(m_Values, m_Duration, m_Snapping).SetDelay(m_Delay).SetLoops(m_Loop).SetEase(m_Curve).Pause();

                case DtAnimationClipType.LocalMove:
                    return Tran.DOLocalMove(m_Values, m_Duration, m_Snapping).SetDelay(m_Delay).SetLoops(m_Loop).SetEase(m_Curve).Pause();

                case DtAnimationClipType.Rotate:
                    return Tran.DORotate(m_Values, m_Duration).SetDelay(m_Delay).SetLoops(m_Loop).SetEase(m_Curve).Pause();

                case DtAnimationClipType.LocalRotate:
                    return Tran.DOLocalRotate(m_Values, m_Duration).SetDelay(m_Delay).SetLoops(m_Loop).SetEase(m_Curve).Pause();

                case DtAnimationClipType.Scale:
                    if (m_UniformScale)
                    {
                        return Tran.DOScale(m_Scale, m_Duration).SetDelay(m_Delay).SetLoops(m_Loop).SetEase(m_Curve).Pause();
                    }
                    else
                    {
                        return Tran.DOScale(m_Values, m_Duration).SetDelay(m_Delay).SetLoops(m_Loop).SetEase(m_Curve).Pause();
                    }

                case DtAnimationClipType.Jump:
                    return Tran.DOJump(
                        m_Values, m_JumpPower, m_JumpNumber, m_Duration, m_Snapping
                    ).SetDelay(m_Delay).SetLoops(m_Loop).SetEase(m_Curve).Pause();

                case DtAnimationClipType.LocalJump:
                    return Tran.DOLocalJump(
                        m_Values, m_JumpPower, m_JumpNumber, m_Duration, m_Snapping
                    ).SetDelay(m_Delay).SetLoops(m_Loop).SetEase(m_Curve).Pause();

                case DtAnimationClipType.ShakeLocation:
                    if (m_UniformStrength)
                    {
                        return Tran.DOShakePosition(
                            m_Duration, m_Strength, m_Vibrato, m_Randomness, m_Snapping, m_Fadeout
                        ).SetDelay(m_Delay).SetLoops(m_Loop).SetEase(m_Curve).Pause();
                    }
                    else
                    {
                        return Tran.DOShakePosition(
                            m_Duration, m_Values, m_Vibrato, m_Randomness, m_Snapping, m_Fadeout
                        ).SetDelay(m_Delay).SetLoops(m_Loop).SetEase(m_Curve).Pause();
                    }

                case DtAnimationClipType.ShakeRotation:
                    if (m_UniformStrength)
                    {
                        return Tran.DOShakeRotation(
                            m_Duration, m_Strength, m_Vibrato, m_Randomness, m_Fadeout
                        ).SetDelay(m_Delay).SetLoops(m_Loop).SetEase(m_Curve).Pause();
                    }
                    else
                    {
                        return Tran.DOShakeRotation(
                            m_Duration, m_Values, m_Vibrato, m_Randomness, m_Fadeout
                        ).SetDelay(m_Delay).SetLoops(m_Loop).SetEase(m_Curve).Pause();
                    }

                case DtAnimationClipType.ShakeScale:
                    if (m_UniformStrength)
                    {
                        return Tran.DOShakeScale(
                            m_Duration, m_Strength, m_Vibrato, m_Randomness, m_Fadeout
                        ).SetDelay(m_Delay).SetLoops(m_Loop).SetEase(m_Curve).Pause();
                    }
                    else
                    {
                        return Tran.DOShakeScale(
                            m_Duration, m_Values, m_Vibrato, m_Randomness, m_Fadeout
                        ).SetDelay(m_Delay).SetLoops(m_Loop).SetEase(m_Curve).Pause();
                    }

                case DtAnimationClipType.PunchLocation:
                    return Tran.DOPunchPosition(
                        m_Values, m_Duration, m_Vibrato, m_Elasticity, m_Snapping
                    ).SetDelay(m_Delay).SetLoops(m_Loop).SetEase(m_Curve).Pause();

                case DtAnimationClipType.PunchRotation:
                    return Tran.DOPunchRotation(
                        m_Values, m_Duration, m_Vibrato, m_Elasticity
                    ).SetDelay(m_Delay).SetLoops(m_Loop).SetEase(m_Curve).Pause();

                case DtAnimationClipType.PunchScale:
                    return Tran.DOPunchScale(
                        m_Values, m_Duration, m_Vibrato, m_Elasticity
                    ).SetDelay(m_Delay).SetLoops(m_Loop).SetEase(m_Curve).Pause();
            }
            return null;
        }
    }

    public string m_Key = "";
    public List<DtSerializableClip> m_Clips = new List<DtSerializableClip>();

    public DtSerializableClip AddClip(DtSerializableClip Clip)
    {
        m_Clips.Add(Clip); return Clip;
    }

    public DtSerializableClip GetClip(int Index)
    {
        if(m_Clips.Count > Index) return m_Clips[Index]; return null;
    }

    public void RemoveClip()
    {
        if(m_Clips.Count > 0) m_Clips.RemoveAt(m_Clips.Count - 1);
    }

    public Sequence CreateSequence(Transform Tran)
    {
        var Seq = DOTween.Sequence();

        for (int i = 0; i < m_Clips.Count; i += 1)
        {
            var Twe = m_Clips[i].CreateTweener(Tran);
            if (Twe != null) Seq.Join(Twe);
        }

        return Seq;
    }

    public DtSerializableClip DeepCloneClip(DtSerializableClip From)
    {
        DtSerializableClip To = new DtSerializableClip();

        To.m_ClipType = From.m_ClipType;

        To.m_To              = From.m_To;
        To.m_Snapping        = From.m_Snapping;
        To.m_Fadeout         = From.m_Fadeout;
        To.m_UniformStrength = From.m_UniformStrength;
        To.m_UniformScale    = From.m_UniformScale;

        To.m_Loop       = From.m_Loop;
        To.m_Vibrato    = From.m_Vibrato;
        To.m_JumpNumber = From.m_JumpNumber;

        To.m_Delay      = From.m_Delay;
        To.m_Duration   = From.m_Duration;
        To.m_Strength   = From.m_Strength;
        To.m_Scale      = From.m_Scale;
        To.m_Randomness = From.m_Randomness;
        To.m_Elasticity = From.m_Elasticity;
        To.m_JumpPower  = From.m_JumpPower;

        To.m_Values = new Vector3(From.m_Values.x, From.m_Values.y, From.m_Values.z);

        To.m_Curve = new AnimationCurve(From.m_Curve.keys);

        return To;
    }

    public DtSerializableSequence DeepClone()
    {
        DtSerializableSequence To = new DtSerializableSequence();

        To.m_Key = this.m_Key;
        To.m_Clips = new List<DtSerializableClip>();

        foreach (var Item in this.m_Clips)
        {
            To.m_Clips.Add(this.DeepCloneClip(Item));
        }

        return To;
    }

    /*
    // Doesnt Work for UnityEngine.xxx
    public static T DeepClone<T>(this T obj)
    {
        using (var ms = new MemoryStream())
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);
            ms.Position = 0;

            return (T)formatter.Deserialize(ms);
        }
    }
    */
}

[System.Serializable]
public class DtSerializationDict : ISerializationCallbackReceiver
{
    public List<string> _keys = new List<string>();
    public List<DtSerializableSequence> _values = new List<DtSerializableSequence>();

    public SortedDictionary<string, DtSerializableSequence> m_Dict = new SortedDictionary<string, DtSerializableSequence>();

    public void OnBeforeSerialize()
    {
        _keys.Clear();
        _values.Clear();

        foreach (var kvp in m_Dict)
        {
            _keys.Add(kvp.Key);
            _values.Add(kvp.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        m_Dict = new SortedDictionary<string, DtSerializableSequence>();

        for (int i = 0; i != System.Math.Min(_keys.Count, _values.Count); i++)
            m_Dict.Add(_keys[i], _values[i]);
    }
}

[System.Serializable]
public class DtAnimationAsset : ScriptableObject
{
    public DtSerializationDict m_SavedSequences = new DtSerializationDict();

    public SortedDictionary<string, DtSerializableSequence> Data
    {
        get
        {
            return m_SavedSequences.m_Dict;
        }
    }
}
