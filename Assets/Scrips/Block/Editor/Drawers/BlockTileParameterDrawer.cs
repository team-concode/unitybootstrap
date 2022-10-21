using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[BlockParameterDrawer(typeof(BlockTileParameter))]
sealed class BlockTileParameterDrawer : BlockParameterDrawer {
    public override bool OnGUI(BlockDataParameter parameter, GUIContent title) {
        var value = parameter.value;
        if (value.propertyType != SerializedPropertyType.ObjectReference)
            return false;

        var v = EditorGUILayout.ObjectField(title, value.objectReferenceValue, typeof(TileBase), true);
        value.objectReferenceValue = v;
        return true;
    }
}