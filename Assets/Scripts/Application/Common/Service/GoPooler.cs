using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public interface GoPool {
    void Return(GoItem item);
}

[UnityBean.Service]
public class GoPooler : GoPool {
    private Dictionary<string, Stack> storedObjects = new();
    private Dictionary<string, GameObject> storedPrefabs = new();
    private Dictionary<int, WeakReference> aliveItems = new();

    private int lastNo;

    public async Task<bool> Initialize() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        return true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (mode != LoadSceneMode.Additive) {
            foreach (var item in storedObjects.Values) {
                item.Clear();
            }

            storedObjects.Clear();
            storedPrefabs.Clear();
            aliveItems.Clear();
            lastNo = 0;
        }
    }

    public T Get<T>(string path, GameObject parent) where T : GoItem {
        if (storedObjects.ContainsKey(path)) {
            var stack = storedObjects[path];
            var stored = PopGameObjectFromStack(stack);
            if (stored != null) {
                var storedItem = stored.GetComponent<T>();
                SetReady(stored, storedItem, parent);
                aliveItems.Add(storedItem.goNo, new WeakReference(storedItem.gameObject));
                return storedItem;
            }
        }

        var newly = MakeLocal(path);
        if (newly == null) {
            return null;
        }

        var item = newly.GetComponent<T>();
        SetReady(newly, item, parent);
        aliveItems.Add(item.goNo, new WeakReference(item.gameObject));
        return item;
    }

    public T Get<T>(GameObject prefab, GameObject parent) where T : GoItem {
        string path = prefab.GetHashCode().ToString();
        storedPrefabs[path] = prefab;
        return Get<T>(path, parent);
    }

    public T Get<T>(GoItem prefab, GameObject parent) where T : GoItem {
        return this.Get<T>(prefab.gameObject, parent);
    }

    public T Get<T>(GoItem prefab, Transform parent) where T : GoItem {
        return this.Get<T>(prefab.gameObject, parent.gameObject);
    }

    public void Return<T>(List<T> items, bool clearList) where T : GoItem {
        foreach (var item in items) {
            Return(item);
        }

        if (clearList) {
            items.Clear();
        }
    }

    public void Return(GoItem item) {
        if (item == null) {
            return;
        }

        var path = item.resourcePath;
        if (string.IsNullOrEmpty(path)) {
            aliveItems.Remove(item.goNo);
            GameObject.Destroy(item.gameObject);
            return;
        }

        if (item.isInPool) {
            return;
        }

        var stack = GetStack(path);
        stack.Push(new WeakReference(item.gameObject));
        item.isInPool = true;
        item.OnGoingIntoPool();
        item.transform.localPosition = Vector3.up * 9999;
        item.gameObject.SetActive(false);
        var rb = item.GetComponent<Rigidbody>();
        if (rb != null) {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        aliveItems.Remove(item.goNo);
    }

    public IEnumerator ReturnWithDelay(GoItem item, float delay) {
        if (item == null) {
            yield break;
        }
        
        int useCount = item.useCount;
        yield return new WaitForSeconds(delay);
        if (item == null || useCount != item.useCount) {
            yield break;
        }

        Return(item);
    }

    private GameObject MakeLocal(string path) {
        GameObject itemObj;
        GameObject prefab;
        if (!storedPrefabs.TryGetValue(path, out prefab) || prefab == null) {
            prefab = Resources.Load<GameObject>(path);
            storedPrefabs[path] = prefab;
        }

        itemObj = GameObject.Instantiate(prefab) as GameObject;
        if (itemObj == null) {
            Debug.LogError("Can not load GameObject:" + path);
            return null;
        }

        var item = itemObj.GetComponent<GoItem>();
        if (item == null) {
            Debug.LogError("Doesn't have GoItem:" + path);
        }

        item.resourcePath = path;
        item.transform.localScale = Vector3.one;
        return itemObj;
    }

    private Stack GetStack(string path) {
        Stack stack = null; 
        if (storedObjects.ContainsKey(path)) {
            stack = storedObjects[path];
        } else {
            stack = new Stack();
            storedObjects[path] = stack;
        }

        return stack;
    }

    private void SetReady(GameObject itemObj, GoItem item, GameObject parent) {
        if (parent != null) {
            itemObj.transform.SetParent(parent.transform);
            itemObj.transform.SetSiblingIndex(parent.transform.childCount - 1);
        }

        itemObj.transform.localPosition = Vector3.up * 9999;
        itemObj.transform.localScale = Vector3.one;
        itemObj.SetActive(true);

        item.goNo = lastNo++;
        item.useCount++;
        item.isInPool = false;
        item.pool = this;
        item.OnGettingOutPool();
    }

    private GameObject PopGameObjectFromStack(Stack stack) {
        while (stack.Count > 0) {
            var item = (WeakReference)stack.Pop();

            if (item.Target != null) {
                return item.Target as GameObject;
            }
        }

        return null;
    }

    public List<GoItem> GetAliveItems() {
        var res = new List<GoItem>();
        foreach (var item in aliveItems.Values) {
            var gameObject = item.Target as GameObject;
            if (gameObject == null) {
                continue;
            }

            var go = gameObject.GetComponent<GoItem>();
            res.Add(go);
        }
        
        return res;
    }
}
