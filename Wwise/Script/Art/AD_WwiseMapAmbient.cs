// 读取数据恢复场景的时候，有的地方使用了SerializedObject，有的地方是直接Set，考虑一下是否需要使用SerializedObject

using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class AD_WwiseMapAmbient
{
    private static void BuildTreeNode(GameObject Node, LitJson.JsonData jD, bool isRuntime)
    {
        Func<LitJson.JsonData, float> Convert = (jsonData) =>
        {
            double Result = 0;

            if (jsonData.IsInt) Result = (int)jsonData;
            if (jsonData.IsDouble) Result = (double)jsonData;

            return (float)Result;
        };

        if (jD.ContainsKey("Name"))
        {
            Node.name = (string)jD["Name"];
        }

        if (jD.ContainsKey("Transform"))
        {
            var transformData = jD["Transform"];

            Node.transform.position = new Vector3(
                Convert(transformData["Position"][0]),
                Convert(transformData["Position"][1]),
                Convert(transformData["Position"][2])
            );

            Node.transform.rotation = Quaternion.Euler(
                Convert(transformData["Rotation"][0]),
                Convert(transformData["Rotation"][1]),
                Convert(transformData["Rotation"][2])
            );
        }

        if (jD.ContainsKey("Event"))
        {
            var eventList = jD["Event"];

            var eventComponent = Node.AddComponent<AD_WwiseEvent>();

            if (isRuntime)
            {
                eventComponent.m_Events = new System.Collections.Generic.List<AD_WwiseEvent.AD_WwiseEventData>();
                for (int i = 0; i < eventList.Count; i += 1)
                {
                    var eventData = eventList[i];
                    var csEventData = new AD_WwiseEvent.AD_WwiseEventData();
                    csEventData.m_lifeCycle = (AD_WwiseMonoBase.LifeCycleType)((int)eventData["LifeCycle"]);
                    csEventData.m_eventName = (string)eventData["Name"];
                    csEventData.m_bIs3DSound = (bool)eventData["Is3D"];

                    eventComponent.m_Events.Add(csEventData);
                }
            }
            else
            {
#if UNITY_EDITOR
                SerializedObject So = new SerializedObject(eventComponent);

                So.Update();
                var eventSo = So.FindProperty("m_Events");
                for (int i = 0; i < eventList.Count; i += 1)
                {
                    var eventData = eventList[i];

                    eventSo.InsertArrayElementAtIndex(i);
                    var eventDataSo = eventSo.GetArrayElementAtIndex(i);

                    eventDataSo.FindPropertyRelative("m_lifeCycle").intValue = (int)eventData["LifeCycle"];
                    eventDataSo.FindPropertyRelative("m_eventName").stringValue = (string)eventData["Name"];
                    eventDataSo.FindPropertyRelative("m_bIs3DSound").boolValue = (bool)eventData["Is3D"];
                }
                So.ApplyModifiedProperties();
#endif
            }
        }

        if (jD.ContainsKey("Bank"))
        {
            var bankLifeCycle = jD["Bank"]["LifeCycle"];
            var bankNameList = jD["Bank"]["NameList"];

            var bankComponent = Node.AddComponent<AD_WwiseBank>();

            if (isRuntime)
            {
                bankComponent.m_LifeCycle = (AD_WwiseMonoBase.LifeCycleType)((int)bankLifeCycle);

                bankComponent.m_LoadBanks = new System.Collections.Generic.List<string>();
                for (int i = 0; i < bankNameList.Count; i += 1)
                {
                    var bankName = (string)bankNameList[i];

                    bankComponent.m_LoadBanks.Add(bankName);
                }
            }
            else
            {
#if UNITY_EDITOR
                SerializedObject So = new SerializedObject(bankComponent);

                So.Update();

                So.FindProperty("m_LifeCycle").intValue = (int)bankLifeCycle;
                var bankListSo = So.FindProperty("m_LoadBanks");

                for (int i = 0; i < bankNameList.Count; i += 1)
                {
                    var bankName = (string)bankNameList[i];

                    bankListSo.InsertArrayElementAtIndex(i);
                    bankListSo.GetArrayElementAtIndex(i).stringValue = bankName;
                }

                So.ApplyModifiedProperties();
#endif
            }
        }

        if (jD.ContainsKey("Collider"))
        {
            var jdCollider = jD["Collider"];

            if (jdCollider.ContainsKey("Box"))
            {
                var boxColliders = jdCollider["Box"];

                for (int i = 0; i < boxColliders.Count; i += 1)
                {
                    var boxCollider = Node.AddComponent<BoxCollider>();
                    boxCollider.isTrigger = true;
                    boxCollider.center = new Vector3(
                        Convert(boxColliders[i]["Center"][0]),
                        Convert(boxColliders[i]["Center"][1]),
                        Convert(boxColliders[i]["Center"][2])
                    );
                    boxCollider.size = new Vector3(
                        Convert(boxColliders[i]["Size"][0]),
                        Convert(boxColliders[i]["Size"][1]),
                        Convert(boxColliders[i]["Size"][2])
                    );
                }
            }

            if (jdCollider.ContainsKey("Sphere"))
            {
                var sphereColliders = jdCollider["Sphere"];

                for (int i = 0; i < sphereColliders.Count; i += 1)
                {
                    var sphereCollider = Node.AddComponent<SphereCollider>();
                    sphereCollider.isTrigger = true;
                    sphereCollider.center = new Vector3(
                        Convert(sphereColliders[i]["Center"][0]),
                        Convert(sphereColliders[i]["Center"][1]),
                        Convert(sphereColliders[i]["Center"][2])
                    );
                    sphereCollider.radius = Convert(sphereColliders[i]["Radius"]);
                }
            }
        }

        if (jD.ContainsKey("Child"))
        {
            var childData = jD["Child"];

            for (int i = 0; i < childData.Count; i += 1)
            {
                var childNode = new GameObject();
                var childNodeData = childData[i];

                // Append Child
                childNode.transform.parent = Node.transform;

                BuildTreeNode(childNode, childNodeData, isRuntime);
            }
        }
    }

    private static string ReadJsonFromFile(string mapName)
    {
        string jsonFilePath = AD_WwiseManager.GetMapAmbientPath(mapName);

        if (System.IO.File.Exists(jsonFilePath))
        {
            return System.IO.File.ReadAllText(jsonFilePath);
        }

        return null;
    }

    // Helper, Call at Runtime
    public static void LoadMapAmbient(GameObject soundRoot, string mapName, bool isRuntime)
    {
        var mapParent = soundRoot.transform.parent;
        if (mapParent != null)
        {
            // Clear Sound Node
            var Index = soundRoot.transform.GetSiblingIndex();
            GameObject.DestroyImmediate(soundRoot.gameObject);
            var importSoundRoot = new GameObject("_sound");
            importSoundRoot.transform.parent = mapParent;
            importSoundRoot.transform.SetSiblingIndex(Index);

            string jsonText = "";
            if (isRuntime)
            {
                // Load via Resource Manager
                UnityEngine.TextAsset jsonTextAsset = UT_ResourceLoadManager.Instance.LoadUObject(
                    AD_WwiseManager.GetMapAmbientAddressablePath(mapName)
                ) as UnityEngine.TextAsset;
                if (jsonTextAsset != null) jsonText = jsonTextAsset.text;
            }
            else
            {
                jsonText = ReadJsonFromFile(mapName);
            }

            if (!string.IsNullOrEmpty(jsonText))
            {
                LitJson.JsonData jsonData = LitJson.JsonMapper.ToObject(jsonText);

                if (jsonData != null)
                {
                    if (isRuntime) importSoundRoot.SetActive(false);
                    BuildTreeNode(importSoundRoot, jsonData, isRuntime);
                    if (isRuntime) importSoundRoot.SetActive(true);
                }
            }

            if (!isRuntime)
            {
#if UNITY_EDITOR
                EditorUtility.SetDirty(importSoundRoot.gameObject);
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();

                // Back
                AD_WwiseTools.DoSelectSoundRoot();
#endif
            }
        }
    }
}