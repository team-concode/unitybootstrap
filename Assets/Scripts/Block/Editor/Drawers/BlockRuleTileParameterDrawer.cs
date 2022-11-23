using UnityEditor;
using UnityEngine;

[BlockParameterDrawer(typeof(BlockRuleTileParameter))]
sealed class BlockRuleTileParameterDrawer : BlockParameterDrawer {
    public override bool OnGUI(BlockDataParameter parameter, GUIContent title) {
        var value = parameter.value;
        if (value.propertyType != SerializedPropertyType.ObjectReference)
            return false;

        var v = EditorGUILayout.ObjectField(title, value.objectReferenceValue, typeof(RuleTile), true);
        value.objectReferenceValue = v;
        return true;
    }
}