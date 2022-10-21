using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CommonToastMessage : GoItem {
    [SerializeField] private Text message;
    [SerializeField] private Image background;
    private CanvasGroup panel;
    private Coroutine showRoutine;
    private Coroutine hideRoutine;
    private RectTransform rectTransform;

    private GoPooler goPooler => UnityBean.BeanContainer.GetBean<GoPooler>();

    private void Awake() {
        panel = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    public override void OnGettingOutPool() {
        showRoutine = null;
        hideRoutine = null;
    }

    public void Show(string text, Color bgColor, Color txtColor) {
        if (showRoutine == null && hideRoutine == null) {
            background.color = bgColor;
            message.color = txtColor;
            message.text = text;
            showRoutine = StartCoroutine(ShowRoutine());
        }
    }

    public void Hide() {
        if (this.gameObject.activeSelf == false) {
            return;
        }

        if (hideRoutine == null) {
            hideRoutine = StartCoroutine(HideRoutine());
            this.StopCoroutineSafe(showRoutine);
            showRoutine = null;
        }
    }

    private IEnumerator ShowRoutine() {
        rectTransform.anchoredPosition = new Vector3(0, -100);
        panel.alpha = 0;
        yield return StartCoroutine(panel.AlphaTo(EaseType.easeInQuad, 0.1f, 1f));
        yield return new WaitForSeconds(0.8f);
        Hide();
    }

    private IEnumerator HideRoutine() {
        StartCoroutine(rectTransform.MoveTo(EaseType.easeInQuad, 0.7f, new Vector3(0, 110, 0)));
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(panel.AlphaTo(EaseType.easeInQuad, 0.3f, 0f));
        hideRoutine = null;
        goPooler.Return(this);
    }
}