using System;
using UnityEngine;

[Serializable]
public struct Vec3 {
    public float x;
    public float y;
    public float z;
    
    public static readonly Vec3 zero = new(0f, 0f, 0f);
    public static readonly Vec3 one = new(1f, 1f, 1f);
    public static readonly Vec3 half = new(0.5f, 0.5f, 0.5f);
    public static readonly Vec3 up = new(0f, 1f, 0f);
    public static readonly Vec3 down = new(0f, -1f, 0f);
    public static readonly Vec3 left = new(-1f, 0f, 0f);
    public static readonly Vec3 right = new(1f, 0f, 0f);
    public static readonly Vec3 forward = new(0f, 0f, 1f);
    public static readonly Vec3 back = new(0f, 0f, -1f);

    
    public static readonly Vec3 [] news = {
        up,
        left,
        right,
        down,
    };

    public Vec3(float x, float y, float z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static Vec3 Build(float x, float y, float z) {
        return new Vec3(x, y, z);
    }
    
    public Vec3 Add(Vec3 rhs) {
        return new Vec3(x + rhs.x, y + rhs.y, z + rhs.z);
    }

    public Vec3 Subtract(Vec3 rhs) {
        return new Vec3(x - rhs.x, y - rhs.y, z - rhs.z);
    }

    public Vec3 Subtract(Vec3Int rhs) {
        return new Vec3(x - rhs.x, y - rhs.y, z - rhs.z);
    }

    public Vec3 Scalar(float s) {
        return new Vec3(x * s, y * s, z * s);
    }

    public float Len2() {
        return (x * x) + (y * y) + (z * z);
    }
    
    public float Len() {
        return Mathf.Sqrt(Len2());
    }

    public Vec3Int Round() {
        return new(Util.Round(x), Util.Round(y), Util.Round(z));
    }

    public Vec3Int Floor() {
        return new(Util.Floor(x), Util.Floor(y), Util.Floor(z));
    }

    public Vector3 ToVector3() {
        return new Vector3(x, y, z);
    }

    public override string ToString() {
        return $"(x:{x}, y:{y}, z:{z})";
    }

    public override bool Equals(object other) {
        return other is Vec3 other1 && this.Equals(other1);
    }

    public bool Equals(Vec3 other) {
        return this.x.Equals(other.x) && this.y.Equals(other.y) && this.z.Equals(other.z);
    }

    public override int GetHashCode() {
        return this.x.GetHashCode() ^ this.y.GetHashCode() << 2 ^ this.z.GetHashCode() >> 2;
    }

    public Vec3 Normalize() {
        var len = Len();
        var res = new Vec3 {
            x = x / len,
            y = y / len,
            z = z / len
        };
        return res;
    }

    public static Vec3 Lerp(Vec3 a, Vec3 b, float t) {
        t = Mathf.Clamp01(t);
        return new Vec3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
    }
    
    public static float Dot(Vec3 lhs, Vec3 rhs) => (float) ((double) lhs.x * (double) rhs.x + (double) lhs.y * (double) rhs.y + (double) lhs.z * (double) rhs.z);
    
    public static float Angle(Vec3 from, Vec3 to) {
        var num = (float) Math.Sqrt((double) from.Len() * (double) to.Len());
        return (double) num < 1.0000000036274937E-15 ? 0.0f : (float) Math.Acos((double) Mathf.Clamp(Vec3.Dot(from, to) / num, -1f, 1f)) * 57.29578f;
    }

    public static float SignedAngle(Vec3 from, Vec3 to, Vec3 axis) {
        var num1 = Angle(from, to);
        var num2 = (float) ((double) from.y * (double) to.z - (double) from.z * (double) to.y);
        var num3 = (float) ((double) from.z * (double) to.x - (double) from.x * (double) to.z);
        var num4 = (float) ((double) from.x * (double) to.y - (double) from.y * (double) to.x);
        var num5 = Mathf.Sign((float) ((double) axis.x * (double) num2 + (double) axis.y * (double) num3 + (double) axis.z * (double) num4));
        return num1 * num5;
    }    
    
    public Vec3 RotateX(float angle) {
        var sin = Mathf.Sin(angle);
        var cos = Mathf.Cos(angle);
        var ty = y;
        var tz = z;

        return new Vec3 {
            x = x,
            y = (cos * ty) - (sin * tz),
            z = (cos * tz) + (sin * ty)
        };
    }

    public Vec3 RotateY(float angle) {
        var sin = Mathf.Sin( angle );
        var cos = Mathf.Cos( angle );
        var tx = x;
        var tz = z;

        return new Vec3 {
            x = (cos * tx) + (sin * tz),
            y = y,
            z = (cos * tz) - (sin * tx),
        };
    }
    
    public Vec3 RotateZ(float angle) {
        var sin = Mathf.Sin( angle );
        var cos = Mathf.Cos( angle );
        var tx = x;
        var ty = y;

        return new Vec3 {
            x = (cos * tx) - (sin * ty),
            y = (cos * ty) + (sin * tx),
            z = z,
        };
    }    
}