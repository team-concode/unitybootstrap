using UnityEngine;
using System;
using Newtonsoft.Json;

public class LocalSerializer : Singleton<LocalSerializer> {
    public T Load<T>(string path) where T : class {
        T result;

        string text = PersistenceUtil.LoadTextFile(path);
        if (text.Length == 0) {
            return null;
        }

        try {
            result = JsonConvert.DeserializeObject<T>(text);
        } catch (Exception e) {
            Debug.LogError(e.ToString());
            return null;
        }

        return result;
    }

    public void Save<T>(string path, T target) where T : class {
        if (target == null) {
            return;
        }

        try {
            string text = JsonConvert.SerializeObject(target);
            PersistenceUtil.SaveTextFile(path, text);
        } catch (Exception e) {
            Debug.LogError(e.ToString());
        }
    }

    public void SaveText(string path, string text) {
        PersistenceUtil.SaveTextFile(path, text);
    }

    public string Build<T>(T target) where T : class {
        try {
            return JsonConvert.SerializeObject(target);
        }
        catch (Exception e) {
            Debug.LogError(e.ToString());
        }

        return null;
    }

    public T Parse<T>(string json) where T : class {
        if (string.IsNullOrEmpty(json)) {
            return null;
        }

        T result;
        try {
            result = JsonConvert.DeserializeObject<T>(json);
        } catch (Exception e) {
            Debug.LogError(e.ToString());
            return null;
        }

        return result;
    }
}