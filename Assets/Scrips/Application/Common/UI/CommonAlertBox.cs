using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityBean;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum AlertBoxType {
    Ok = 0,
    OkCancel,
    YesNo,
    Retry,
    Update,
}

public enum AlertBoxResult {
    None = 0,
    Ok,
    Cancel,
    Yes,
    No,
    Shop,
}

public class AlertBoxOutResult : OutResult<AlertBoxResult> {
}

public class CommonAlertBox : MonoBehaviour, BlackPanelObserver {
    [SerializeField] private Text textLabel;
    [SerializeField] private Transform bottom;
    [LazyWired] private SoundService soundService;
    
    private AlertBoxResult result = AlertBoxResult.None;
    private CanvasGroup panel;
    private RectTransform rectTransform;
    private Dictionary<AlertBoxType, Transform> footers = new();
    private AlertBoxType type;

    private void Awake() {
        BeanContainer.LazyDI(this);
        panel = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();

        foreach (Transform child in bottom) {
            var type = EnumUtil.To<AlertBoxType>(child.name);
            footers.Add(type, child);
        }
    }

    private void OnDestroy() {
        if (App.mainUI != null) {
            App.mainUI.currentCommonAlert = null;
        }
    }

    public void OnClickOk() {
        soundService.PlayFx("click");
        result = AlertBoxResult.Ok;
    }

    public void OnClickCancel() {
        soundService.PlayFx("click");
        result = AlertBoxResult.Cancel;
    }

    public void OnClickYes() {
        soundService.PlayFx("click");
        result = AlertBoxResult.Yes;
    }

    public void OnClickNo() {
        soundService.PlayFx("click");
        result = AlertBoxResult.No;
    }

    public void OnClickRetry() {
        soundService.PlayFx("click");
        result = AlertBoxResult.Ok;
    }

    public void OnClickUpdate() {
        soundService.PlayFx("click");
        Application.OpenURL(App.config.GetDownloadUrl());
        result = AlertBoxResult.Cancel;
    }

    private void OnDisable() {
        App.mainUI.currentCommonAlert = null;
        App.mainUI.HideBlackPanel(this);
    }

    private void FitSize() {
        var height = textLabel.preferredHeight;
        var sizeDelta = rectTransform.sizeDelta;
        sizeDelta.y = Mathf.Max(100, height) + 200;
        rectTransform.sizeDelta = sizeDelta;
    }

    public IEnumerator Show(string message, AlertBoxType type, AlertBoxOutResult outResult) {
        this.gameObject.SetActive(true);
        this.type = type;
        App.mainUI.currentCommonAlert = this;
        textLabel.text = message;
        foreach (var footer in footers) {
            footer.Value.gameObject.SetActive(false);
        }

        if (footers.TryGetValue(type, out var current)) {
            current.gameObject.SetActive(true);
            var count = current.transform.childCount;
            if (count > 0) {
                var lastButton = current.transform.GetChild(count - 1);
                lastButton.GetComponent<Button>().Select();
            }
        }
        
        FitSize();
        panel.alpha = 0;
        panel.transform.localScale = Vector3.one * 0.95f;
        yield return null;
        App.mainUI.ShowBlackPanel(this);
        StartCoroutine(panel.AlphaTo(EaseType.easeOutQuad, 0.1f, 1f, () => {}, false));
        yield return panel.gameObject.ScaleTo(EaseType.easeOutQuad, 0.2f, Vector3.one, false);
        
        while (this.result == AlertBoxResult.None) {
            yield return null;
        }

        if (outResult != null) {
            outResult.value = this.result;
        }

        yield return panel.gameObject.ScaleTo(EaseType.easeInQuad, 0.1f, Vector3.one * 0.95f, false);
        App.mainUI.currentCommonAlert = null;
        EventSystem.current.SetSelectedGameObject(null);
        Destroy(this.gameObject);
    }

    // BlackPanelObserver interface implementation
    //-------------------------------------------------------------------------
    public void OnClickBlackPanel() {
        if (this.type == AlertBoxType.YesNo) {
            return;
        }
        result = AlertBoxResult.Cancel;
    }

    public void OnAndroidBack() {
        if (this.type == AlertBoxType.YesNo) {
            return;
        }

        result = AlertBoxResult.Cancel;
    }

    public CanvasGroup QueryBlackTargetPanel() {
        return panel;
    }

    public float QueryBlackPanelAlpha() {
        return 0.8f;
    }
}
