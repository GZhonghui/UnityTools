#if UNITY_EDITOR

using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AD_WwiseTools
{
#if Art_Editor
    [UnityEditor.MenuItem("Wwise/Select Wwise Manager", false, (int)3)]
#endif
    public static void DoSelectWwiseImporter()
    {
        AD_WwiseImport[] wwiseImports = GameObject.FindObjectsOfType<AD_WwiseImport>();

        if (wwiseImports.Length == 1)
        {
            Selection.activeGameObject = wwiseImports[0].gameObject;
            wwiseImports[0].name = "WwiseImporter";

            // Move to Editor Scene
            var mainScene = SceneManager.GetActiveScene();

            for (int i = 0; i < SceneManager.sceneCount; i += 1)
            {
                var Scene = SceneManager.GetSceneAt(i);
                if (Scene.name.ToLower() == mainScene.name.ToLower() + "_editor")
                {
                    SceneManager.MoveGameObjectToScene(wwiseImports[0].gameObject, Scene);
                    break;
                }
            }
        }
        else if (wwiseImports.Length == 0)
        {
            // Try Create
            var prefabGuid = AssetDatabase.AssetPathToGUID(AD_WwiseManager.GetWwiseImporterPrefabPath());
            if (prefabGuid != null)
            {
                var Importer = AssetDatabase.LoadAssetAtPath(AD_WwiseManager.GetWwiseImporterPrefabPath(), typeof(GameObject));
                var Go = PrefabUtility.InstantiatePrefab(Importer);
                if (Go != null)
                {
                    DoSelectWwiseImporter();
                }
            }
            else
            {
                Debug.Log("Wwise Can't Load Importer to Scene: " + AD_WwiseManager.GetWwiseImporterPrefabPath());
            }
        }
        else
        {
            // Destroy
            for (int i = 1; i < wwiseImports.Length; i++)
            {
                // Edit Mode
                // GameObject.Destroy(wwiseImports[i].gameObject);
                GameObject.DestroyImmediate(wwiseImports[i].gameObject);
            }
            DoSelectWwiseImporter();
        }
    }

#if Art_Editor
    [UnityEditor.MenuItem("Wwise/Select Sound Root", false, (int)4)]
#endif
    public static void DoSelectSoundRoot()
    {
        if (GetSoundRootInActiveScene(out GameObject soundRoot, out string sceneName))
        {
            Selection.activeObject = soundRoot;
        }
    }

#if Art_Editor
    [UnityEditor.MenuItem("GameObject/Wwise/Create Sound Node", false, (int)0)]
#endif
    public static void DoCreateSoundNode()
    {
        var soundTarget = Selection.activeObject as GameObject;
        if (soundTarget == null) return;

        if (!GetSoundRootInActiveScene(out GameObject soundRoot, out string activeSceneName))
        {
            return;
        }

        var soundNode = new GameObject(soundTarget.name);
        soundNode.transform.position = soundTarget.transform.position;
        soundNode.transform.rotation = soundTarget.transform.rotation;
        soundNode.transform.parent = soundRoot.transform;

        Selection.activeObject = soundNode;

        soundNode.AddComponent<AD_WwiseEvent>();
    }

    private static bool GetSoundRootInActiveScene(out GameObject soundRoot, out string activeSceneName)
    {
        soundRoot = null;
        activeSceneName = null;

        var activeScene = SceneManager.GetActiveScene();
        if (activeScene.name.ToLower().EndsWith("_editor"))
        {
            return false;
        }

        activeSceneName = activeScene.name;

        soundRoot = GameObject.Find(activeScene.name + "/_sound");
        if (soundRoot == null)
        {
            return false;
        }

        return true;
    }

    private static void PackTreeNode(GameObject Node, LitJson.JsonWriter jW)
    {
        // A Node
        jW.WriteObjectStart();

        // Node's Property
        jW.WritePropertyName("Name");
        jW.Write(Node.name);

        jW.WritePropertyName("Transform"); // Transform Start
        jW.WriteObjectStart();

        jW.WritePropertyName("Position");
        jW.WriteArrayStart();
        jW.Write(Node.transform.position.x);
        jW.Write(Node.transform.position.y);
        jW.Write(Node.transform.position.z);
        jW.WriteArrayEnd();

        jW.WritePropertyName("Rotation");
        jW.WriteArrayStart();
        jW.Write(Node.transform.rotation.x);
        jW.Write(Node.transform.rotation.y);
        jW.Write(Node.transform.rotation.z);
        jW.WriteArrayEnd();

        jW.WriteObjectEnd(); // Transform End

        // AD_Event
        var eventComponent = Node.GetComponent<AD_WwiseEvent>();
        if (eventComponent != null)
        {
            jW.WritePropertyName("Event"); // Event Start
            jW.WriteArrayStart();

            SerializedObject So = new SerializedObject(eventComponent);
            var Events = So.FindProperty("m_Events");
            for (int i = 0; i < Events.arraySize; i += 1)
            {
                var eventData = Events.GetArrayElementAtIndex(i);
                if (eventData.FindPropertyRelative("m_eventName").stringValue.Length <= 0) continue;

                jW.WriteObjectStart();

                jW.WritePropertyName("LifeCycle");
                jW.Write(eventData.FindPropertyRelative("m_lifeCycle").intValue);

                jW.WritePropertyName("Name");
                jW.Write(eventData.FindPropertyRelative("m_eventName").stringValue);

                jW.WritePropertyName("Is3D");
                jW.Write(eventData.FindPropertyRelative("m_bIs3DSound").boolValue);

                jW.WriteObjectEnd();
            }

            jW.WriteArrayEnd(); // Event End
        }

        // AD_Bank
        var bankComponent = Node.GetComponent<AD_WwiseBank>();
        if (bankComponent != null)
        {
            SerializedObject So = new SerializedObject(bankComponent);

            var bankLifeCycle = So.FindProperty("m_LifeCycle");
            var bankList = So.FindProperty("m_LoadBanks");

            jW.WritePropertyName("Bank");
            jW.WriteObjectStart(); // Bank Start

            jW.WritePropertyName("LifeCycle");
            jW.Write(bankLifeCycle.intValue);

            jW.WritePropertyName("NameList");
            jW.WriteArrayStart();
            for (int i = 0; i < bankList.arraySize; i += 1)
            {
                if (bankList.GetArrayElementAtIndex(i).stringValue.Length <= 0) continue;
                jW.Write(bankList.GetArrayElementAtIndex(i).stringValue);
            }
            jW.WriteArrayEnd();

            jW.WriteObjectEnd(); // Bank End
        }

        // Collider
        jW.WritePropertyName("Collider");
        jW.WriteObjectStart(); // Collider Start

        // Box
        var boxColliders = Node.GetComponents<BoxCollider>();
        if (boxColliders.Length > 0)
        {
            jW.WritePropertyName("Box");
            jW.WriteArrayStart();

            for (int i = 0; i < boxColliders.Length; i += 1)
            {
                jW.WriteObjectStart();

                jW.WritePropertyName("Center");
                jW.WriteArrayStart();
                jW.Write(boxColliders[i].center.x);
                jW.Write(boxColliders[i].center.y);
                jW.Write(boxColliders[i].center.z);
                jW.WriteArrayEnd();

                jW.WritePropertyName("Size");
                jW.WriteArrayStart();
                jW.Write(boxColliders[i].size.x);
                jW.Write(boxColliders[i].size.y);
                jW.Write(boxColliders[i].size.z);
                jW.WriteArrayEnd();

                jW.WriteObjectEnd();
            }

            jW.WriteArrayEnd();
        }

        // Sphere
        var sphereColliders = Node.GetComponents<SphereCollider>();
        if (sphereColliders.Length > 0)
        {
            jW.WritePropertyName("Sphere");
            jW.WriteArrayStart();

            for (int i = 0; i < sphereColliders.Length; i += 1)
            {
                jW.WriteObjectStart();

                jW.WritePropertyName("Center");
                jW.WriteArrayStart();
                jW.Write(sphereColliders[i].center.x);
                jW.Write(sphereColliders[i].center.y);
                jW.Write(sphereColliders[i].center.z);
                jW.WriteArrayEnd();

                jW.WritePropertyName("Radius");
                jW.Write(sphereColliders[i].radius);

                jW.WriteObjectEnd();
            }

            jW.WriteArrayEnd();
        }
        jW.WriteObjectEnd(); // Collider End

        // Children
        jW.WritePropertyName("Child");
        jW.WriteArrayStart(); // Children Start

        foreach (Transform childNode in Node.transform)
        {
            PackTreeNode(childNode.gameObject, jW);
        }

        jW.WriteArrayEnd(); // Children End

        jW.WriteObjectEnd();
    }

    // Json
    private static void WriteJsonToFile(string mapName, string jsonData)
    {
        string jsonFilePath = AD_WwiseManager.GetMapAmbientPath(mapName);

        if (!System.IO.File.Exists(jsonFilePath)) System.IO.File.Create(jsonFilePath);

        System.IO.File.WriteAllText(jsonFilePath, jsonData);
    }

    // YAML

#if Art_Editor
    [UnityEditor.MenuItem("Wwise/Export Map Ambient", false, (int)5)]
#endif
    public static void DoExportMapAmbient()
    {
        if (!GetSoundRootInActiveScene(out GameObject soundRoot, out string activeSceneName))
        {
            return;
        }

        StringBuilder stringBuilder = new StringBuilder();
        LitJson.JsonWriter jsonWriter = new LitJson.JsonWriter(stringBuilder);

        if (EditorUtility.DisplayDialog("Wwise Export", $"Export Map Ambient for {activeSceneName}?", "Continue", "Cancel"))
        {
            // Parse Scene Hierarchy
            PackTreeNode(soundRoot.gameObject, jsonWriter);

            // Write to File
            WriteJsonToFile(activeSceneName, stringBuilder.ToString());

            EditorUtility.DisplayDialog("Wwise Export", $"Export Map Ambient for {activeSceneName} Successfully!", "Done");
        }
    }

#if Art_Editor
    [UnityEditor.MenuItem("Wwise/Import Map Ambient", false, (int)6)]
#endif
    public static void DoImportMapAmbient()
    {
        if (!GetSoundRootInActiveScene(out GameObject soundRoot, out string activeSceneName))
        {
            return;
        }

        if (EditorUtility.DisplayDialog("Wwise Import", $"Import Map Ambient for {activeSceneName} from Config File?", "Continue", "Cancel"))
        {
            AD_WwiseMapAmbient.LoadMapAmbient(soundRoot, activeSceneName, false);

            // Clean
            /*
            foreach (Transform Child in soundRoot.transform)
            {
                GameObject.DestroyImmediate(Child.gameObject);
            }
            */
        }
    }

#if Art_Editor
    [UnityEditor.MenuItem("Wwise/Show Source Sphere", false, (int)7)]
#endif
    public static void DoShowSourceSphere()
    {
        AD_WwiseMonoBase.m_bDrawGizmos = true;
    }

#if Art_Editor
    [UnityEditor.MenuItem("Wwise/Hide Source Sphere", false, (int)8)]
#endif
    public static void DoHideSourceSphere()
    {
        AD_WwiseMonoBase.m_bDrawGizmos = false;
    }

#if Art_Editor
    [UnityEditor.MenuItem("Wwise/Analyze Scene", false, (int)9)]
#endif
    public static void DoAnalyzeScene()
    {

    }
}

#endif