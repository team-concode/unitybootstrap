using UnityBean;
using UnityEngine;

public class TitleUIPresenter : UIPresenterBase {
    [SerializeField] private GameObject mainAnchor;
    [SerializeField] private GameObject alertAnchor;
    [SerializeField] private GameObject waitingAnchor;
    [SerializeField] private GameObject pushAnchor;

    [LazyWired] private SoundService soundService;
    [LazyWired] private SettingService settingService;

    public override SceneType sceneType => SceneType.Title;
    public override GameObject defaultUI => mainAnchor;
    public override GameObject alertLayer => alertAnchor;
    public override GameObject waitingLayer => waitingAnchor;
    public override GameObject pushLayer => pushAnchor;
    public override BlinkPanel blinkPanel => null;
    
    private static bool logoDisplayed;

    public void Start() {
        Initialize();
        if (App.ready) {
            BeanContainer.LazyDI(this);
            soundService.PlayBGM("Bgm/Mode");
        }
    }

    public async void OnClickStart() {
        if (Bec.instance.Can()) {
            await SwitchScene(SceneType.World);
        }
    }
}
