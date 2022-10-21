using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Class)]
public sealed class BlockParameterDrawerAttribute : Attribute {
    public readonly Type parameterType;

    public BlockParameterDrawerAttribute(Type parameterType) {
        this.parameterType = parameterType;
    }
}

public abstract class BlockParameterDrawer {
    public virtual bool IsAutoProperty() => true;
    public abstract bool OnGUI(BlockDataParameter parameter, GUIContent title);
}