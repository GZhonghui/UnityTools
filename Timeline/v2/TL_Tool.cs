#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

public static class TL_Tool
{
    private static bool Check() { return UnityEngine.Application.isPlaying; }

    [UnityEditor.MenuItem("Timeline/Select Timeline Editor #T", false, (int)0)]
    public static void DoSelectTimelineEditor()
    {
        if (Check()) return;

        var kEditorGo = TL_Utility.FindMainTimelineEditorInScene();
        UnityEditor.Selection.activeObject = kEditorGo;
    }

    [UnityEditor.MenuItem("Timeline/Enable Timeline Camera", false, (int)1)]
    public static void DoEnableTimelineCamera()
    {
        if (Check()) return;
    }

    [UnityEditor.MenuItem("Timeline/Disable Timeline Camera", false, (int)2)]
    public static void DoDisableTimelineCamera()
    {
        if (Check()) return;
    }

    [UnityEditor.MenuItem("Timeline/Export Timeline Binding", false, (int)3)]
    public static void DoExportTimelineBinding()
    {
        if (Check()) return;

        // TODO
        UnityEditor.Selection.activeObject = null; // 取消选择Director，还原时间轴到初始状态

        var kEditorGo = TL_Utility.FindMainTimelineEditorInScene();
        TL_AssetHandler.Export(kEditorGo);
    }

    [UnityEditor.MenuItem("Timeline/Import Timeline Binding", false, (int)4)]
    public static void DoImportTimelineBinding()
    {
        if (Check()) return;
    }
}

#endif