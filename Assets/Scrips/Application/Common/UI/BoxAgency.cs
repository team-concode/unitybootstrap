using System.Collections.Generic;

public class BoxAgency : Singleton<BoxAgency> {
    public HashSet<Box> realEstates = new HashSet<Box>();

    public void Add(Box item) {
        realEstates.Add(item);
    }

    public void Remove(Box item) {
        realEstates.Remove(item);
    }

    public bool Contains(float x, float y) {
        foreach (var item in realEstates) {
            if (item.Contains(x, y)) {
                return true;
            }
        }

        return false;
    }
}