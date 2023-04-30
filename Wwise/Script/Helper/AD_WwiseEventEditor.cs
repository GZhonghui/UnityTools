#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[UnityEditor.CustomEditor(typeof(AD_WwiseEvent))]
public class AD_WwiseEventEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        var ownGo = target as AD_WwiseEvent;

        serializedObject.Update();
        var eventsProperty = serializedObject.FindProperty("m_Events");
        var eventsCount = eventsProperty.arraySize;

        // Helper
        EditorGUILayout.HelpBox("The Radius Value is Only For Preview", MessageType.Info);
        ownGo.m_fGizmosRadius = EditorGUILayout.Slider("Radius", ownGo.m_fGizmosRadius, 0.5f, 50.0f);

        var triggerTypeStr = new string[]
        {
            "None",
            "Component Start Destroy",
            "Component Start",
            "Component Destroy",
            "Trigger Enter Exit",
            "Trigger Enter",
            "Trigger Exit"
        };

        var triggerType = new AD_WwiseEvent.LifeCycleType[]
        {
            AD_WwiseEvent.LifeCycleType.None,
            AD_WwiseEvent.LifeCycleType.ComponentStartDestroy,
            AD_WwiseEvent.LifeCycleType.ComponentStart,
            AD_WwiseEvent.LifeCycleType.ComponentDestroy,
            AD_WwiseEvent.LifeCycleType.TriggerEnterExit,
            AD_WwiseEvent.LifeCycleType.TriggerEnter,
            AD_WwiseEvent.LifeCycleType.TriggerExit
        };

        var triggerTypeInv = new Dictionary<AD_WwiseEvent.LifeCycleType, int>();
        for (int i = 0; i < triggerType.Length; i += 1)
        {
            triggerTypeInv[triggerType[i]] = i;
        }

        // Add & Remove
        GUILayout.BeginHorizontal();
        GUI.color = Color.green;
        bool clickedAdd = GUILayout.Button("Add");
        GUI.color = Color.red;
        bool clickedRemove = GUILayout.Button("Remove");
        GUI.color = Color.white;
        GUILayout.EndHorizontal();

        for (int i = 0; i < eventsCount; i++)
        {
            var eventData = eventsProperty.GetArrayElementAtIndex(i);
            var eventLifeCycle = eventData.FindPropertyRelative("m_lifeCycle");
            var eventName = eventData.FindPropertyRelative("m_eventName");
            var eventIs3DSound = eventData.FindPropertyRelative("m_bIs3DSound");

            GUILayout.BeginVertical("HelpBox");

            GUILayout.Label($"Event# {i}");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Trigger", GUILayout.Width(72));

            // Trigger
            int selectIndex = 0;
            if (triggerTypeInv.ContainsKey((AD_WwiseMonoBase.LifeCycleType)eventLifeCycle.intValue))
            {
                selectIndex = triggerTypeInv[(AD_WwiseMonoBase.LifeCycleType)eventLifeCycle.intValue];
            }

            selectIndex = UnityEditor.EditorGUILayout.Popup(selectIndex, triggerTypeStr);

            if (selectIndex >= 0 && selectIndex < triggerType.Length)
            {
                eventLifeCycle.intValue = (int)triggerType[selectIndex];
            }

            GUILayout.EndHorizontal();

            // Need a Collider
            if (eventLifeCycle.intValue >= (int)AD_WwiseMonoBase.LifeCycleType.TriggerEnterExit
                && eventLifeCycle.intValue <= (int)AD_WwiseMonoBase.LifeCycleType.TriggerExit)
            {
                if (ownGo.GetComponent<Collider>() == null)
                {
                    // Default
                    var bC = ownGo.gameObject.AddComponent<BoxCollider>();
                    bC.isTrigger = true;
                }

                // Set isTrigger
                var cS = ownGo.GetComponents<Collider>();
                for (int j = 0; j < cS.Length; j += 1)
                {
                    if (cS[j].GetType() != typeof(BoxCollider) && cS[j].GetType() != typeof(SphereCollider))
                    {
                        DestroyImmediate(cS[j]);
                        continue;
                    }
                    cS[j].isTrigger = true;
                }
            }

            int processIndex = i;
            bool clicked3D = WwiseEventPicker.WwiseEventProperty(eventName.stringValue, "Select", (selectedEventName) =>
                {
                    serializedObject.Update();

                    var eventsProperty = serializedObject.FindProperty("m_Events");
                    if (processIndex < eventsProperty.arraySize)
                    {
                        var eventSelectionProperty = eventsProperty.GetArrayElementAtIndex(processIndex);
                        if (eventSelectionProperty != null)
                        {
                            eventSelectionProperty.FindPropertyRelative("m_eventName").stringValue = selectedEventName;
                        }
                    }

                    if (serializedObject.hasModifiedProperties)
                    {
                        serializedObject.ApplyModifiedProperties();
                    }
                },
                eventIs3DSound.boolValue
            );

            if (clicked3D)
            {
                eventIs3DSound.boolValue = !eventIs3DSound.boolValue;
            }

            GUILayout.EndVertical();
        }

        if (clickedAdd)
        {
            eventsProperty.InsertArrayElementAtIndex(eventsCount);
            var eventData = eventsProperty.GetArrayElementAtIndex(eventsCount);
            eventData.FindPropertyRelative("m_lifeCycle").intValue = (int)AD_WwiseEvent.LifeCycleType.ComponentStartDestroy; // Default
            eventData.FindPropertyRelative("m_eventName").stringValue = "";
            eventData.FindPropertyRelative("m_bIs3DSound").boolValue = true;
        }

        if (clickedRemove)
        {
            if (eventsCount > 0)
            {
                eventsProperty.DeleteArrayElementAtIndex(eventsCount - 1);
            }
        }

        if (serializedObject.hasModifiedProperties)
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif // UNITY_EDITOR