public static class StringUtil {
    public static string TrimFloat(string value) {
        var count = value.Length;
        if (count <= 1) {
            return value;
        }

        var dotPoint = value.IndexOf(".");
        if (dotPoint < 0) {
            return value;
        }

        var len = count;
        for (var index = count - 1; index >= dotPoint; index--) {
            if (value[index] == '0') {
                len = index;
            } else {
                break;
            }
        }

        return value.Substring(0, len).Trim('.');
    }
}
