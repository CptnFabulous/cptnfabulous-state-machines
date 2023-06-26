using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CptnFabulous.StateMachines
{
    [CustomEditor(typeof(FiniteStateMachine))]
    public class StateMachineEditor : Editor
    {
        FiniteStateMachine fsm => target as FiniteStateMachine;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            

            if (GUILayout.Button("Open node editor"))
            {
                StateMachineEditorWindow.target = fsm;
                EditorWindow.GetWindow<StateMachineEditorWindow>();
            }
        }
    }

    public class StateMachineEditorWindow : EditorWindow
    {
        // I found this forum post!
        // https://forum.unity.com/threads/simple-node-editor.189230/
        // This should help a lot.

        public static FiniteStateMachine target;

        //public SerializedObject serializedObject;
        Editor currentlyEditing;

        public static List<State> states => target.states;
        public static List<StateTransition> transitions => target.transitions;


        float transitionLineWidth = 5;
        float transitionSideOffset = 10; // How much the curve deviates to the side, to account for another line going in the same direction
        float transitionArrowSpacing = 30;



        [MenuItem("Window/State Machine Editor")]
        static void ShowEditor() => EditorWindow.GetWindow<StateMachineEditorWindow>();

        /*
        
        Still to do:
        * Transition conditions storing
        * Transition conditions editing
        * Make 'edit state' context menu appear properly
        * 'Change entry state' context menu

        */

        private void OnEnable()
        {
            if (target == null) return;

            target.states.RemoveAll((s) => !s);
            target.transitions.RemoveAll((t) => t.from == null && t.to == null);
            target.transitions.RemoveAll((t) => t.from == t.to);
            //ResetLook();
        }
        private void OnGUI()
        {
            if (target == null)
            {
                GUI.Label(rootVisualElement.contentRect, "No FiniteStateMachine selected. Please select one and press 'Open node editor'.");
                return;
            }

            if (EditorShortcuts.ContextClickedOnRect(rootVisualElement.contentRect))
            {
                // Show context menu for creating a new state
                StateMachineEditorFunctions.ShowCreateStateMenu(target, Event.current.mousePosition);
            }
            
            foreach (StateTransition t in transitions) DrawTransition(t);

            #region Draw windows

            BeginWindows();

            int i = 0;

            #region State windows
            for (; i < states.Count; i++)
            {
                StateMachineEditorFunctions.DrawStateWindow(states[i], i, ShowEditStateMenu);
            }
            #endregion

            #region Entry, 'any state' and exit boxes
            // Draw 'entry' box
            EditorShortcuts.DrawDraggableWindow(target, "Entry", i, ref target.entryNodePosition, StateMachineEditorFunctions.nonStateObjectSize, EditEntryStateMenu().ShowAsContext, null);
            // Draw line going from entry box to first state
            if (target.entryState != null) StateMachineEditorFunctions.DrawTransitionLine(target.entryNodePosition, target.entryState.editorPosition, null, null);

            i++; // Increase index and draw 'from any state' box
            EditorShortcuts.DrawDraggableWindow(target, "Any State", i, ref target.fromAnyStatePosition, StateMachineEditorFunctions.nonStateObjectSize, () => ShowEditStateMenu(null), null);
            i++; // Increase index and draw 'exit' box
            EditorShortcuts.DrawDraggableWindow(target, "Exit", i, ref target.exitStatePosition, StateMachineEditorFunctions.nonStateObjectSize, null, null);
            #endregion

            /*
            i++;
            Rect conditionsRect = new Rect(rootVisualElement.contentRect);
            conditionsRect.width = 200;
            GUI.Window(i, conditionsRect, (i) => ConditionsWindow(), "Conditions");
            
            i++;
            Rect inspectorRect = new Rect(rootVisualElement.contentRect);
            inspectorRect.x = inspectorRect.width - 300;
            inspectorRect.width = 300;
            GUI.Window(i, inspectorRect, (i) => InspectorWindow(), "Inspector");
            */

            EndWindows();

            #endregion
        }

        

        

        #region View state
        
        void ShowEditStateMenu(State state)
        {
            GenericMenu menu = new GenericMenu();

            #region Create transition
            string subMenu = "Create transition heading to/";
            foreach (State s in states)
            {
                Debug.Log(s);
                GUIContent content = new GUIContent(subMenu + EditorShortcuts.OptionNameInGenericMenuSafeFormat(s.name));
                if (s != state)
                {
                    menu.AddItem(content, false, () => AddTransition(state, s));
                }
                else
                {
                    menu.AddDisabledItem(content);
                }
            }
            if (state != null)
            {
                menu.AddSeparator(subMenu);
                menu.AddItem(new GUIContent(subMenu + "Exit"), false, () => AddTransition(state, null));
            }
            #endregion

            #region Delete state
            if (state != null)
            {
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Delete"), false, () =>
                {
                    Undo.RecordObject(target, "Delete state");
                    transitions.RemoveAll((t) => t.from == state || t.to == state);
                    states.Remove(state);
                    EditorUtility.SetDirty(target);
                });
            }
            #endregion

            menu.ShowAsContext();
        }
        void AddTransition(State from, State to)
        {
            Undo.RecordObject(target, "Add transition");

            StateTransition t = new StateTransition(from, to);
            transitions.Add(t);

            EditorUtility.SetDirty(target);
        }
        #endregion

        public GenericMenu EditEntryStateMenu()
        {
            GenericMenu menu = new GenericMenu();
            
            string subMenu = "Set entry state";
            foreach (State state in states)
            {
                GUIContent content = new GUIContent(subMenu + '/' + EditorShortcuts.OptionNameInGenericMenuSafeFormat(state.name));
                menu.AddItem(content, state == target.entryState, () =>
                {
                    Undo.RecordObject(target, "Change entry state");
                    target.entryState = state;
                    EditorUtility.SetDirty(target);
                });
            }

            return menu;
        }

        #region Transitions
        void DrawTransition(StateTransition t)
        {
            Vector2 p1 = (t.from != null) ? t.from.editorPosition : target.fromAnyStatePosition;
            Vector2 p2 = (t.to != null) ? t.to.editorPosition : target.exitStatePosition;

            string text = t.conditionData != null ? t.conditionData : "No condition";
            //string text = t.condition != null ? t.condition.ToString() : "No condition";

            //DrawTransitionLine(p1, p2, text, () => currentlyEditing = Editor.CreateEditor(t));
            StateMachineEditorFunctions.DrawTransitionLine(p1, p2, text, () => ShowTransitionWindow(t));
        }
        
        GenericMenu EditTransitionMenu(StateTransition t)
        {
            // Set up 'edit transition' menu
            GenericMenu menu = new GenericMenu();
            string subMenu;

            #region Change from state
            subMenu = "Change 'from' state/";
            menu.AddItem(new GUIContent(subMenu + "Any State"), t.from == null, () => SetTransitionStateValue(t, null, false));
            menu.AddSeparator(subMenu);
            AddTransitionPointSetterOptions(menu, t, subMenu, false);
            #endregion

            #region Change to state
            subMenu = "Change 'to' state/";
            AddTransitionPointSetterOptions(menu, t, subMenu, true);
            menu.AddSeparator(subMenu);
            menu.AddItem(new GUIContent(subMenu + "Exit"), t.to == null, () => SetTransitionStateValue(t, null, true));
            #endregion

            #region Set conditions
            /*
            if (target.transitionConditions != null && target.transitions.Count > 0)
            {
                subMenu = "Change condition/";

                menu.AddItem(new GUIContent(subMenu + "Inactive"), t.condition == null, () => t.condition = null);
                menu.AddSeparator(subMenu);
                
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("No conditions found"));
            }
            */
            #endregion

            #region Delete transition
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Delete"), false, () =>
            {
                Undo.RecordObject(target, "Delete transition");
                transitions.Remove(t);
                EditorUtility.SetDirty(target);
            });
            #endregion

            return menu;
        }
        void AddTransitionPointSetterOptions(GenericMenu menu, StateTransition transition, string subMenu, bool toAndNotFrom)
        {
            foreach (State newState in target.states)
            {
                string optionName = subMenu + EditorShortcuts.OptionNameInGenericMenuSafeFormat(newState.name);
                State stateToCheck = toAndNotFrom ? transition.to : transition.from;

                // If this state is the value of the other variable, show it greyed out to show that it's already taken (since a state can't transition to itself)
                State opposite = toAndNotFrom ? transition.from : transition.to;
                if (newState == opposite)
                {
                    // Add an explanatino showing that this one is the other end of the transition
                    string explanation = " (Transitions " + (!toAndNotFrom ? "to" : "from") + ')';
                    menu.AddDisabledItem(new GUIContent(optionName + explanation));
                    continue;
                }

                // Add an item for this state (tick the one that's currently selected)
                menu.AddItem(new GUIContent(optionName), stateToCheck == newState, () =>
                {
                    SetTransitionStateValue(transition, newState, toAndNotFrom);
                    /*
                    if (toAndNotFrom)
                    {
                        SetTransitionStateValue(ref transition.to, newState, "Set 'to' state");
                    }
                    else
                    {
                        SetTransitionStateValue(ref transition.from, newState, "Set 'from' state");
                    }
                    */
                });
            }
        }
        void SetTransitionStateValue(StateTransition transition, State newValue, bool toAndNotFrom)
        {
            string message = toAndNotFrom ? "Set 'to' state" : "Set 'from' state";
            Undo.RecordObject(target, message);

            if (toAndNotFrom)
            {
                transition.to = newValue;
            }
            else
            {
                transition.from = newValue;
            }

            EditorUtility.SetDirty(target);
        }

        void ShowTransitionWindow(StateTransition transition)
        {
            Debug.Log("Editing transition " + transition);
            EditTransitionMenu(transition).ShowAsContext();
            return;

            //EditorGUI.MaskField()
            
            /*
            Rect transitionWindowRect = new Rect(Input.mousePosition, new Vector2(200, 300));
            GUI.Window(-1, transitionWindowRect, (_) => TransitionWindow(transition), "Set Transition Values");
            */
        }
        
        #endregion









        void ResetLook()
        {
            Vector2 min = Vector2.positiveInfinity;
            Vector2 max = Vector2.negativeInfinity;

            void SetMinAndMax(Vector2 value)
            {
                min = Vector2.Min(min, value);
                max = Vector2.Max(max, value);
            }

            SetMinAndMax(target.entryNodePosition);
            SetMinAndMax(target.exitStatePosition);
            SetMinAndMax(target.fromAnyStatePosition);
            foreach (State s in target.states) SetMinAndMax(s.editorPosition);

            Vector2 size = max - min;
            Vector2 centre = min + (0.5f * size);
        }

    }
}
