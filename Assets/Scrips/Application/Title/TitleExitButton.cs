using UnityEngine;
using UnityEngine.UI;

public class TitleExitButton : MonoBehaviour {
    private void Start() {
#if UNITY_IOS || UNITY_ANDROID
        gameObject.SetActive(false);
        return;
#endif
        var canvasGroup = GetComponent<CanvasGroup>();
        var button = GetComponentInChildren<Button>();

        canvasGroup.alpha = 0;
        button.enabled = false;

        this.RunAfter(0.5f, () => {
            this.Run(canvasGroup.AlphaTo(EaseType.easeInQuad, 0.5f, 1f, () => {
                button.enabled = true;
            }));
        });
    }

    public async void OnClickButton() {
        var res = await App.mainUI.ShowAlertKey("applicaiton.quit", AlertBoxType.YesNo);
        if (res == AlertBoxResult.Yes) {
            Application.Quit();
        } else {
            var button = this.GetComponent<Button>();
            button.Select();
        }
    } 
}
