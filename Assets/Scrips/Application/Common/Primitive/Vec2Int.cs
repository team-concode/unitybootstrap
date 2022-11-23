using System;
using UnityEngine;

[Serializable]
public struct Vec2Int {
    public int x;
    public int y;

    public static readonly Vec2Int zero = new(0, 0);
    public static readonly Vec2Int one = new(1, 1);
    public static readonly Vec2Int up = new(0, 1);
    public static readonly Vec2Int down = new(0, -1);
    public static readonly Vec2Int left = new(-1, 0);
    public static readonly Vec2Int right = new(1, 0);

    public static readonly Vec2Int leftTop = new(-1, 1);
    public static readonly Vec2Int top = new(0, 1);
    public static readonly Vec2Int rightTop = new(1, 1);

    public static readonly Vec2Int leftBottom = new(-1, -1);
    public static readonly Vec2Int bottom = new(0, -1);
    public static readonly Vec2Int rightBottom = new(1, -1);

    public static Vec2Int [] news = {
        up,
        left,
        right,
        down,
    };

    public static Vec2Int [] news8 = {
        leftTop,
        top,
        rightTop,
        left,
        right,
        leftBottom,
        bottom,
        rightBottom
    };

    public static Vec2Int [] diagonal = {
        leftTop,
        rightTop,
        leftBottom,
        rightBottom
    };


    public Vec2Int(int x, int y) {
        this.x = x;
        this.y = y;
    }

    public Vec2Int Add(Vec2Int rhs) {
        return new Vec2Int(x + rhs.x, y + rhs.y);
    }

    public Vec2Int Subtract(Vec2Int rhs) {
        return new Vec2Int(x - rhs.x, y - rhs.y);
    }

    public Vector2Int ToVector2Int() {
        return new Vector2Int(x, y);
    }

    public Vec2 ToWdVec() {
        return new Vec2(x, y);
    }

    public override string ToString() {
        return $"(x:{x}, y:{y})";
    }

    public override bool Equals(object other) {
        return other is Vec2Int other1 && this.Equals(other1);
    }

    public bool Equals(Vec2Int other) {
        return this.x.Equals(other.x) && this.y.Equals(other.y);
    }

    public override int GetHashCode() {
        return this.x ^ this.y << 2;
    }

    public static Vec2Int From(Vector3 pt) {
        return new Vec2Int(Mathf.RoundToInt(pt.x), Mathf.RoundToInt(pt.y));
    }
}