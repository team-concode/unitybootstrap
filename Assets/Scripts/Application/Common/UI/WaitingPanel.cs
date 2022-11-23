using UnityEngine;
using System.Collections;

public class WaitingPanel : MonoBehaviour {
    [SerializeField] private CanvasGroup panel;

    private Coroutine easeRoutine;

    public void Show() {
        panel.alpha = 0.05f;
        gameObject.SetActive(true);
        this.StopCoroutineSafe(easeRoutine);
        easeRoutine = StartCoroutine(ShowInteral());
    }

    public void Hide() {
        gameObject.SetActive(false);
    }

    private IEnumerator ShowInteral() {
        panel.alpha = 0.05f;
        yield return new WaitForSecondsRealtime(0.1f);
        yield return StartCoroutine(panel.AlphaTo(EaseType.easeInOutQuad, 0.2f, 1, () => {}, false));
    }
}