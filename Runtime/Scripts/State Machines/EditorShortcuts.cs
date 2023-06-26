using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor;

public static class EditorShortcuts
{

    public static Vector2 contextButtonSize = new Vector2(20, 20);

    #region Windows
    public static void DrawDraggableWindow(UnityEngine.Object target, string name, int id, ref Vector2 position, Vector2 size, System.Action onContextClick, GUI.WindowFunction windowContent)
    {
        Vector2 offset = -size * 0.5f;
        Rect windowRect = new Rect(position + offset, size);

        windowRect = GUI.Window(id, windowRect, (i) =>
        {
            Rect labelRect = new Rect(0, 0, size.x, contextButtonSize.y);
            if (onContextClick != null)
            {
                labelRect.width -= contextButtonSize.x;
                Vector2 position = new Vector2(labelRect.width, 0);
                Rect contextButtonRect = new Rect(position, contextButtonSize);

                if (GUI.Button(contextButtonRect, GUIContent.none))
                {
                    onContextClick.Invoke();
                }
            }

            GUI.DragWindow(labelRect);
            windowContent?.Invoke(i);
        }, name);

        Vector2 newPos = windowRect.position - offset;
        if (newPos != position) // If position is changed, confirm value and record in undo history
        {
            Undo.RecordObject(target, "Set position");
            position = newPos; // Update value to save change to the node's position
            EditorUtility.SetDirty(target);
        }
    }
    #endregion

    #region Input
    public static bool ContextClickedOnRect(Rect r/*, out Vector2 screenPosition*/)
    {
        Event e = Event.current;
        //screenPosition = e.mousePosition;
        if (e.type != EventType.ContextClick) return false;
        if (r.Contains(e.mousePosition) == false) return false;
        return true;
    }
    #endregion

    #region Style and formatting
    public static string OptionNameInGenericMenuSafeFormat(string text)
    {
        // Get rid of all characters that could result in Unity thinking there are keyboard commands
        text = text.Replace("%", "(percent)");
        text = text.Replace("#", "(hash)");
        text = text.Replace("&", "(ampersand)");
        text = text.Replace("_", "(underscore)");
        return text;
    }
    #endregion

    #region Value setters
    public static T SetValueFromList<T>(string displayName, T current, List<T> values) where T : UnityEngine.Object
    {
        string[] names = new string[values.Count];
        for (int i = 0; i < names.Length; i++)
        {
            names[i] = values[i].name;
        }

        int index = values.IndexOf(current);


        index = EditorGUILayout.Popup(displayName, index, names);
        return values[index];
    }
    #endregion


}