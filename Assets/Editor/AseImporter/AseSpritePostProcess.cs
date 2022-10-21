using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

public static class AseSpritePostProcess {
    public class Property {
        public SerializedProperty border;
        public SerializedProperty physicsShape;
    }

    public static List<Vector4> GetPrevBorders(TextureImporter importer, List<SpriteMetaData> metaList) {
        SerializedObject serializedImporter = new SerializedObject(importer);
        var property = serializedImporter.FindProperty("m_SpriteSheet.m_Sprites");
        var res = new List<Vector4>();
        
        for (int index = 0; index < property.arraySize; index++) {
            var element = property.GetArrayElementAtIndex(index);
            var border = element.FindPropertyRelative("m_Border");
            res.Add(border.vector4Value);
        }
        
        return res;
    }
    
    public static Dictionary<string, Property> GetPhysicsShapeProperties(TextureImporter importer, 
                                                                         List<SpriteMetaData> metaList) {
        SerializedObject serializedImporter = new SerializedObject(importer);
        var property = serializedImporter.FindProperty("m_SpriteSheet.m_Sprites");
        var res = new Dictionary<string, Property>();
        var removed = new HashSet<int>();
        
        for (int index = 0; index < property.arraySize; index++) {
            var name = importer.spritesheet[index].name;
            if (res.ContainsKey(name)) {
                continue;
            }
            
            var element = property.GetArrayElementAtIndex(index);
            var border = element.FindPropertyRelative("m_Border");
            var physicsShape = element.FindPropertyRelative("m_PhysicsShape");
            
            res.Add(name, new Property {
                physicsShape = physicsShape,
                border = border
            });

            removed.Add(index);
        }
        
        return res;
    }

    public static void RecoverPhysicsShapeProperty(
        Dictionary<string, Property> newProperties, 
        Dictionary<string, Property> oldProperties) {

        SerializedProperty property1 = null; 
        SerializedProperty property2 = null; 
        foreach (var item in newProperties) {
            if (!oldProperties.TryGetValue(item.Key, out var oldItem)) {
                continue;
            }

            var newItem = item.Value;
            
            // recover physics
            if (oldItem.physicsShape.arraySize > 0) {
                newItem.physicsShape.arraySize = oldItem.physicsShape.arraySize;
                
                for (int index = 0; index < newItem.physicsShape.arraySize; index++) {
                    var newShape = newItem.physicsShape.GetArrayElementAtIndex(index);
                    var oldShape = oldItem.physicsShape.GetArrayElementAtIndex(index);
                    newShape.arraySize = oldShape.arraySize;

                    for (int pi = 0; pi < newShape.arraySize; pi++) {
                        var newPt = newShape.GetArrayElementAtIndex(pi);
                        var oldPt = oldShape.GetArrayElementAtIndex(pi);
                        newPt.vector2Value = oldPt.vector2Value;
                    }
                }

                property1 ??= newItem.physicsShape;
            }

            // recover border
            newItem.border.vector4Value = oldItem.border.vector4Value;
            property2 ??= newItem.border;
        }
        
        property1?.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        property2?.serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }
}