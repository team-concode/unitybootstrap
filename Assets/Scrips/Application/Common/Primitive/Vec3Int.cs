using System;
using UnityEngine;

[Serializable]
public struct Vec3Int {
    public int x;
    public int y;
    public int z;

    public static readonly Vec3Int zero = new Vec3Int(0, 0, 0);
    public static readonly Vec3Int one = new Vec3Int(1, 1, 0);
    public static readonly Vec3Int up = new Vec3Int(0, 1, 0);
    public static readonly Vec3Int down = new Vec3Int(0, -1, 0);
    public static readonly Vec3Int left = new Vec3Int(-1, 0, 0);
    public static readonly Vec3Int right = new Vec3Int(1, 0, 0);
    public static readonly Vec3Int forward = new Vec3Int(0, 0, 1);
    public static readonly Vec3Int back = new Vec3Int(0, 0, -1);

    public static readonly Vec3Int leftTop = new Vec3Int(-1, 1, 0);
    public static readonly Vec3Int top = new Vec3Int(0, 1, 0);
    public static readonly Vec3Int rightTop = new Vec3Int(1, 1, 0);

    public static readonly Vec3Int leftBottom = new Vec3Int(-1, -1, 0);
    public static readonly Vec3Int bottom = new Vec3Int(0, -1, 0);
    public static readonly Vec3Int rightBottom = new Vec3Int(1, -1, 0);

    public static readonly Vec3Int [] news = {
        up,
        left,
        right,
        down,
    };

    public static readonly Vec3Int [] news8 = {
        leftTop,
        top,
        rightTop,
        left,
        right,
        leftBottom,
        bottom,
        rightBottom
    };

    public static Vec3Int [] diagonal = {
        leftTop,
        rightTop,
        leftBottom,
        rightBottom
    };


    public Vec3Int(int x, int y, int z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vec3Int Add(Vec3Int rhs) {
        return new Vec3Int(x + rhs.x, y + rhs.y, y + rhs.z);
    }

    public Vec3Int Subtract(Vec3Int rhs) {
        return new Vec3Int(x - rhs.x, y - rhs.y, z - rhs.z);
    }

    public Vector3Int ToVector3Int() {
        return new Vector3Int(x, y, z);
    }

    public Vec3 ToWdVec() {
        return new Vec3(x, y, z);
    }

    public override string ToString() {
        return $"(x:{x}, y:{y}, z:{z})";
    }

    public override bool Equals(object other) {
        return other is Vec3Int other1 && this.Equals(other1);
    }

    public bool Equals(Vec3Int other) {
        return this.x.Equals(other.x) && this.y.Equals(other.y) && this.z.Equals(other.z);
    }

    public override int GetHashCode() {
        var hashCode1 = this.y.GetHashCode();
        var hashCode2 = this.z.GetHashCode();
        return this.x.GetHashCode() ^ hashCode1 << 4 ^ hashCode1 >> 28 ^ hashCode2 >> 4 ^ hashCode2 << 28;
    }

    public static Vec3Int From(Vector3 pt) {
        return new Vec3Int(Mathf.RoundToInt(pt.x), Mathf.RoundToInt(pt.y), Mathf.RoundToInt(pt.z));
    }
}