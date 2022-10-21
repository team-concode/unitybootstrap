using UnityBean;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class IntroUIPresenter : UIPresenterBase {
    [SerializeField] private GameObject mainAnchor;
    [SerializeField] private GameObject alertAnchor;
    [SerializeField] private GameObject waitingAnchor;
    [SerializeField] private GameObject pushAnchor;
    [SerializeField] private SceneTransitionEffect loading;
    [SerializeField] private Animator logo;

    [LazyWired] private SoundService soundService;
    [LazyWired] private SettingService settingService;

    public override SceneType sceneType => SceneType.Intro;
    public override GameObject defaultUI => mainAnchor;
    public override GameObject alertLayer => alertAnchor;
    public override GameObject waitingLayer => waitingAnchor;
    public override GameObject pushLayer => pushAnchor;
    public override BlinkPanel blinkPanel => null;
    
    private static bool logoDisplayed;

    public async void Start() {
        Initialize();
        
        App.version = new Version(Application.version);
        
        // load settings
        ReadyPhase();
        var bootResult = await BeanContainer.Initialize((bean) => {
            log.debug("Starting " + bean);
        }, (bean) => {
            log.debug("Success starting " + bean);
        }, (bean) => {
            log.debug("Failed starting " + bean);
            AlertContainerFailed(bean);
        });

        if (!bootResult) {
            return;
        }

        BeanContainer.LazyDI(this);
        if (!logoDisplayed) {
            await new WaitForSeconds(0.1f);
            logo.CrossFade("Play", 0, -1, 0f);
            soundService.PlayFx("Fx/points");
            
            await new WaitForSeconds(1f);
            logoDisplayed = true;
        }
        
        // update text bundle
        TextLocale.RefreshAll();

        await new WaitForSeconds(0.5f);
        
        App.ready = true;
        RefreshScreen();
        MoveToNextScene();
    }

    private void MoveToNextScene() {
        var scene = settingService.value.shared.nextScene;
        if (scene != SceneType.World) {
            scene = SceneType.Title;
        }

        //SwitchScene(scene, loading).RunAsync();
    }

    private void RefreshScreen() {
        if (!ScreenUtil.IsTablet()) {
            return;
        }
        
#if UNITY_STANDALONE
        UpdateResolution();
#else
        if (settingService.value.landscape) {
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.orientation = ScreenOrientation.AutoRotation;
        } else {
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = true;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.orientation = ScreenOrientation.AutoRotation;
        }
#endif
    }

    private void UpdateResolution() {
        var quality = settingService.value.graphicQuality;
        var width = (float)Screen.width;
        var height = (float)Screen.height;

        var targetWidth = Screen.width;
        if (quality == SettingGraphicQuality.Best) {
        } else if (quality == SettingGraphicQuality.Good) {
            targetWidth = 1920;
        } else if (quality == SettingGraphicQuality.Low) {
            targetWidth = 1280;
        }

        targetWidth = Mathf.RoundToInt(Mathf.Min(width, targetWidth));
        var scale = width / targetWidth;
        var newHeight = height * scale;
        Screen.SetResolution(targetWidth, Mathf.RoundToInt(newHeight), true, 60);        
    }

    private async void AlertContainerFailed(string bean) {
        var res = new AlertBoxOutResult();
        var key = "intro." + bean + ".failed";
        await App.mainUI.ShowAlertKey(key, AlertBoxType.Retry, res);
        if (res.value == AlertBoxResult.Ok) {
            SceneManager.LoadScene(0);
        }
    }

    private void ReadyPhase() {
        App.phase = Phase.Production;
        PersistenceUtil.CreateFolder(URL.instance.localRoot);
    }
}
