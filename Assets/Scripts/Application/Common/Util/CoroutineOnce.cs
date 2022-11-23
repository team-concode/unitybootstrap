using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineOnce {
    private MonoBehaviour owner;
    private Dictionary<IEnumerator, Coroutine> coroutines;
    
    public CoroutineOnce(MonoBehaviour mb) {
        owner = mb;
        coroutines = new Dictionary<IEnumerator, Coroutine>();
    }

    public void Run(IEnumerator method) {
        Stop(method);
        coroutines[method] = owner.StartCoroutine(method);
    }

    public void Stop(IEnumerator method) {
        Coroutine oldRoutine;
        if (coroutines.TryGetValue(method, out oldRoutine)) {
            owner.StopCoroutine(oldRoutine);
        }
    }
}