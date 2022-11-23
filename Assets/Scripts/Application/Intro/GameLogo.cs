using UnityEngine;

public class GameLogo : MonoBehaviour {
    [SerializeField] private RectTransform bgImage;

    private void Start() {
        bgImage.anchoredPosition = Screen.width < Screen.height ? new Vector2(375, 0) : new Vector2(0, 0);
    }
}
