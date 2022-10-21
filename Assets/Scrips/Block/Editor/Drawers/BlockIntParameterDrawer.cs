using UnityEditor;
using UnityEngine;

[BlockParameterDrawer(typeof(BlockIntParameter))]
sealed class BlockIntParameterDrawer : BlockParameterDrawer {
    public override bool OnGUI(BlockDataParameter parameter, GUIContent title) {
        var value = parameter.value;
        if (value.propertyType != SerializedPropertyType.Integer)
            return false;

        int v = EditorGUILayout.IntField(title, value.intValue);
        value.intValue = v;
        return true;
    }
}
