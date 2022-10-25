using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BlinkPanel : MonoBehaviour {
    [SerializeField] private Canvas cv;
    [SerializeField] private CanvasGroup cg;
    [SerializeField] private Image background;

    public IEnumerator Blink(float duration, Color color) {
        cv.enabled = true;
        background.color = color;
        cg.alpha = 1;
        yield return new WaitForSeconds(duration);
        yield return cg.AlphaTo(EaseType.easeOutQuad, 0.4f, 0);
        cv.enabled = false;
    }

    public void Begin(Color color, float alpha) {
        cv.enabled = true;
        background.color = color;
        cg.alpha = alpha;
    }

    public IEnumerator Fade(EaseType easeType, float duration, float to) {
        var from = cg.alpha;
        yield return this.RunEaseUnscaled(easeType, duration, (value) => {
            cg.alpha = Mathf.Lerp(from, to, value);
        });
    }

    public void End() {
        cv.enabled = false;
    }
}