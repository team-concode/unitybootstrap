using System.Threading.Tasks;
using UnityEngine;

public class SceneTransitionEffect : MonoBehaviour, BlackPanelObserver {
    private CanvasGroup cg;
    private Canvas canvas;

    private void Awake() {
        //DontDestroyOnLoad(gameObject);
        canvas = GetComponent<Canvas>();
        cg = GetComponent<CanvasGroup>();
    }

    public async Task Display() {
        canvas.enabled = true;
        gameObject.SetActive(true);
        await new WaitForEndOfFrame();

        cg.alpha = 0;
        await cg.AlphaTo(EaseType.easeInQuad, 0.4f, 1);
        await new WaitForSeconds(0.75f);
        await Hide();
    }

    public async Task Hide() {
        await new WaitForSeconds(0.2f);
        await cg.AlphaTo(EaseType.easeInQuad, 0.4f, 0);
        //Destroy(gameObject);
    }

    public void OnClickBlackPanel() {
    }

    public void OnAndroidBack() {
    }

    public float QueryBlackPanelAlpha() {
        return 0.8f;
    }

    public CanvasGroup QueryBlackTargetPanel() {
        return cg;
    }
}