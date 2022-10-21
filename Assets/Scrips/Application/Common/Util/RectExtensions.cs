using UnityEngine;

public static class RectExtensions {
    public static Vector2 FitIn(this Rect rect, Vector2 pt) {
        pt.x = Mathf.Max(rect.xMin, pt.x);
        pt.y = Mathf.Max(rect.yMin, pt.y);
        pt.x = Mathf.Min(rect.xMax, pt.x);
        pt.y = Mathf.Min(rect.yMax, pt.y);
        return pt;
    }     
}
