using UnityEngine;

public class GoItem : MonoBehaviour {
    public int goNo { get; set; }
    public int useCount { get; set; }
    public bool isInPool { get; set; }
    public string resourcePath { get; set; }
    public GoPool pool { get; set; }

    public bool IsActive() { return gameObject.activeSelf; }
    public virtual void Prepare() {}
    public virtual void OnGettingOutPool() { }
    public virtual void OnGoingIntoPool() { }
}

public class GoItemRef<T> where T : GoItem {
    private T target;
    private int useCount;

    public GoItemRef() {
    }

    public GoItemRef(T target) {
        this.target = target;
        this.useCount = target?.useCount ?? 0;
    }

    public T Get() {
        if (target == null) {
            return null;
        }

        if (target.useCount != useCount) {
            return null;
        }

        if (target.isInPool) {
            return null;
        }

        return target;
    }

    public static implicit operator T(GoItemRef<T> goRef) {
        return goRef.Get();
    }

    public static implicit operator GoItemRef<T>(T target) {
        return new GoItemRef<T>(target);
    }
}