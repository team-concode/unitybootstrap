using System;

[AttributeUsage(AttributeTargets.Field)]
public class BlockDisplayInfoAttribute : Attribute {
    public string name;
    public int order;
}