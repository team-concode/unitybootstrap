using System.Collections.Generic;

public static class EqualUtil {
    public static bool Equal<T>(List<T> lhs, List<T> rhs) {
        if (lhs.Count != rhs.Count) {
            return false;
        } 
        
        for (int index = 0; index < lhs.Count; index++) {
            var li = lhs[index];
            var ri = rhs[index];
            if (!li.Equals(ri)) {
                return false;
            }
        }

        return true;
    }

    public static bool Equal<K, V>(Dictionary<K, List<V>> lhs, Dictionary<K, List<V>> rhs) {
        if (lhs.Count != rhs.Count) {
            return false;
        } 
        
        foreach (var item in lhs) {
            if (!rhs.TryGetValue(item.Key, out List<V> otherV)) {
                return false;
            }

            if (item.Value.Count != otherV.Count) {
                return false;
            }

            for (int index = 0; index < item.Value.Count; index++) {
                var li = item.Value[index];
                var ri = otherV[index];
                if (!li.Equals(ri)) {
                    return false;
                }
            }
        }

        return true;
    }
}