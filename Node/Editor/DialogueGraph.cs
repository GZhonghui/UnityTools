using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEditor;

public class DialogueGraph : EditorWindow
{
    private DialogueGraphView m_GraphView;

    [MenuItem("Graph/Dialogue Editor")]
    public static void OpenDialogueViewWindow()
    {
        var Window = GetWindow<DialogueGraph>();
        Window.titleContent = new GUIContent("Dialogue Editor");
    }

    private void OnEnable()
    {
        ConstructGraphView();
        GenerateToolbar();
    }

    private void OnDisable()
    {
        if (m_GraphView != null) rootVisualElement.Remove(m_GraphView);
    } 

    private void ConstructGraphView()
    {
        m_GraphView = new DialogueGraphView
        {
            name = "Dialogue View"
        };
        m_GraphView.StretchToParentSize();
        rootVisualElement.Add(m_GraphView);
    }

    private void GenerateToolbar()
    {
        // UnityEditor.UIElements
        var Bar = new Toolbar();

        var nodeCreateButton = new Button(() =>
        {
            m_GraphView?.CreateNode("Dialogue Node");
        });
        nodeCreateButton.text = "Create Node";
        Bar.Add(nodeCreateButton);

        rootVisualElement.Add(Bar);
    }
}
