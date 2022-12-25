// Checked

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DtAnimation
{
    [CustomEditor(typeof(DtAnimationRoot))]
    public class DtAnimationRootEditor : Editor
    {
        Dictionary<string, bool> Foldout = new Dictionary<string, bool>();

        public override void OnInspectorGUI()
        {
            DtAnimationRoot ownObject = target as DtAnimationRoot;

            serializedObject.Update();

            GUI.color = Color.green;
            if (GUILayout.Button("Refresh"))
            {
                ownObject.Refresh();
            }
            GUI.color = Color.white;

            foreach (var Key in ownObject.Data.Keys)
            {
                if (!Foldout.ContainsKey(Key)) Foldout.Add(Key, true);
                GUI.color = Color.green;
                Foldout[Key] = EditorGUILayout.Foldout(Foldout[Key], "Lua Export: " + Key);
                GUI.color = Color.white;

                // Game Objects
                if (Foldout[Key])
                {
                    for (int i = 0; i < ownObject.Data[Key].Data.Count; i++)
                    {
                        GUI.color = Color.yellow;
                        GUILayout.BeginHorizontal();
                        GameObject animGo = ownObject.Data[Key].Data[i];
                        GUILayout.Label("Game Object: " + animGo.name);

                        GUILayout.FlexibleSpace();

                        GUI.color = Color.white;
                        if (GUILayout.Button("Select"))
                        {
                            Selection.activeObject = animGo;
                        }

                        GUILayout.EndHorizontal();
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    } // class DtAnimationRootEditor
} // namespace DtAnimation