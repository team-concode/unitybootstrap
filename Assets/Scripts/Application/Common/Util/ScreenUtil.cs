using UnityEngine;

public class ScreenUtil {
    public static float DeviceDiagonalSizeInInches() {
        var screenWidth = Screen.width / Screen.dpi;
        var screenHeight = Screen.height / Screen.dpi;
        var diagonalInches = Mathf.Sqrt (Mathf.Pow (screenWidth, 2) + Mathf.Pow (screenHeight, 2));
        Debug.Log ("Getting device inches: " + diagonalInches);
        return diagonalInches;
    }

    public static bool IsTablet() {
#if UNITY_IOS
        var identifier = SystemInfo.deviceModel;
        return identifier.StartsWith("iPad");
#else
        return false;
#endif
    }
}
