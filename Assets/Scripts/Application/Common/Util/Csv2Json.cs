using System.Collections.Generic;
using System.Text;

public class Csv2Json {
    public static string ToJson(string csv, char delimiter = '\t') {
        var sb = new StringBuilder();
        var parser = new CsvParser();
        parser.Parse(csv, delimiter);
        var keyRow = parser.GetRow(0);

        var keys = new List<string>();
        for (int index = 0; index < keyRow.Count; index++) {
            keys.Add(keyRow.NextString());
        }

        sb.Append("[");
        sb.AppendLine();
        for (int index = 1; index < parser.Count; index++) {
            var row = parser.GetRow(index);
            sb.Append("  {");
            sb.AppendLine();

            for (int ki = 0; ki < keys.Count; ki++) {
                // key
                var key = keys[ki];
                sb.Append("    \"");
                sb.Append(key);
                sb.Append("\": ");

                // value
                var value = row.NextString();
                var needQM = !IsNumber(value) && !IsBool(value);
                if (needQM) {
                    sb.Append("\"");
                }
                sb.Append(value.Replace("\\","\\\\")
                               .Replace("\"","\\\""));
                if (needQM) {
                    sb.Append("\"");
                }

                if (ki != keys.Count - 1) {
                    sb.Append(",");
                }
                sb.AppendLine();
            }

            sb.Append("  }");
            if (index != parser.Count - 1) {
                sb.Append(",");
            }
            sb.AppendLine();
        }

        sb.Append("]");
        sb.AppendLine();
        return sb.ToString();
    }

    private static bool IsBool(string value) {
        value = value.ToLower();
        return value is "true" or "false";
    }

    private static bool IsNumber(string value) {
        var hasPoint = false;
        if (value.Length == 0) {
            return false;
        }
        
        foreach (var ch in value) {
            if (ch < '0' || ch > '9') {
                return false;
            }

            if (ch == '.') {
                if (hasPoint) {
                    return false;
                }

                hasPoint = true;
            }
        }

        return true;
    }
}