using UnityEngine;

public static class Vector3Extension {
    public static Vector3 RotateX(this Vector3 v, float angle) {
        var sin = Mathf.Sin(angle);
        var cos = Mathf.Cos(angle);
        var ty = v.y;
        var tz = v.z;

        var result = v;
        result.y = (cos * ty) - (sin * tz);
        result.z = (cos * tz) + (sin * ty);
        return result;
    }

    public static Vector3 RotateY(this Vector3 v, float angle) {
        var sin = Mathf.Sin(angle);
        var cos = Mathf.Cos(angle);
        var tx = v.x;
        var tz = v.z;

        var result = v;
        result.x = (cos * tx) + (sin * tz);
        result.z = (cos * tz) - (sin * tx);
        return result;
    }
    
    public static Vector3 RotateZ(this Vector3 v, float angle) {
        var sin = Mathf.Sin(angle);
        var cos = Mathf.Cos(angle);
        var tx = v.x;
        var ty = v.y;

        var result = v;
        result.x = (cos * tx) - (sin * ty);
        result.y = (cos * ty) + (sin * tx);
        return result;
    }
}