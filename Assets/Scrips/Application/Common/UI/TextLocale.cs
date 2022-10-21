using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TextLocale : MonoBehaviour {
    [SerializeField] private string key;

    private static HashSet<TextLocale> textLocales = new HashSet<TextLocale>();
    private StringBundleService sb => UnityBean.BeanContainer.GetBean<StringBundleService>();

    private void Awake() {
        if (!App.ready) {
            return;
        }
        
        if (sb.isReady) {
            Refresh();
        }
        textLocales.Add(this);
    }

    private void OnDestroy() {
        textLocales.Remove(this);
    }

    public void Refresh() {
        GetComponent<Text>().text = sb.Get(key);
    }

    public static void RefreshAll() {
        foreach (var item in textLocales) {
            item.Refresh();
        }
    }
}