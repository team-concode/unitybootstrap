using UnityEngine;

public static class ColorUtil {
    public static Color FromHex(string hex) {
        byte r = byte.Parse(hex.Substring(0,2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2,2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4,2), System.Globalization.NumberStyles.HexNumber);
        return new Color32(r,g,b, 255);
    }

    public static Color HdrColor(Color color, float intensity) {
        var factor = Mathf.Pow(2, intensity);
        return new Color(color.r * factor,color.b * factor,color.b * factor);
    }
}