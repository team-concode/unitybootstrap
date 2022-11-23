using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Spinner : MonoBehaviour {
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image icon;
    [SerializeField] private bool autoStart = true;

    private Coroutine showRoutine1;
    private Coroutine showRoutine2;

    private void Start() {
        if (autoStart) {
            Display();
        }
    }
    
    public void Display() {
        this.StopCoroutineSafe(showRoutine1);
        this.StopCoroutineSafe(showRoutine2);
        showRoutine2 = StartCoroutine(ShowRoutine());
    }

    public void Hide() {
        this.StopCoroutineSafe(showRoutine1);
        showRoutine1 = StartCoroutine(HideRoutine());
    }

    private IEnumerator ShowRoutine() {
        canvasGroup.alpha = 0;
        showRoutine1 = StartCoroutine(canvasGroup.AlphaTo(EaseType.easeOutQuad, 0.2f, 1f));

        int index = 0;
        while (true) {
            if (index % 2 == 1) {
                icon.rectTransform.localScale = new Vector3(-1, 1, 1);
            } else {
                icon.rectTransform.localScale = Vector3.one;
            }

            var ease = new EaseRunner(EaseType.linear, 1);
            while (ease.IsPlaying()) {
                if (index % 2 == 1) {
                    icon.fillAmount = ease.Run();
                } else {
                    icon.fillAmount = 1 - ease.Run();
                }

                yield return null;
            }

            index++;
        }
    }

    private IEnumerator HideRoutine() {
        yield return canvasGroup.AlphaTo(EaseType.easeOutQuad, 0.2f, 0f);
        this.StopCoroutineSafe(showRoutine2);
        this.gameObject.SetActive(false);
    }
}