using UnityEngine;
using System.Collections.Generic;

public class AverageFilterVector3 {
    public class FilterItem {
        public float dt { get; set; }
        public Vector3 value { get; set; }

        public FilterItem(float dt, Vector3 value) {
            this.dt = dt;
            this.value = value;
        }
    }
    
    private LinkedList<FilterItem> raw = new();
    private float time = 1f;
    private Vector3 sumValue = Vector3.zero;
    private float sumTime = 0f;
    private Vector3 avg = Vector3.zero;

    // properties
    //-------------------------------------------------------------------------
    public Vector3 value => avg;
    public int inputCount { get; private set; }

    // public apis
    //-------------------------------------------------------------------------
    public AverageFilterVector3(float time) { 
        Clear();
        this.time = time;
    }

    public void Clear() {
        raw.Clear();
        sumValue = Vector3.zero;
        sumTime = 0f;
        inputCount = 0;
        avg = Vector3.zero;
    }

    public Vector3 Filter(Vector3 value) {
        var dt = Time.deltaTime;
        if (Time.timeScale < 0.01f) {
            return avg;
        }

        if (time < sumTime) {
            var first = raw.First.Value;
            sumTime -= first.dt;
            sumValue -= first.value;
            raw.RemoveFirst();
        }

        raw.AddLast(new FilterItem(dt, value));
        sumTime += dt;
        sumValue += value;
        avg = sumValue / raw.Count;
        inputCount++;
        return avg;
    }

    public LinkedList<FilterItem> GetRaw() {
        return raw;
    }
}


public class AverageFilterFloat {
    public class FilterItem {
        public float dt { get; set; }
        public float value { get; set; }

        public FilterItem(float dt, float value) {
            this.dt = dt;
            this.value = value;
        }
    }
    
    private LinkedList<FilterItem> raw = new();
    private float time = 1f;
    private float sumValue = 0f;
    private float sumTime = 0f;
    private float avg = 0f;

    // properties
    //-------------------------------------------------------------------------
    public float value => avg;
    public int inputCount { get; private set; }

    // public apis
    //-------------------------------------------------------------------------
    public AverageFilterFloat(float time) { 
        Clear();
        this.time = time;
    }

    public void Clear() {
        raw.Clear();
        sumValue = 0f;
        sumTime = 0f;
        inputCount = 0;
        avg = 0f;
    }

    public float Filter(float value) {
        var dt = Time.deltaTime;
        if (Time.timeScale < 0.01f) {
            return avg;
        }

        if (time < sumTime) {
            var first = raw.First.Value;
            sumTime -= first.dt;
            sumValue -= first.value;
            raw.RemoveFirst();
        }

        raw.AddLast(new FilterItem(dt, value));
        sumTime += dt;
        sumValue += value;
        avg = sumValue / raw.Count;
        inputCount++;
        return avg;
    }
}
