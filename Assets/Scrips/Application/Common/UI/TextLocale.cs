using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TextLocale : MonoBehaviour {
    [SerializeField] private string key;

    private static HashSet<TextLocale> all = new();
    private StringBundleService sb => UnityBean.BeanContainer.GetBean<StringBundleService>();

    private void Awake() {
        if (!App.ready) {
            return;
        }
        
        if (sb.isReady) {
            Refresh();
        }

        all.Add(this);
    }

    private void OnDestroy() {
        all.Remove(this);
    }

    private void Refresh() {
        GetComponent<Text>().text = sb.Get(key);
    }

    public static void RefreshAll() {
        foreach (var item in all) {
            item.Refresh();
        }
    }
}