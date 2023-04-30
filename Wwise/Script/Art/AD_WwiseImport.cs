using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[UnityEngine.DefaultExecutionOrder(-200)]
public class AD_WwiseImport : MonoBehaviour
{
    public GameObject LocalPlayer;
    public GameObject AudioSource;

    private void Awake()
    {
        Debug.Log("Wwise Awake");
        InitWwise();
    }

#if Art_Editor
    [UnityEditor.MenuItem("Wwise/Init Wwise", false, (int)2)]
    [UnityEditor.Callbacks.DidReloadScripts] // Unity Hot Reload
#endif
    public static void DoInitWwise()
    {
        if (Application.isPlaying) return;

        var akIniter = GameObject.FindObjectOfType<AkInitializer>();
        if (akIniter == null)
        {
            Debug.Log("Wwise Can't Find AkInitializer in Scene");
            return;
        }

        // Only Need in Edit Mode
        AkInitializer.useCarefullyGetInstance = akIniter;

        InitWwise();
    }

    public static void InitWwise()
    {
        var akIniter = GameObject.FindObjectOfType<AkInitializer>();
        if (akIniter == null) return;

        akIniter.enabled = false;

        if (AkSoundEngine.IsInitialized())
        {
            Debug.Log("Wwise Closeing...");
            AkSoundEngine.StopAll();
            AK.Wwise.Unity.WwiseAddressables.AkAddressableBankManager.Instance.UnloadInitBank();
            AK.Wwise.Unity.WwiseAddressables.AkAddressableBankManager.Instance.UnloadAllBanks();
            AkSoundEngine.Term();
        }

        AD_WwiseManager.Instance.UninitWise();
        AD_WwiseManager.Instance.InitWwise(createWwise: false);

#if Art_Editor
        var mainCamera = FindObjectOfType<AD_WwiseListener>();
        var mainImporter = FindObjectOfType<AD_WwiseImport>();
        if (mainCamera != null && mainImporter != null)
        {
            if(mainImporter.LocalPlayer != null)
            {
                mainCamera.RegPlayer(mainImporter.LocalPlayer);
            }
        }
#endif

        akIniter.enabled = true; // OnEnable: Init Wwise
    }

    /*
    private void EditorApplication_playModeStateChanged(UnityEditor.PlayModeStateChange state)
    {
        switch(state)
        {
            case UnityEditor.PlayModeStateChange.ExitingEditMode:
                AD_WwiseManager.Instance.UninitWise();
                break;

            case UnityEditor.PlayModeStateChange.ExitingPlayMode:
                AD_WwiseManager.Instance.UninitWise();
                break;

            case UnityEditor.PlayModeStateChange.EnteredEditMode:
                AD_WwiseManager.Instance.InitWwise();
                break;

            case UnityEditor.PlayModeStateChange.EnteredPlayMode:
                AD_WwiseManager.Instance.InitWwise();
                break;
        }
    }
    */
}
