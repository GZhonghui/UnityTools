using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

#if UNITY_EDITOR
using UnityEditor; // For Android
#endif

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
    public static string DTANIM_FILEPATH { get { return Application.dataPath + "/ArtRes/Bundle/UI/DtAnimation/DtAnimation.asset"; } }
    public static string DTANIM_ASSETPATH { get { return "Assets/ArtRes/Bundle/UI/DtAnimation/DtAnimation.asset"; } }
    public static string DTANIM_ADDRESSABLE { get { return "DtAnimation.asset"; } }

    protected DtAnimationAsset m_DtSequenceAsset = null;
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
                    m_DtSequenceAsset = Addressables.LoadAssetAsync<DtAnimationAsset>(DTANIM_ADDRESSABLE).WaitForCompletion();
                }
                else
                {
                    m_DtSequenceAsset = AssetDatabase.LoadAssetAtPath<DtAnimationAsset>(DTANIM_ASSETPATH);
                }
#else
                m_DtSequenceAsset = Addressables.LoadAssetAsync<DtAnimationAsset>(DTANIM_ADDRESSABLE).WaitForCompletion();
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

        if(!AssetDatabase.Contains(m_DtSequenceAsset))
        {
            AssetDatabase.CreateAsset(m_DtSequenceAsset, DTANIM_ASSETPATH);
        }

        EditorUtility.SetDirty(m_DtSequenceAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
#endif

    public DtAnimationAsset GetAsset()
    {
        return m_DtSequenceAsset;
    }
}
