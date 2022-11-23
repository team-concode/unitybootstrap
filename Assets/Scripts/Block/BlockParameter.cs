using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class BlockParameter {
    public T GetValue<T>() {
        return ((BlockParameter<T>) this).value;
    }

    public abstract void SetValue(BlockParameter parameter);

    public static bool IsObjectParameter(Type type) {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(BlockObjectParameter<>)) {
            return true;
        }

        return type.BaseType != null && IsObjectParameter(type.BaseType);
    }
}

[Serializable]
public class BlockParameter<T> : BlockParameter, IEquatable<BlockParameter<T>> {
    [SerializeField]
    protected T _value;

    public virtual T value {
        get => _value;
        set => _value = value;
    }

    public BlockParameter() : this(default) {
    }

    public BlockParameter(T value) {
        _value = value;
    }

    public override void SetValue(BlockParameter parameter) {
        _value = parameter.GetValue<T>();
    }

    public override int GetHashCode() {
        unchecked {
            var hash = 17 * 23;
            if (!EqualityComparer<T>.Default.Equals(value)) {
                hash = hash * 23 + value?.GetHashCode()??0;
            }

            return hash;
        }
    }

    public override string ToString() => $"{value}";

    public static bool operator==(BlockParameter<T> lhs, T rhs) {
        return lhs != null && !ReferenceEquals(lhs.value, null) && lhs.value.Equals(rhs);
    }

    public static bool operator!=(BlockParameter<T> lhs, T rhs) => !(lhs == rhs);

    public bool Equals(BlockParameter<T> other) {
        if (ReferenceEquals(null, other)) {
            return false;
        }

        if (ReferenceEquals(this, other)) {
            return true;
        }

        return EqualityComparer<T>.Default.Equals(_value, other._value);
    }

    public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) {
            return false;
        }

        if (ReferenceEquals(this, obj)) {
            return true;
        }

        if (obj.GetType() != GetType()) {
            return false;
        }

        return Equals((BlockParameter<T>)obj);
    }

    public static explicit operator T(BlockParameter<T> prop) => prop._value;
}


[Serializable]
public class BlockBoolParameter : BlockParameter<bool> {
    public BlockBoolParameter(bool value) : base(value) {}
}

[Serializable]
public class BlockIntParameter : BlockParameter<int> {
    public BlockIntParameter(int value) : base(value) {}
}

[Serializable]
public class BlockColorParameter : BlockParameter<Color> {
    public BlockColorParameter(Color value) : base(value) {}
}

[Serializable]
public class BlockTileParameter : BlockParameter<TileBase> {
    public BlockTileParameter(TileBase value) : base(value) {}
}

[Serializable]
public class BlockRuleTileParameter : BlockParameter<RuleTile> {
    public BlockRuleTileParameter(RuleTile value) : base(value) {}
}

[Serializable]
public class BlockObjectParameter<T> : BlockParameter<T> {
    internal ReadOnlyCollection<BlockParameter> parameters { get; private set; }

    public sealed override T value {
        get => _value;
        set {
            _value = value;
            if (_value == null) {
                parameters = null;
                return;
            }

            parameters = _value.GetType()
                .GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(t => t.FieldType.IsSubclassOf(typeof(BlockParameter)))
                .OrderBy(t => t.MetadataToken) // Guaranteed order
                .Select(t => (BlockParameter)t.GetValue(_value))
                .ToList()
                .AsReadOnly();
        }
    }

    public BlockObjectParameter(T value) {
        this.value = value;
    }
}