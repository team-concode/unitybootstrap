using UnityBean;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class TextOutFitterLimit : MonoBehaviour {
    [SerializeField] private Text target;
    [SerializeField] private Vector2 padding;
    [SerializeField] private Vector2 margin;
    [SerializeField] private float minWidth = 80f;
    [SerializeField] private float maxWidth = 360f;
    [SerializeField] private string locale = "";

    [LazyWired] private StringBundleService sbService;

    public RectTransform rect { get; private set; }
    
    private Box box;
    
    private void Awake() {
        if (!App.ready) {
            return;
        }
        
        rect = GetComponent<RectTransform>();
        var parent = transform.parent;
        while (parent != null) {
            box = transform.parent.GetComponent<Box>();
            if (box != null) {
                break;
            }

            parent = parent.parent;
        }
        
        if (!string.IsNullOrEmpty(locale)) {
            BeanContainer.LazyDI(this);
            SetText(sbService.Get(locale));
        }
    }

    private void Start() {
        if (!App.ready) {
            return;
        }

        if (!string.IsNullOrEmpty(locale)) {
            SetText(sbService.Get(locale));
        }
    }

    public void SetText(string text) {
        target.text = text;
        RefreshSize();
    }

    public void SetColor(Color color) {
        target.color = color;
    }

    public void RefreshSize() {
        if (target == null || rect == null) {
            return;
        }

        target.rectTransform.sizeDelta = new Vector2(720, 400);
        var width = Mathf.Max(minWidth, target.preferredWidth);
        var height = target.preferredHeight;
        
        if (maxWidth < width) {
            target.rectTransform.sizeDelta = new Vector2(maxWidth, height);
            width = maxWidth;
            height = target.preferredHeight;
        }

        var scale = target.transform.localScale;
        target.rectTransform.sizeDelta = new Vector2(width + 0.001f, height + 0.001f);
        rect.sizeDelta = new Vector2(width * scale.x + padding.x, height * scale.y + padding.y);

        if (box != null) {
            box.Arrange();
        }
    }

    public Vector2 GetSize() {
        return rect.sizeDelta + padding + margin;
    }

    public Vector2 GetContentSize() {
        return rect.sizeDelta;
    }

    public void SetPosition(Vector2 pos) {
        rect.anchoredPosition = pos;
    }

    public Text GetText() {
        return target;
    }
    
#if UNITY_EDITOR
    private void Update() {
        if (!Application.isPlaying) {
            RefreshSize();
        }
    }
#endif
}