using System;
using System.Collections.Generic;
using UnityEngine;

public class BlockProfile<TBlock> : Block where TBlock : Block {
    public List<TBlock> blocks = new List<TBlock>();

    [NonSerialized]
    public bool isDirty = true; // Editor only, doesn't have any use outside of it

    protected override void OnEnable() {
        blocks.RemoveAll(x => x == null);
    }

    public void Reset() {
        isDirty = true;
    }

    public K Add<K>() where K : TBlock {
        return (K)Add(typeof(K));
    }

    public TBlock Add(Type type) {
        var component = (TBlock)CreateInstance(type);
#if UNITY_EDITOR
        component.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
        component.name = type.Name;
#endif
        blocks.Add(component);
        isDirty = true;
        return component;
    }
    
    public void Remove(int index) {
        blocks.RemoveAt(index);
        isDirty = true;
    }

    public override int GetHashCode() {
        unchecked {
            var hash = 17;
            foreach (var t in blocks) {
                hash = hash * 23 + t.GetHashCode();
            }

            return hash;
        }
    }

    public int GetComponentListHashCode() {
        unchecked {
            var hash = 17;
            foreach (var t in blocks) {
                hash = hash * 23 + t.GetType().GetHashCode();
            }

            return hash;
        }
    }
    
    public T GetBlock<T>() where T : Block {
        foreach (var component in blocks) {
            if (component.GetType() == typeof(T)) {
                return component as T;
            }
        }

        return null;
    }
    
    public List<T> GetBlocks<T>() where T : Block {
        var res = new List<T>();
        foreach (var component in blocks) {
            if (component.GetType() == typeof(T)) {
                res.Add(component as T);
            }
        }

        return res;
    }
}