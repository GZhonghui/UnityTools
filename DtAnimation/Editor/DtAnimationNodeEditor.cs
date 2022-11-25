using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DtAnimationNode))]
public class DtAnimationNodeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        #region Title
        GUI.color = Color.green;
        GUILayout.Label("DoTween Animation for RoMeta:");
        GUI.color = Color.white;
        #endregion

        var Asset = DtAnimationManager.Instance.GetAsset();

        List<string> markToRemove = new List<string>();

        foreach (var Item in Asset.Data)
        {
            GUILayout.BeginHorizontal();

            GUI.color = Color.green;
            GUILayout.Label(Item.Key);
            GUILayout.FlexibleSpace();
            GUI.color = Color.red;
            if (GUILayout.Button("Remove"))
            {
                markToRemove.Add(Item.Key);
            }
            GUI.color = Color.white;

            GUILayout.EndHorizontal();
        }

        foreach (var Item in markToRemove)
        {
            if(Asset.Data.ContainsKey(Item))
            {
                Asset.Data.Remove(Item);
            }
        }

        if(markToRemove.Count > 0)
        {
            DtAnimationManager.Instance.SaveToAsset();
        }
    }
}
