using UnityEngine;
using UnityEngine.UI;

public class GameLogo : MonoBehaviour {
    [SerializeField] private RectTransform bgImage;
    [SerializeField] private Image logoIcon;
    [SerializeField] private Sprite title;
    [SerializeField] private Sprite titleKo;
    [SerializeField] private Sprite titleJp;

    private void Start() {
        var code = StringBundleService.GetLanguageCode();
        if (code == "ko") {
            logoIcon.sprite = titleKo;
        } else if (code == "ja") {
            logoIcon.sprite = titleJp;
        } else {
            logoIcon.sprite = title;
        }

        if (Screen.width < Screen.height) {
            bgImage.anchoredPosition = new Vector2(375, 0);
        } else {
            bgImage.anchoredPosition = new Vector2(0, 0);
        }
    }
}
