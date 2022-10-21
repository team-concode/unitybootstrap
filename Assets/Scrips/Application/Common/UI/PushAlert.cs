using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PushAlert : GoItem {
    [SerializeField] private Text titleText;
    [SerializeField] private Text bodyText;
    [SerializeField] private RectTransform body;

    private Coroutine moveRoutine;
    private bool isShown;

    private GoPooler goPooler => UnityBean.BeanContainer.GetBean<GoPooler>();

    public void Display(string title, string message) {
        isShown = true;
        titleText.text = title;
        bodyText.text = message;

        body.anchoredPosition = new Vector2(0, 310);
        body.localScale = Vector3.one;
        moveRoutine = App.mainUI.Run(body.MoveTo(EaseType.easeOutQuad, 0.5f, Vector3.zero));
    }

    public void OnClickHide() {
        if (isShown) {
            isShown = false;
            App.mainUI.Stop(moveRoutine);
            App.mainUI.Run(Hide());
        }
    }

    private IEnumerator Hide() {
        yield return body.MoveTo(EaseType.easeOutQuad, 0.5f, new Vector2(0, 310));
        goPooler.Return(this);
    }
}