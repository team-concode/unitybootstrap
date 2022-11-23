using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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

public class CommonAlertBox : GoItem, BlackPanelObserver {
    [SerializeField] private Text textLabel;
    [SerializeField] private Transform bottom;
    [LazyWired] private SoundService soundService;
    
    private AlertBoxResult result = AlertBoxResult.None;
    private CanvasGroup panel;
    private RectTransform rectTransform;
    private readonly Dictionary<AlertBoxType, Transform> footers = new();
    private AlertBoxType type;

    private void Awake() {
        BeanContainer.LazyDI(this);
        panel = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();

        foreach (Transform child in bottom) {
            footers.Add(EnumUtil.To<AlertBoxType>(child.name), child);
        }
    }

    public override void OnGoingIntoPool() {
        if (App.mainUI != null) {
            App.mainUI.currentCommonAlert = null;
        }
    }

    public void OnClickOk() {
        soundService.PlayFx("Fx/click");
        result = AlertBoxResult.Ok;
    }

    public void OnClickCancel() {
        soundService.PlayFx("Fx/click");
        result = AlertBoxResult.Cancel;
    }

    public void OnClickYes() {
        soundService.PlayFx("Fx/click");
        result = AlertBoxResult.Yes;
    }

    public void OnClickNo() {
        soundService.PlayFx("Fx/click");
        result = AlertBoxResult.No;
    }

    public void OnClickRetry() {
        soundService.PlayFx("Fx/click");
        result = AlertBoxResult.Ok;
    }

    public void OnClickUpdate() {
        soundService.PlayFx("Fx/click");
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

    public async Task<AlertBoxResult> Display(string message, AlertBoxType type) {
        this.gameObject.SetActive(true);
        this.transform.localPosition = Vector3.zero;
        this.type = type;
        this.result = AlertBoxResult.None;

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
        await new WaitForEndOfFrame();
        App.mainUI.ShowBlackPanel(this);
        StartCoroutine(panel.AlphaTo(EaseType.easeOutQuad, 0.1f, 1f));
        await panel.gameObject.ScaleTo(EaseType.easeOutQuad, 0.2f, Vector3.one, false);
        
        while (this.result == AlertBoxResult.None) {
            await new WaitForEndOfFrame();
        }

        await panel.gameObject.ScaleTo(EaseType.easeInQuad, 0.1f, Vector3.one * 0.95f, false);
        App.mainUI.currentCommonAlert = null;
        EventSystem.current.SetSelectedGameObject(null);
        this.pool.Return(this);
        return this.result;
    }

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
