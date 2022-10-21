using UnityEditor;
using UnityEngine;

[BlockParameterDrawer(typeof(BlockColorParameter))]
sealed class BlockColorParameterDrawer : BlockParameterDrawer {
    public override bool OnGUI(BlockDataParameter parameter, GUIContent title) {
        var value = parameter.value;
        if (value.propertyType != SerializedPropertyType.Color)
            return false;

        var v = EditorGUILayout.ColorField(title, value.colorValue);
        value.colorValue = v;
        return true;
    }
}