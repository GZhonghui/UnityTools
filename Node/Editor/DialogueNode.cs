using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class DialogueNode : Node
{
    public Guid m_Guid;

    public string m_DialogueText;

    public bool m_EntryPoint = false;
}
