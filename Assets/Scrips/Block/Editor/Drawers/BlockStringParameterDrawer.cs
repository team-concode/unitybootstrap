using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[BlockParameterDrawer(typeof(BlockParameter<string>))]
sealed class BlockStringParameterDrawer : BlockParameterDrawer {
    public override bool OnGUI(BlockDataParameter parameter, GUIContent title) {
        var value = parameter.value;
        EditorGUILayout.PropertyField(value, title, true);
        return true;
    }
}


[BlockParameterDrawer(typeof(BlockParameter<List<string>>))]
sealed class BlockStringListParameterDrawer : BlockParameterDrawer {
    public override bool OnGUI(BlockDataParameter parameter, GUIContent title) {
        var value = parameter.value;
        EditorGUILayout.PropertyField(value, title, true);
        return true;
    }
}