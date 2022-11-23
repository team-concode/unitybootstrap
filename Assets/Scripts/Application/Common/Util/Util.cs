using System.Collections.Generic;

public class Util {
    public static float epsilon = 0.001f;

    private static TimeService timeService => UnityBean.BeanContainer.GetBean<TimeService>();

    public static ulong CurrentTimeMillis() {
        return (ulong)timeService.GetServerUnixTimeMilli();
    }
    
    public static int Min(int a, int b) {
        return a < b ? a : b;
    }
    
    public static int Max(int a, int b) {
        return a > b ? a : b;
    }

    public static float Min(float a, float b) {
        return a < b ? a : b;
    }
    
    public static float Max(float a, float b) {
        return a > b ? a : b;
    }

    public static float Abs(float a) {
        return a < 0 ? -a : a;
    }

    public static int Abs(int a) {
        return a < 0 ? -a : a;
    }

    public static bool Approximately(float a, float b) {
        return Abs(a - b) < epsilon;
    }
    
    public static int Round(float v) {
        if (v > 0) {
            return (int)(v + 0.5f);
        }
        return (int)(v - 0.5f);
    }

    public static int Floor(float v) {
        return (int)v;
    }

    public static int Ceil(float v) {
        int intV = (int)v;
        if (v > 0) {
            if (intV < v) {
                return intV + 1;
            }
        } else if (v < 0) {
            if (intV > v) {
                return intV - 1;
            }
        }

        return intV;
    }
}