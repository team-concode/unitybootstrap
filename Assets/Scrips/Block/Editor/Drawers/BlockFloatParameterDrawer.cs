using UnityEditor;
using UnityEngine;

[BlockParameterDrawer(typeof(BlockParameter<float>))]
sealed class BlockFloatParameterDrawer : BlockParameterDrawer {
    public override bool OnGUI(BlockDataParameter parameter, GUIContent title) {
        var value = parameter.value;
        if (value.propertyType != SerializedPropertyType.Float)
            return false;

        float v = EditorGUILayout.FloatField(title, value.floatValue);
        value.floatValue = v;
        return true;
    }
}