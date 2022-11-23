using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

public interface IPopupPanel {
    Canvas canvas { get; }
    RectTransform rt { get; }
    bool isShown { get; }

    void Display(object param);
    bool Hide();
    void Refresh();
    MonoBehaviour GetMono();
    IEnumerator WaitForClose();
    void OnBack();
    void OnTop();
    void OnNavigate(RectTransform item);
    int GetKey();
}

public class PopupBase : GoItem, IPopupPanel {
    public Canvas canvas { get; private set; }
    public RectTransform rt { get; private set; }
    public bool isShown { get; protected set; }
    public GameObject lastFocused { get; protected set; }
    
    private PopupPanelEaser easer;
    private Dictionary<Button, bool> buttons = new();

    protected virtual void Awake() {
        canvas = GetComponent<Canvas>();
        rt = GetComponent<RectTransform>();
        this.SetActive(this.gameObject, false);
    }

    public virtual int GetKey() {
        return 0;
    }

    public virtual void Display(object param) {
        RestoreButtons();
        lastFocused = null;
        OnDisplay(param);
        Show();
        this.RunNextFrame(OnSelect);
    }

    public virtual void Refresh() {
    }

    protected virtual void OnDisplay(object param) {
    }

    protected virtual void OnHide() {
        EventSystem.current.SetSelectedGameObject(null);
    }
    
    protected virtual PopupPanelEaser GetPopupEaser() {
        return new PopupPanelEaserDefault(this);
    }

    protected virtual void Show() {
        if (isShown) {
            return;
        }

        easer ??= GetPopupEaser();
        easer.Show();
        isShown = true;
        App.mainUI.AddPopup(this);
    }

    protected bool CanHide() {
        if (!isShown) {
            return false;
        }

        if (easer != null && easer.IsWorking()) {
            return false;
        }

        return true;
    }

    public virtual bool Hide() {
        if (!CanHide()) {
            return false;
        }
        
        OnHide();
        App.mainUI.RemovePopup(this);
        isShown = false;
        easer ??= new PopupPanelEaserDefault(this);
        easer.Hide(() => {
            this.gameObject.SetActive(false);
        });
        return true;
    }

    public MonoBehaviour GetMono() {
        return this;
    }
    
    public IEnumerator WaitForClose() {
        while (isShown) {
            yield return null;
        }
    }

    protected virtual void OnSelect() {
        var top = this.GetComponentInChildren<Button>();
        if (top != null) {
            top.Select();
        }
    }

    public virtual void OnBack() {
        var all = this.GetComponentsInChildren<Button>();
        foreach (var button in all) {
            buttons[button] = button.enabled;
            button.enabled = false;
        }

        lastFocused = EventSystem.current.currentSelectedGameObject;
        EventSystem.current.SetSelectedGameObject(null);
    }
    
    public virtual void OnTop() {
        RestoreButtons();
        if (lastFocused != null && lastFocused.activeInHierarchy) {
            EventSystem.current.SetSelectedGameObject(lastFocused);
        } else {
            var top = this.GetComponentInChildren<Button>();
            if (top != null) {
                top.Select();
            }
        }
    }

    protected void RestoreButtons() {
        foreach (var button in buttons) {
            button.Key.enabled = button.Value;
        }

        buttons.Clear();
    }

    public virtual void OnNavigate(RectTransform item) {
    }
}

public class PopupBaseBP : PopupBase, BlackPanelObserver {
    private CanvasGroup panel;
    private GraphicRaycaster raycaster;

    protected override void Awake() {
        base.Awake();
        panel = GetComponent<CanvasGroup>();
        raycaster = GetComponent<GraphicRaycaster>();
    } 

    protected override void OnDisplay(object param) {
        this.SetActive(this.gameObject, true);
        App.mainUI.ShowBlackPanel(this);
    }
    
    protected override void Show() {
        base.Show();
        if (raycaster) {
            raycaster.enabled = true;
        }
    }
    
    public override bool Hide() {
        var res = base.Hide();
        if (!res) {
            return false;
        }
        
        App.mainUI.HideBlackPanel(this);
        if (raycaster) {
            raycaster.enabled = false;
        }

        return true;
    }

    public virtual void OnClickBlackPanel() {
        Hide();
    }
    
    public virtual void OnAndroidBack() {
        if (!App.mainUI.IsTopPopup(this)) {
            return;
        }
        
        Hide();
    }

    public virtual float QueryBlackPanelAlpha() {
        return 0.8f;
    }

    public CanvasGroup QueryBlackTargetPanel() {
        return panel;
    }
}

    
    
public interface PopupPanelEaser {
    void Show();
    void Hide(Action onDone);
    bool IsWorking();
}

public class PopupPanelEaserDefault : PopupPanelEaser {
    private IPopupPanel target;
    
    public PopupPanelEaserDefault(IPopupPanel target) {
        this.target = target;
    }
    
    public void Show() {
        target.rt.gameObject.SetActive(true);
    }
    
    public void Hide(Action onDone) {
        target.rt.gameObject.SetActive(false);
        onDone?.Invoke();
    }

    public bool IsWorking() {
        return false;
    }
}


public class PopupPanelEaserDelay : PopupPanelEaser {
    private IPopupPanel target;
    private Coroutine showRoutine;
    private float delay;
    
    public PopupPanelEaserDelay(IPopupPanel target, float delay) {
        this.target = target;
        this.delay = delay;
    }
    
    public void Show() {
        target.rt.gameObject.SetActive(true);
        target.GetMono().StopCoroutineSafe(showRoutine);
    }
    
    public void Hide(Action onDone) {
        showRoutine = target.GetMono().StartCoroutine(HideRoutine(onDone));
    }

    private IEnumerator HideRoutine(Action onDone) {
        yield return new WaitForSecondsRealtime(delay);
        target.rt.gameObject.SetActive(false);
        onDone?.Invoke();
    }

    public bool IsWorking() {
        return false;
    }
}


public class PopupPanelEaserBottomUp : PopupPanelEaser {
    private Coroutine popupRoutine;
    private RectTransform anchor;
    private RectTransform parent;
    private CanvasGroup parentCg;
    private float duration = 0.25f;
    private IPopupPanel target;
    private Vector2 toPos;

    public PopupPanelEaserBottomUp(IPopupPanel target, RectTransform anchor, RectTransform parent, Vector2 toPos) {
        this.target = target;
        this.anchor = anchor;
        this.parent = parent;
        this.toPos = toPos;
        if (parent != null) {
            this.parentCg = parent.GetComponent<CanvasGroup>();
        }
        anchor.anchoredPosition = new Vector2(0, -GetScreenHeight());
    }

    public void Show() {
        target.canvas.enabled = true;
        target.GetMono().StopCoroutineSafe(popupRoutine);
        popupRoutine = target.GetMono().StartCoroutine(ShowRoutine());
    }

    public void Hide(Action onDone) {
        target.GetMono().StopCoroutineSafe(popupRoutine);
        popupRoutine = target.GetMono().StartCoroutine(HideRoutine(onDone));
    }

    private float GetScreenHeight() {
        return App.mainUI.canvasScaler.GetOriginSize().y;
    }

    public bool IsWorking() {
        return popupRoutine != null;
    }

    private IEnumerator ShowRoutine() {
        var from = anchor.anchoredPosition;
        yield return anchor.RunEase(EaseType.easeOutQuad, duration, v => {
            anchor.anchoredPosition = Vector2.Lerp(from, toPos, v);
            if (parentCg != null) {
                parentCg.alpha = Mathf.Lerp(1, 0, v);
                parent.localScale = Vector2.Lerp(Vector2.one, Vector2.one * 0.9f, v);
            }
        }, () => {
            popupRoutine = null;
        }, false);
    }

    private IEnumerator HideRoutine(Action onDone) {
        var from1 = anchor.anchoredPosition;
        var to1 = new Vector2(0, -GetScreenHeight());
        yield return anchor.RunEase(EaseType.easeOutQuad, duration, v => {
            anchor.anchoredPosition = Vector2.Lerp(from1, to1, v);
            if (parentCg != null) {
                parentCg.alpha = Mathf.Lerp(0, 1, v);
                parent.localScale = Vector2.Lerp(Vector2.one * 0.9f, Vector2.one, v);
            }
        }, () => {
            popupRoutine = null;
        }, false);

        target.canvas.enabled = false;
        onDone?.Invoke();
    }
}


public class PopupPanelEaserPop : PopupPanelEaser {
    private Coroutine popupRoutine;
    private RectTransform parent;
    private CanvasGroup cg;
    private CanvasGroup parentCg;
    private float duration = 0.15f;
    private IPopupPanel target;
    
    private float minimumDisplay = 0.25f;
    private float lastShow = 0;

    public PopupPanelEaserPop(IPopupPanel target, RectTransform parent) {
        this.target = target;
        this.cg = target.rt.GetComponent<CanvasGroup>();
        
        this.parent = parent;
        if (parent != null) {
            this.parentCg = parent.GetComponent<CanvasGroup>();
        }
    }

    public void Show() {
        target.canvas.enabled = true;
        target.GetMono().StopCoroutineSafe(popupRoutine);
        popupRoutine = target.GetMono().StartCoroutine(ShowRoutine());
        lastShow = Time.realtimeSinceStartup;
    }

    public void Hide(Action onDone) {
        target.GetMono().StopCoroutineSafe(popupRoutine);
        popupRoutine = target.GetMono().StartCoroutine(HideRoutine(onDone));
    }

    public void SetMinimumDisplay(float duration) {
        minimumDisplay = duration;
    }

    public bool IsWorking() {
        var past = Time.realtimeSinceStartup - lastShow;
        if (past < minimumDisplay) {
            return true;
        }
        
        return popupRoutine != null;
    }

    private IEnumerator ShowRoutine() {
        cg.alpha = 0;
        var from = Vector2.one * 0.95f;
        target.rt.localScale = from;
        yield return target.rt.RunEase(EaseType.easeOutQuad, duration, v => {
            cg.alpha = Mathf.Lerp(0, 1, v);
            target.rt.localScale = Vector3.Lerp(from, Vector3.one, v);
            if (parentCg != null) {
                parent.localScale = Vector2.Lerp(Vector2.one, Vector2.one * 0.95f, v);
            }
        }, () => {
            popupRoutine = null;
        }, false);
    }

    private IEnumerator HideRoutine(Action onDone) {
        var fromAlpha = cg.alpha;
        var from1 = target.rt.localScale;
        yield return target.rt.RunEase(EaseType.easeOutQuad, duration, v => {
            cg.alpha = Mathf.Lerp(fromAlpha, 0, v);
            target.rt.localScale = Vector3.Lerp(from1, Vector3.one * 0.95f, v);
            if (parentCg != null) {
                parent.localScale = Vector2.Lerp(Vector2.one * 0.95f, Vector2.one, v);
            }
        }, () => {
            popupRoutine = null;
        }, false);

        target.canvas.enabled = false;
        onDone?.Invoke();
    }
}


public class PopupPanelEaserBubble : PopupPanelEaser {
    private Coroutine popupRoutine;
    private Coroutine bornRoutine;
    private RectTransform parent;
    private CanvasGroup cg;
    private CanvasGroup parentCg;
    private float duration = 0.15f;
    private IPopupPanel target;

    public PopupPanelEaserBubble(IPopupPanel target, RectTransform parent) {
        this.target = target;
        this.cg = target.rt.GetComponent<CanvasGroup>();
        
        this.parent = parent;
        if (parent != null) {
            this.parentCg = parent.GetComponent<CanvasGroup>();
        }
    }

    public void Show() {
        target.canvas.enabled = true;
        target.GetMono().StopCoroutineSafe(popupRoutine);
        target.GetMono().StopCoroutineSafe(bornRoutine);
        popupRoutine = target.GetMono().StartCoroutine(ShowRoutine());
    }

    public void Hide(Action onDone) {
        target.GetMono().StopCoroutineSafe(popupRoutine);
        target.GetMono().StopCoroutineSafe(bornRoutine);
        popupRoutine = target.GetMono().StartCoroutine(HideRoutine(onDone));
    }

    public bool IsWorking() {
        return popupRoutine != null;
    }

    private IEnumerator ShowRoutine() {
        cg.alpha = 0;
        bornRoutine = target.GetMono().StartCoroutine(BornAnim());
        yield return target.rt.RunEase(EaseType.easeOutQuad, duration, v => {
            cg.alpha = Mathf.Lerp(0, 1, v);
            if (parentCg != null) {
                parent.localScale = Vector2.Lerp(Vector2.one, Vector2.one * 0.95f, v);
            }
        }, () => {
            popupRoutine = null;
        }, false);
    }

    private IEnumerator HideRoutine(Action onDone) {
        var fromAlpha = cg.alpha;
        var from1 = target.rt.localScale;
        yield return target.rt.RunEase(EaseType.easeOutQuad, duration, v => {
            cg.alpha = Mathf.Lerp(fromAlpha, 0, v);
            target.rt.localScale = Vector3.Lerp(from1, Vector3.one * 0.95f, v);
            if (parentCg != null) {
                parent.localScale = Vector2.Lerp(Vector2.one * 0.95f, Vector2.one, v);
            }
        }, () => {
            popupRoutine = null;
        }, false);

        target.canvas.enabled = false;
        onDone?.Invoke();
    }
    
    private IEnumerator BornAnim() {
        var size = target.rt.sizeDelta;
        var scale = size;
        var min = Mathf.Min(scale.x, scale.y);
        scale.x = min / scale.x;
        scale.y = min / scale.y;
        scale *= 400f / min;

        var tf = target.rt.transform;
        var to1 = Vector2.one + new Vector2(0.03f, -0.03f) * scale;
        yield return tf.ScaleTo(EaseType.easeOutQuad, 0.1f, to1);

        var to2 = Vector2.one + new Vector2(-0.025f, 0.025f) * scale;
        yield return tf.ScaleTo(EaseType.easeOutQuad, 0.05f, to2);
    
        var to3 = Vector2.one + new Vector2(0.02f, -0.02f) * scale;
        yield return tf.ScaleTo(EaseType.easeOutQuad, 0.03f, to3);

        var to4 = Vector2.one;
        yield return tf.ScaleTo(EaseType.easeOutQuad, 0.02f, to4);
    }
}

