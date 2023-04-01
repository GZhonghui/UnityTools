// Checked

using UnityEngine;

namespace DtAnimation
{
    public class DtAnimationManager
    {
        #region Signleton
        protected static DtAnimationManager _instance = new DtAnimationManager();
        public static DtAnimationManager Instance
        {
            get
            {
                if (Application.isPlaying) _instance.RuntimeInit();
                else _instance.EditorInit();

                return _instance;
            }
        }
        #endregion

        #region Data
        protected DtAnimationAsset m_DtSequenceAsset = null;

        protected DtAnimationReference m_DtReferenceAsset = null;
        #endregion

        #region Path
        public static string DTANIM_FILEPATH { get { return Application.dataPath + "/ArtRes/Bundle/UI/DtAnimation/DtAnimation.asset"; } }
        public static string DTANIM_ASSETPATH { get { return "Assets/ArtRes/Bundle/UI/DtAnimation/DtAnimation.asset"; } }
        public static string DTANIM_ADDRESSABLE { get { return "DtAnimation.asset"; } }

        public static string DtReferenceFilePath { get { return Application.dataPath + "/ArtRes/Bundle/UI/DtAnimation/DtAnimationReference.asset"; } }
        public static string DtReferenceAssetPath { get { return "Assets/ArtRes/Bundle/UI/DtAnimation/DtAnimationReference.asset"; } }

        public static string UiPrefabPath { get { return "Assets/ArtRes/Bundle/UI/Prefab/"; } }
        #endregion

        private void EditorInit()
        {
            LoadAsset();
        }

        private void RuntimeInit()
        {
            LoadAsset();
        }

        private void LoadAsset()
        {
            if (m_DtSequenceAsset == null)
            {
                if (System.IO.File.Exists(DTANIM_FILEPATH))
                {
#if UNITY_EDITOR
                    if (Application.isPlaying)
                    {
                        // m_DtSequenceAsset = Addressables.LoadAssetAsync<DtAnimationAsset>(DTANIM_ADDRESSABLE).WaitForCompletion();
                        m_DtSequenceAsset = UT_ResourceLoadManager.Instance.LoadUObject(DTANIM_ADDRESSABLE) as DtAnimationAsset;
                    }
                    else
                    {
                        m_DtSequenceAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<DtAnimationAsset>(DTANIM_ASSETPATH);
                    }
#else
                    // m_DtSequenceAsset = Addressables.LoadAssetAsync<DtAnimationAsset>(DTANIM_ADDRESSABLE).WaitForCompletion();
                    m_DtSequenceAsset = UT_ResourceLoadManager.Instance.LoadUObject(DTANIM_ADDRESSABLE) as DtAnimationAsset;
#endif
                }
                else
                {
                    m_DtSequenceAsset = ScriptableObject.CreateInstance<DtAnimationAsset>();
                }
            }
        }

#if UNITY_EDITOR
        public void SaveToAsset()
        {
            if (m_DtSequenceAsset == null || Application.isPlaying) return;

            if (!UnityEditor.AssetDatabase.Contains(m_DtSequenceAsset))
            {
                UnityEditor.AssetDatabase.CreateAsset(m_DtSequenceAsset, DTANIM_ASSETPATH);
            }

            UnityEditor.EditorUtility.SetDirty(m_DtSequenceAsset);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }
#endif

        public DtAnimationAsset GetAsset()
        {
            return m_DtSequenceAsset;
        }


#if UNITY_EDITOR
        #region Reference
        public void SaveReference()
        {
            if (m_DtReferenceAsset == null) return;

            if (!UnityEditor.AssetDatabase.Contains(m_DtReferenceAsset))
            {
                UnityEditor.AssetDatabase.CreateAsset(m_DtReferenceAsset, DtReferenceAssetPath);
            }

            UnityEditor.EditorUtility.SetDirty(m_DtReferenceAsset);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }

        public void GetReference(out DtAnimationReference Data)
        {
            Data = null;

            if (Application.isPlaying) return;

            if (m_DtReferenceAsset == null)
            {
                if (System.IO.File.Exists(DtReferenceFilePath))
                {
                    m_DtReferenceAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<DtAnimationReference>(DtReferenceAssetPath);
                }
                else
                {
                    m_DtReferenceAsset = ScriptableObject.CreateInstance<DtAnimationReference>();
                }
            }

            Data = m_DtReferenceAsset;
        }
        #endregion
#endif
    }
}