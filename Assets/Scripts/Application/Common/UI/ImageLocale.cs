using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[Serializable]
public class ImageLocaleData {
    public string lang;
    public Sprite sprite;
}

public class ImageLocale : MonoBehaviour {
    [SerializeField] private List<ImageLocaleData> data;
    
    private static HashSet<ImageLocale> all = new();
    private StringBundleService sb => UnityBean.BeanContainer.GetBean<StringBundleService>();

    private void Awake() {
        all.Add(this);
        if (!App.ready) {
            return;
        }
        
        if (sb.isReady) {
            Refresh();
        }
    }

    private void OnDestroy() {
        all.Remove(this);
    }

    private void Refresh() {
        foreach (var item in data) {
            if (item.lang == sb.language) {
                GetComponent<Image>().sprite = item.sprite;
                break;
            }
        }
    }

    public static void RefreshAll() {
        foreach (var item in all) {
            item.Refresh();
        }
    }
}