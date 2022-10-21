using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

[Serializable]
public class Block : ScriptableObject {
    public ReadOnlyCollection<BlockParameter> parameters { get; private set; }
    
    public override int GetHashCode() {
        unchecked {
            var hash = 17;
            foreach (var t in parameters) {
                hash = hash * 23 + t.GetHashCode();
            }

            return hash;
        }
    }
    
    protected virtual void OnEnable() {
        parameters = this.GetType()
            .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(t => t.FieldType.IsSubclassOf(typeof(BlockParameter)))
            .OrderBy(t => t.MetadataToken) // Guaranteed order
            .Select(t => (BlockParameter)t.GetValue(this))
            .ToList()
            .AsReadOnly();
    }
}

public class BlockMenu : Attribute {
    public string menu { get; protected set; }
}