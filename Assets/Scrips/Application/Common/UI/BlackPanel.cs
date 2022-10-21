using UnityEngine;
using System.Collections;
using UnityBean;
using UnityEngine.UI;

public interface BlackPanelObserver {
    void OnClickBlackPanel();
    void OnAndroidBack();
    float QueryBlackPanelAlpha();
    CanvasGroup QueryBlackTargetPanel();
}

[RequireComponent(typeof(CanvasGroup))]
public class BlackPanel : GoItem {
    [SerializeField] private Image image;
    [SerializeField] private float showDuration = 0.15f;
    private CanvasGroup cg;
    private Coroutine alphaRoutine;
    private BlackPanelObserver observer;
    private GraphicRaycaster raycaster;
    private readonly Bec checker = new Bec();

    [LazyWired] private GoPooler goPooler;

    private void Awake() {
        cg = GetComponent<CanvasGroup>();
        raycaster = GetComponent<GraphicRaycaster>();
        BeanContainer.LazyDI(this);
    }

    public void Display(BlackPanelObserver ob) {
        this.observer = ob;
        this.StopCoroutineSafe(alphaRoutine);
        if (gameObject.activeSelf == false) {
            gameObject.SetActive(true);
        }

        UpdatePosition(ob);
        if (ob != null) {
            var c = ob.QueryBlackPanelAlpha();
            if (image != null) {
                var color = new Color(0, 0, 0, c);
                image.color = color;
            }
        }
        
        raycaster.enabled = true;
        alphaRoutine = App.mainUI.Run(cg.AlphaTo(EaseType.easeInOutQuad, showDuration, 1f, () => {
            alphaRoutine = null;
        }));
    }

    public void Hide() {
        App.mainUI.Stop(alphaRoutine);
        raycaster.enabled = false;
        alphaRoutine = App.mainUI.Run(HideRoutine());
    }

    public void OnClickClose() {
        if (alphaRoutine != null) {
            return;
        }
        
        if (checker.CanEnter("Close", 0.5f) == false) {
            return;
        }

        observer?.OnClickBlackPanel();
    }

    private IEnumerator HideRoutine() {
        yield return cg.AlphaTo(EaseType.easeInOutQuad, 0.15f, 0f);
        alphaRoutine = null;
        goPooler.Return(this);
    }

    private void UpdatePosition(BlackPanelObserver ob) {
        if (ob == null) {
            return;
        }
        
        var targetPanel = ob.QueryBlackTargetPanel();
        var parent = targetPanel.transform.parent;
        var index = targetPanel.transform.GetSiblingIndex();

        if (transform.parent != parent) {
            transform.SetParent(parent);
        }

        if (transform.GetSiblingIndex() < index) {
            index--;
        }

        transform.SetSiblingIndex(index);
        SetPosition();
    }

    private void SetPosition() {
        var parent = transform.parent;
        var pos = Vector2.zero;
        while (parent != null) {
            if (parent.parent == null) {
                break;
            }
            
            pos += parent.GetComponent<RectTransform>().anchoredPosition;
            parent = parent.parent;
        }

        this.transform.localScale = Vector3.one;
        this.GetComponent<RectTransform>().anchoredPosition = -pos;
    }

    private void Update() {
        if (observer != null && Input.GetKeyDown(KeyCode.Escape)) {
            observer.OnAndroidBack();
        }
    }
}
