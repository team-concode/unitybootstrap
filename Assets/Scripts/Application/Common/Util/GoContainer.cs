using System;
using Newtonsoft.Json;
using UnityEngine;

public class GoContainer : MonoBehaviour {
    public Action onAwake;
    public Action onStart;
    public Action onEnable;
    public Action onDisable;
    public Action onDestroy;
    public Action onUpdate;
    public EventBusTopic<NativeMessage> onNative = new();
    
    private void Awake() {
        onAwake?.Invoke();
    }

    private void Start() {
        onStart?.Invoke();
    }
    
    private void Update() {
        onUpdate?.Invoke();
    }

    private void OnEnable() {
        onEnable?.Invoke();
    }

    private void OnDisable() {
        onDisable?.Invoke();
    }

    private void OnDestroy() {
        onDestroy?.Invoke();
    }

    public void OnNative(string message) {
        onNative.Fire(JsonConvert.DeserializeObject<NativeMessage>(message));
    }

    public static GoContainer New(string name, bool dontDestroyOnLoad = false) {
        var go = new GameObject();
        go.name = name;
        if (dontDestroyOnLoad) {
            DontDestroyOnLoad(go);
        }
        
        return go.AddComponent<GoContainer>();
    }
}
