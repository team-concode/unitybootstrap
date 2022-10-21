using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(Text))]
public class TextOutFitter : MonoBehaviour {
    [SerializeField] private Vector2 padding;
    public RectTransform rect { get; private set; }
    public Text target { get; private set; }
    private Box box;

    private void Awake() {
        rect = GetComponent<RectTransform>();
        var target = GetComponent<Text>();
        var parent = transform.parent;
        while (parent != null) {
            box = transform.parent.GetComponent<Box>();
            if (box != null) {
                break;
            }

            parent = parent.parent;
        }
        target.horizontalOverflow = HorizontalWrapMode.Overflow;
        target.verticalOverflow = VerticalWrapMode.Overflow;
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
        var width = target.preferredWidth;
        var height = target.preferredHeight;
        rect.sizeDelta = new Vector2(width + padding.x, height + padding.y);
        target.rectTransform.sizeDelta = new Vector2(width + 0.001f, height);
        if (box != null) {
            box.Arrange();
        }
    }

    public Vector2 GetSize() {
        return rect.sizeDelta;
    }

    public void SetPosition(Vector2 pos) {
        rect.anchoredPosition = pos;
    }
    
#if UNITY_EDITOR
    private void Update() {
        if (!Application.isPlaying) {
            RefreshSize();
        }
    }
#endif
}