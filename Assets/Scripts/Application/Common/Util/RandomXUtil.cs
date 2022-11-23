using System;
using System.Collections.Generic;

public class RandomXUtil {
    private static RandomX _rand;
    private static RandomX rand => GetRandom();

    private static RandomX GetRandom() {
        if (_rand == null) {
            _rand = new RandomX(Util.CurrentTimeMillis());
        }
        return _rand;
    }

    public static int NextIndex<T>(List<T> candidates, RandomX random = null) where T : DataWithRate {
        var totalRate = 0;
        foreach (var item in candidates) {
            totalRate += item.rate;
        }

        var rd = random;
        if (rd == null) {
            rd = rand;
        }
        
        var value = rd.NextInt(totalRate);
        var current = 0;
        for (var index = 0; index < candidates.Count; index++) {
            var item = candidates[index];
            current += item.rate;
            if (value < current) {
                return index;
            }
        }

        return 0;
    }

    public static T Next<T>(List<T> candidates, RandomX random = null) where T : DataWithRate {
        if (candidates.Count == 0) {
            throw new Exception("It's empty list");
        }
        return candidates[NextIndex(candidates, random)];
    }
}