using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine.Assertions;

public sealed class BlockDataParameter {
    public SerializedProperty value { get; }
    public Attribute[] attributes { get; }
    public Type referenceType { get; }

    private SerializedProperty baseProperty;
    private object referenceValue;

    public string displayName => baseProperty.displayName;

    internal BlockDataParameter(SerializedProperty property) {
        var path = property.propertyPath.Split('.');
        object obj = property.serializedObject.targetObject;
        FieldInfo field = null;

        foreach (var p in path) {
            field = obj.GetType().GetField(p, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            obj = field.GetValue(obj);
        }

        Assert.IsNotNull(field);

        baseProperty = property.Copy();
        value = baseProperty.FindPropertyRelative("_value");
        attributes = field.GetCustomAttributes(false).Cast<Attribute>().ToArray();
        referenceType = obj.GetType();
        referenceValue = obj;
    }

    public T GetAttribute<T>() where T : Attribute {
        return (T)attributes.FirstOrDefault(x => x is T);
    }

    public T GetObjectRef<T>() {
        return (T)referenceValue;
    }
}

