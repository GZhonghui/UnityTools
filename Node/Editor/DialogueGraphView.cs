using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

public class DialogueGraphView : GraphView
{
    private readonly Vector2 defaultNodeSize = new Vector2(100, 150);

    public DialogueGraphView()
    {
        styleSheets.Add(Resources.Load<StyleSheet>("DialogueGraph"));
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        // UnityEngine.UIElements
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var gridBackground = new GridBackground();
        Insert(0, gridBackground);
        gridBackground.StretchToParentSize();

        // Add Entry Node
        AddElement(GenerateEntryPoint());
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();

        ports.ForEach((Port) =>
        {
            // TODO
            if (startPort != Port && startPort.node != Port.node && startPort.direction != Port.direction)
            {
                compatiblePorts.Add(Port);
            }
        });

        return compatiblePorts;
    }

    private Port GeneratePort(DialogueNode targetNode, Direction portDirection, Port.Capacity portCapacity = Port.Capacity.Single)
    {
        // Transform Data
        return targetNode.InstantiatePort(Orientation.Horizontal, portDirection, portCapacity, typeof(float));
    }

    private DialogueNode GenerateEntryPoint()
    {
        var Node = new DialogueNode
        {
            title = "Global Entry",
            m_Guid = Guid.NewGuid(),
            m_DialogueText = "Global Entry",
            m_EntryPoint = true
        };

        var Port = GeneratePort(Node, Direction.Output);
        Port.portName = "Next";
        Node.outputContainer.Add(Port);

        Node.RefreshExpandedState();
        Node.RefreshPorts();

        // Entry Node Default Position
        Node.SetPosition(new Rect(new Vector2(100, 200), defaultNodeSize));

        return Node;
    }

    public DialogueNode CreateNode(string nodeName)
    {
        var Node = CreateDialogueNode(nodeName);
        AddElement(Node);

        return Node;
    }

    // Create Node, but Not Add it to View
    private DialogueNode CreateDialogueNode(string nodeName)
    {
        var Node = new DialogueNode
        {
            title = nodeName,
            m_DialogueText = nodeName,
            m_Guid = Guid.NewGuid()
        };

        var inputPort = GeneratePort(Node, Direction.Input, Port.Capacity.Multi);
        inputPort.portName = "Input";
        Node.inputContainer.Add(inputPort);

        var addChoiceButton = new Button(() =>
        {
            AddChoicePort(Node);
        });
        addChoiceButton.text = "Add Choice";
        Node.titleContainer.Add(addChoiceButton);

        Node.RefreshExpandedState();
        Node.RefreshPorts();

        Node.SetPosition(new Rect(Vector2.zero, defaultNodeSize));

        return Node;
    }

    private void AddChoicePort(DialogueNode targetNode)
    {
        var choicePort = GeneratePort(targetNode, Direction.Output);

        // Get Port Count
        var outputPortCount = targetNode.outputContainer.Query("connector").ToList().Count;
        choicePort.portName = $"Choice {outputPortCount}";

        targetNode.outputContainer.Add(choicePort);

        targetNode.RefreshPorts();
        targetNode.RefreshExpandedState();
    }
}
