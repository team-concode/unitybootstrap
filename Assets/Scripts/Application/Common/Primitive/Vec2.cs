using System;
using UnityEngine;

[Serializable]
public struct Vec2 {
    public float x;
    public float y;
    
    public static readonly Vec2 zero = new(0f, 0f);
    public static readonly Vec2 one = new(1f, 1f);
    public static readonly Vec2 half = new(0.5f, 0.5f);
    public static readonly Vec2 up = new(0f, 1f);
    public static readonly Vec2 down = new(0f, -1f);
    public static readonly Vec2 left = new(-1f, 0f);
    public static readonly Vec2 right = new(1f, 0f);

    public static readonly Vec2 [] news = {
        up,
        left,
        right,
        down,
    };

    public Vec2(float x, float y) {
        this.x = x;
        this.y = y;
    }

    public static Vec2 Build(float x, float y) {
        return new Vec2(x, y);
    }
    
    public Vec2 Add(Vec2 rhs) {
        return new Vec2(x + rhs.x, y + rhs.y);
    }

    public Vec2 Subtract(Vec2 rhs) {
        return new Vec2(x - rhs.x, y - rhs.y);
    }

    public Vec2 Subtract(Vec2Int rhs) {
        return new Vec2(x - rhs.x, y - rhs.y);
    }

    public Vec2 Scalar(float s) {
        return new Vec2(x * s, y * s);
    }

    public Vec2 Rotate(float angle) {
        var sin = Mathf.Sin(angle);
        var cos = Mathf.Cos(angle);
        var tx = x;
        var ty = y;

        var result = new Vec2 {
            x = (cos * tx) - (sin * ty),
            y = (cos * ty) + (sin * tx)
        };
        return result;
    }

    public float Len2() {
        return (x * x) + (y * y);
    }
    
    public float Len() {
        return Mathf.Sqrt(Len2());
    }

    public Vec2Int Round() {
        return new(Util.Round(x), Util.Round(y));
    }

    public Vec2Int Floor() {
        return new(Util.Floor(x), Util.Floor(y));
    }

    public Vector2 ToVector2() {
        return new Vector2(x, y);
    }

    public override string ToString() {
        return $"(x:{x}, y:{y})";
    }

    public override bool Equals(object other) {
        return other is Vec2 other1 && this.Equals(other1);
    }

    public bool Equals(Vec2 other) {
        return this.x.Equals(other.x) && this.y.Equals(other.y);
    }

    public override int GetHashCode() {
        return (int)(this.x * 1000) ^ (int)(this.y * 1000) << 2;
    }

    public Vec2 Normalize() {
        var len = Len();
        var res = new Vec2 {
            x = x / len,
            y = y / len
        };
        return res;
    }

    public static Vec2 Lerp(Vec2 a, Vec2 b, float t) {
        t = Mathf.Clamp01(t);
        return new Vec2(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t);
    }
    
    public static float Dot(Vec2 lhs, Vec2 rhs) => (float) ((double) lhs.x * (double) rhs.x + (double) lhs.y * (double) rhs.y);

    public static float Angle(Vec2 from, Vec2 to) {
        float num = (float)Math.Sqrt((double) from.Len2() * (double) to.Len2());
        return num < 1.0000000036274937E-15 ? 0.0f : (float) Math.Acos((double) Mathf.Clamp(Vec2.Dot(from, to) / num, -1f, 1f)) * 57.29578f;
    }
    
    public static float SignedAngle(Vec2 from, Vec2 to) => Vec2.Angle(from, to) * Mathf.Sign((float) ((double) from.x * (double) to.y - (double) from.y * (double) to.x));
}