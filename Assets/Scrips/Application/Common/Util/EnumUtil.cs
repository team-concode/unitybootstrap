using System;
using System.Collections.Generic;
using System.Linq;

public static class EnumUtil {
    public static IEnumerable<T> GetValues<T>() {
        return Enum.GetValues(typeof(T)).Cast<T>();
    }

    public static T To<T>(string key) {
        return (T)Enum.Parse(typeof(T), key, false);
    }
}