public class RandomX {
    private ulong []state = new ulong[4];

    public RandomX(ulong seed) {
        Init(seed);
    }

    private static ulong Rol64(ulong x, int k) {
        return (x << k) | (x >> (64 - k));
    }

    private static ulong SplitMix64(ulong x) {
        var z = (x += 0x9e3779b97f4a7c15);
        z = (z ^ (z >> 30)) * 0xbf58476d1ce4e5b9;
        z = (z ^ (z >> 27)) * 0x94d049bb133111eb;
        return z ^ (z >> 31);
    }
    
    private void Init(ulong seed) {
        var tmp = SplitMix64(seed);
        state[0] = tmp;
        state[1] = tmp >> 32;

        tmp = SplitMix64(tmp);
        state[2] = tmp;
        state[3] = tmp >> 32;
    }
    
    private ulong Xoshiro256ss() {
        var result = Rol64(state[1] * 5, 7) * 9;
        var t = state[1] << 17;

        state[2] ^= state[0];
        state[3] ^= state[1];
        state[1] ^= state[2];
        state[0] ^= state[3];

        state[2] ^= t;
        state[3] = Rol64(state[3], 45);

        return result;
    }

    public int NextInt(int to) {
        if (to <= 0) {
            return 0;
        }
        
        return (int)((Xoshiro256ss() % (uint)to) & 0x7FFFFFFF);
    }
    
    public double NextDouble(double to) {
        var range = ulong.MaxValue;
        var res = Xoshiro256ss() / (double)range;
        return res * to;
    }

    public float NextFloat(float to) {
        return (float)NextDouble(to);
    }

    public float Range(float from, float to) {
        if (to <= from) {
            return from;
        }

        var v = NextDouble(1);
        return (float)(from + (to - from) * v);
    }

    public int Range(int from, int to) {
        if (to <= from) {
            return from;
        }
        
        return from + NextInt(to - from);
    }
    
    public bool Succeed100(float rate) {
        return NextFloat(100) < rate;
    }    
}