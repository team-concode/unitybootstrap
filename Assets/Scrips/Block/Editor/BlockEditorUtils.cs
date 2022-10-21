using System;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

public static class BlockEditorUtils {
    public static bool DrawHeader(GUIContent title, SerializedProperty group, Action<Vector2> contextAction) {
        var backgroundRect = GUILayoutUtility.GetRect(1f, 17f);

        var labelRect = backgroundRect;
        labelRect.xMin += 32f;
        labelRect.xMax -= 20f + 16 + 5;

        var foldoutRect = backgroundRect;
        foldoutRect.y += 1f;
        foldoutRect.width = 13f;
        foldoutRect.height = 13f;

        var toggleRect = backgroundRect;
        toggleRect.x += 16f;
        toggleRect.y += 2f;
        toggleRect.width = 13f;
        toggleRect.height = 13f;
        
        // Background rect should be full-width
        backgroundRect.xMin = 0f;
        backgroundRect.width += 4f;

        // Background
        float backgroundTint = EditorGUIUtility.isProSkin ? 0.1f : 1f;
        EditorGUI.DrawRect(backgroundRect, new Color(backgroundTint, backgroundTint, backgroundTint, 0.2f));

        // Title
        EditorGUI.LabelField(labelRect, title, EditorStyles.boldLabel);

        // Foldout
        group.serializedObject.Update();
        group.isExpanded = GUI.Toggle(foldoutRect, group.isExpanded, GUIContent.none, EditorStyles.foldout);
        group.serializedObject.ApplyModifiedProperties();

        // Context menu
        var menuIcon = CoreEditorStyles.paneOptionsIcon;
        var menuRect = new Rect(labelRect.xMax + 3f + 16 + 5 , labelRect.y + 1f, menuIcon.width, menuIcon.height);

        if (contextAction != null) {
            GUI.DrawTexture(menuRect, menuIcon);
        }
        
        // Handle events
        var e = Event.current;
        if (e.type == EventType.MouseDown) {
            if (contextAction != null && menuRect.Contains(e.mousePosition)) {
                contextAction(new Vector2(menuRect.x, menuRect.yMax));
                e.Use();
            } else if (labelRect.Contains(e.mousePosition)) {
                if (e.button == 0) {
                    group.isExpanded = !group.isExpanded;
                } else {
                    contextAction?.Invoke(e.mousePosition);
                }

                e.Use();
            }
        }

        return group.isExpanded;
    }
}
