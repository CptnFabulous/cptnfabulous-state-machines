using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;

namespace CptnFabulous.StateMachines
{
    [CustomEditor(typeof(BehaviourTreeNode))]
    public class BehaviourTreeNodeEditor : Editor
    {
        BehaviourTreeNode targetNode => target as BehaviourTreeNode;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(10);
            GUILayout.Label("Create new state");

            if (GUILayout.Button("Create new state"))
            {
                StateMachineEditorFunctions.ShowCreateStateMenu(targetNode, Event.current.mousePosition);
            }
        }
    }

}

