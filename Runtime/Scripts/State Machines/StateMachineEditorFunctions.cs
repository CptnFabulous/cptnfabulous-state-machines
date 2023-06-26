using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CptnFabulous.StateMachines;

namespace CptnFabulous.StateMachines
{
    public static class StateMachineEditorFunctions
    {
        public static Vector2 stateObjectSize = new Vector2(200, 50);
        public static Vector2 nonStateObjectSize = new Vector2(100, 50);

        public static float transitionLineWidth = 5;
        public static float transitionSideOffset = 10; // How much the curve deviates to the side, to account for another line going in the same direction
        public static float transitionArrowSpacing = 30;
        static Texture2D tt;
        static Texture2D bg;
        public static Vector2 transitionArrowSize = new Vector2(20, 20);
        public static Vector2 characterSize = new Vector2(10, 20); // Dimensions of a character, to calculate text boxes that change size based on the text therein

        public static Texture2D transitionArrowTexture => tt ??= AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/cptnfabulous-state-machines/Runtime/Scripts/State Machines/directional arrow.png");
        public static Texture2D backgroundTexture => bg ??= AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/cptnfabulous-state-machines/Runtime/Scripts/State Machines/background.png");



        public static void DrawStateWindow(State state, int index, Action<State> contextButton)
        {
            EditorShortcuts.DrawDraggableWindow(state, state.name, index, ref state.editorPosition, stateObjectSize, () => contextButton.Invoke(state), (s) =>
            {
                /*
                Editor e = Editor.CreateEditor(state);
                e.OnInspectorGUI();
                DestroyImmediate(e);
                */
            });
        }
        public static void DrawTransitionLine(Vector2 from, Vector2 to, string buttonText, System.Action onClick)
        {
            #region Draw line
            Vector2 direction = to - from;
            Vector2 dn = direction.normalized;
            Vector2 sideOffsetVector = Vector2.Perpendicular(dn * transitionSideOffset);
            from += sideOffsetVector;
            to += sideOffsetVector;

            Handles.BeginGUI();
            Handles.DrawLine(from, to);
            //Handles.DrawBezier(from, to, from + dn, to + -dn, Color.white, null, transitionLineWidth);
            Handles.EndGUI();
            #endregion

            #region Draw arrows
            /*
            float angle = Vector2.SignedAngle(Vector2.down, direction);
            float distance = Vector2.Distance(to, from);
            Vector2 arrowOffset = transitionArrowSize / 2;
            for (float i = 0; i < distance; i += transitionArrowSpacing)
            {
                Vector2 arrowPosition = from + (dn * i);
                Rect r = new Rect(arrowPosition - arrowOffset, transitionArrowSize);

                GUIUtility.RotateAroundPivot(angle, arrowPosition); // Set rotation
                GUI.Label(r, transitionArrowTexture, GUIStyle.none); // Draw object
                GUI.matrix = Matrix4x4.identity; // Reset rotation
            }
            */

            float angle = Vector2.SignedAngle(Vector2.down, direction);
            float distance = Vector2.Distance(to, from);
            Vector2 arrowOffset = transitionArrowSize / 2;
            int numberOfArrows = Mathf.FloorToInt(distance / transitionArrowSpacing);
            for (int i = 0; i < numberOfArrows; i++)
            {
                float multiplier = distance / numberOfArrows * i;
                Vector2 arrowPosition = from + (dn * multiplier);
                Rect r = new Rect(arrowPosition - arrowOffset, transitionArrowSize);

                GUIUtility.RotateAroundPivot(angle, arrowPosition); // Set rotation
                GUI.Label(r, transitionArrowTexture, GUIStyle.none); // Draw object
                GUI.matrix = Matrix4x4.identity; // Reset rotation
            }
            #endregion

            #region Draw OnClick
            if (onClick != null || string.IsNullOrEmpty(buttonText) == false)
            {
                Vector2 size = characterSize * new Vector2(buttonText.Length, 1);
                Vector2 midPoint = from + (direction * 0.5f);
                Vector2 anchor = (sideOffsetVector.normalized * size).normalized;
                //Vector2 anchor = sideOffsetVector.normalized;
                /*
                anchor.x = Mathf.Sign(anchor.x);
                anchor.y = Mathf.Sign(anchor.y);
                */

                /*
                // Debug of anchor line
                Handles.BeginGUI();
                Handles.color = Color.red;
                Handles.DrawLine(midPoint, midPoint + (anchor * 50));
                Handles.color = Color.white;
                Handles.EndGUI();
                */

                Vector2 offsetToCentre = size / 2;
                Vector2 buttonPosition = midPoint - offsetToCentre + (anchor * offsetToCentre);
                Rect conditionRect = new Rect(buttonPosition, size);

                GUIContent content = new GUIContent(buttonText);
                if (onClick != null)
                {
                    if (GUI.Button(conditionRect, content))
                    {
                        onClick.Invoke();
                    }
                }
                else
                {
                    GUI.Box(conditionRect, content);
                }
            }
            #endregion
        }



        public static void ShowCreateStateMenu(MultiStateRunner parent, Vector2 mousePosition)
        {
            GenericMenu menu = new GenericMenu();

            string subMenu = "New State";

            Type[] types = TypeExtensions.FindInheritedTypes(typeof(State), true);
            foreach (Type stateType in types)
            {
                // Create a 'directory' within the generic menu for the particular state type, 
                string directory = "";

                // Keep checking upwards for each state type, to create a 'directory'
                Type parentType = stateType;
                while (parentType != typeof(State) && parentType != null)
                {
                    directory = '/' + parentType.Name + directory;
                    parentType = parentType.BaseType;
                }

                menu.AddItem(new GUIContent(subMenu + directory), false, () => CreateNewState(stateType, parent, mousePosition));
            }

            menu.ShowAsContext();
        }
        static void CreateNewState(Type stateType, MultiStateRunner parent, Vector2 editorPosition)
        {
            #region Create directory
            // Create a directory for the same folder as this state machine
            string thisPath = AssetDatabase.GetAssetPath(parent);
            Debug.Log(thisPath);
            string directory = thisPath.Remove(thisPath.LastIndexOf('/'));

            // Add onto the application data path to turn it into a proper directory
            string projectDirectory = Application.dataPath;
            projectDirectory = projectDirectory.Remove(projectDirectory.LastIndexOf('/'));
            projectDirectory += '/';
            #endregion

            #region Create name
            // File extension
            string fileExtension = ".asset";

            // Create new name and path (and ensure it isn't a duplicate)
            string newStateName = $"New {stateType.Name}";
            string newPathTemplate = directory + '/' + newStateName;

            // Check if a file already exists with the same name and location.
            // If so, keep iterating with a number on the end until a unique one comes up
            string newPath = newPathTemplate + fileExtension;
            for (int i = 1; System.IO.File.Exists(projectDirectory + newPath); i++)
            {
                newPath = newPathTemplate + $" ({i})" + fileExtension;
            }
            #endregion

            #region Spawn object
            // Instantiate 
            State newState = ScriptableObject.CreateInstance(stateType) as State;
            AssetDatabase.CreateAsset(newState, newPath);
            #endregion

            #region Perform operation to add to parent
            Undo.RecordObject(parent, "Add new state");
            newState.editorPosition = editorPosition;
            parent.states.Add(newState);
            EditorUtility.SetDirty(parent);
            #endregion
        }
    }
}