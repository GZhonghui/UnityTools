// Checked

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace DtAnimation
{
    [System.Serializable]
    public class DtSerializableSequence
    {
        public enum DtAnimationClipType
        {
            None = 0,

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

            Fade,
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

            "Fade"
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

            public AnimationCurve m_Curve = AnimationCurve.Linear(0, 0, 1, 1);

            // Reset By Clip Type
            public void ResetValues(DtAnimationClipType resetType)
            {
                // Default Values
                m_Delay    = 0;
                m_Loop     = 1;
                m_Duration = 1;
                m_Curve    = AnimationCurve.Linear(0, 0, 1, 1);

                m_Values     = new Vector3(1, 1, 1);
                m_Fadeout    = true;
                m_Snapping   = false;
                m_Vibrato    = 10;
                m_Randomness = 90;
                m_Elasticity = 1;
                m_Strength   = 1;
                m_Scale      = 1;

                m_JumpPower  = 25;
                m_JumpNumber = 3;

                m_UniformScale    = true;
                m_UniformStrength = true;

                switch (resetType)
                {
                    case DtAnimationClipType.Move:
                        m_Values = new Vector3(0, 0, 0);
                        break;

                    case DtAnimationClipType.LocalMove:
                        m_Values = new Vector3(0, 0, 0);
                        break;

                    case DtAnimationClipType.Rotate:
                        m_Values = new Vector3(0, 0, 0);
                        break;

                    case DtAnimationClipType.LocalRotate:
                        m_Values = new Vector3(0, 0, 0);
                        break;

                    case DtAnimationClipType.Scale:
                        break;

                    case DtAnimationClipType.Jump:
                        m_Values = new Vector3(0, 0, 0);
                        break;

                    case DtAnimationClipType.LocalJump:
                        m_Values = new Vector3(0, 0, 0);
                        break;

                    case DtAnimationClipType.ShakeLocation:
                        break;

                    case DtAnimationClipType.ShakeRotation:
                        m_Strength = 90;
                        m_Values   = new Vector3(90, 90, 90);
                        break;

                    case DtAnimationClipType.ShakeScale:
                        break;

                    case DtAnimationClipType.PunchLocation:
                        break;

                    case DtAnimationClipType.PunchRotation:
                        break;

                    case DtAnimationClipType.PunchScale:
                        break;
                }
            }

            public Tween CreateTween(Transform Tran)
            {
                System.Func<Tween, Tween> addDelayLoopEaseAndPause = (Anim) =>
                {
                    return Anim.SetDelay(m_Delay).SetLoops(m_Loop).SetEase(m_Curve).Pause();
                };

                switch (m_ClipType)
                {
                    case DtAnimationClipType.Move:
                        return addDelayLoopEaseAndPause(Tran.DOMove(m_Values, m_Duration, m_Snapping));

                    case DtAnimationClipType.LocalMove:
                        return addDelayLoopEaseAndPause(Tran.DOLocalMove(m_Values, m_Duration, m_Snapping));

                    case DtAnimationClipType.Rotate:
                        return addDelayLoopEaseAndPause(Tran.DORotate(m_Values, m_Duration));

                    case DtAnimationClipType.LocalRotate:
                        return addDelayLoopEaseAndPause(Tran.DOLocalRotate(m_Values, m_Duration));

                    case DtAnimationClipType.Scale:
                        return addDelayLoopEaseAndPause(
                            m_UniformScale ? Tran.DOScale(m_Scale, m_Duration) : Tran.DOScale(m_Values, m_Duration)
                        );

                    case DtAnimationClipType.Jump:
                        return addDelayLoopEaseAndPause(Tran.DOJump(
                            m_Values, m_JumpPower, m_JumpNumber, m_Duration, m_Snapping
                        ));

                    case DtAnimationClipType.LocalJump:
                        return addDelayLoopEaseAndPause(Tran.DOLocalJump(
                            m_Values, m_JumpPower, m_JumpNumber, m_Duration, m_Snapping
                        ));

                    case DtAnimationClipType.ShakeLocation:
                        return addDelayLoopEaseAndPause(m_UniformStrength ?
                            Tran.DOShakePosition(m_Duration, m_Strength, m_Vibrato, m_Randomness, m_Snapping, m_Fadeout) :
                            Tran.DOShakePosition(m_Duration, m_Values, m_Vibrato, m_Randomness, m_Snapping, m_Fadeout)
                        );

                    case DtAnimationClipType.ShakeRotation:
                        return addDelayLoopEaseAndPause(m_UniformStrength ?
                            Tran.DOShakeRotation(m_Duration, m_Strength, m_Vibrato, m_Randomness, m_Fadeout) :
                            Tran.DOShakeRotation(m_Duration, m_Values, m_Vibrato, m_Randomness, m_Fadeout)
                        );

                    case DtAnimationClipType.ShakeScale:
                        return addDelayLoopEaseAndPause(m_UniformStrength ?
                            Tran.DOShakeScale(m_Duration, m_Strength, m_Vibrato, m_Randomness, m_Fadeout) :
                            Tran.DOShakeScale(m_Duration, m_Values, m_Vibrato, m_Randomness, m_Fadeout)
                        );

                    case DtAnimationClipType.PunchLocation:
                        return addDelayLoopEaseAndPause(Tran.DOPunchPosition(
                            m_Values, m_Duration, m_Vibrato, m_Elasticity, m_Snapping
                        ));

                    case DtAnimationClipType.PunchRotation:
                        return addDelayLoopEaseAndPause(Tran.DOPunchRotation(
                            m_Values, m_Duration, m_Vibrato, m_Elasticity
                        ));

                    case DtAnimationClipType.PunchScale:
                        return addDelayLoopEaseAndPause(Tran.DOPunchScale(
                            m_Values, m_Duration, m_Vibrato, m_Elasticity
                        ));
                }
                return null;
            }
        }

        [SerializeField]
        public int m_ReferenceCount = 0;
        
        [SerializeField]
        public string m_Key = "";

        [SerializeField]
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
                var Twe = m_Clips[i].CreateTween(Tran);
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
    }

    [System.Serializable]
    public class DtAnimationAsset : ScriptableObject
    {
        [SerializeField]
        public DtSerializationDict<string, DtSerializableSequence> m_SavedSequences = new DtSerializationDict<string, DtSerializableSequence>();

        public SortedDictionary<string, DtSerializableSequence> Data
        {
            get
            {
                return m_SavedSequences.m_Dict;
            }
        }
    }

    [System.Serializable]
    public class DtAnimationReference : ScriptableObject
    {
        [System.Serializable]
        public class RefList
        {
            [SerializeField]
            public List<DtObjectCount> dtObjectCounts = new List<DtObjectCount>();

            public List<DtObjectCount> Data
            {
                get
                {
                    return dtObjectCounts;
                }
            }
        }

        [SerializeField]
        private DtSerializationDict<string, RefList> m_ReferenceCount = new DtSerializationDict<string, RefList>();

        public SortedDictionary<string, RefList> Data
        {
            get
            {
                return m_ReferenceCount.m_Dict;
            }
        }
    }
} // namespace DtAnimation