using System;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public abstract class UIPresenterBase : MonoBehaviour, UIPresenter {
    [SerializeField] private MyCanvasScaler _canvasScaler;
    
    public abstract SceneType sceneType { get; }
    public Canvas canvas { get; private set; }
    public abstract GameObject defaultUI { get; }
    public abstract GameObject alertLayer { get; }
    public abstract GameObject waitingLayer { get; }
    public abstract GameObject pushLayer { get; }
    public abstract BlinkPanel blinkPanel { get; }
    public List<IPopupPanel> popups { get; private set; }
    public CommonAlertBox currentCommonAlert { get; set; }
    public MyCanvasScaler canvasScaler => _canvasScaler;
    public event AlertEvent onCommonAlertStart;
    public event AlertEvent onCommonAlertEnd;

    private static SceneType returnScene = SceneType.Intro;
    private static readonly Stack<SceneType> sceneHistory = new();

    private CommonToastMessage lastToastMessage;
    private int waitingPanelRefCount;
    private Dictionary<BlackPanelObserver, GoItemRef<BlackPanel>> blackPanelMap;
    private CoroutineOnce co;
    private WaitingPanel lastWaiting;

    private StringBundleService sb => UnityBean.BeanContainer.GetBean<StringBundleService>();
    private GoPooler goPooler => UnityBean.BeanContainer.GetBean<GoPooler>();
    private HashSet<string> onDemandRenderings = new();
    private int defaultRenderFrameInterval = 1;

    protected bool Initialize(bool forceStart = false) {
        co = new CoroutineOnce(this);
        App.mainUI = this;
        blackPanelMap = new Dictionary<BlackPanelObserver, GoItemRef<BlackPanel>>();
        popups = new List<IPopupPanel>();
        Application.targetFrameRate = 60;
        Input.multiTouchEnabled = false;
        OnDemandRendering.renderFrameInterval = defaultRenderFrameInterval;

        var scene = SceneManager.GetActiveScene();
        if (scene.buildIndex != 0 && App.ready == false && !forceStart) {
            returnScene = this.sceneType;
            SceneManager.LoadScene(0);
            return false;
        }
        
        canvas = GetComponent<Canvas>();
        return true;
    }

    public void HideSceneTransitionEffect() {
    }

    public void CloseController() {
        App.mainUI = null;
    }

    public virtual void OnChangeScene(SceneType toScene) {
    }

    public async Task SwitchScene(SceneType scene, SceneTransitionEffect effect = null) {
        Time.timeScale = 1;
        OnChangeScene(scene);

        sceneHistory.Push(scene);
        if (effect != null) {
            await effect.Display();
        }

        SceneManager.LoadScene("Empty");
    }

    public static SceneType GetNextScene() {
        if (sceneHistory.Count == 0) {
            return SceneType.Intro;
        }
        
        return sceneHistory.Peek();
    }

    public async Task SwitchPrevScene(SceneTransitionEffect effect = null) {
        if (sceneHistory.Count > 0) {
            sceneHistory.Pop();
            if (effect != null) {
                await effect.Display();
            }

            SceneManager.LoadScene("Empty");
        }
    }

    public async Task SwitchSceneWithHistoryClear(SceneType scene, SceneTransitionEffect effect = null) {
        sceneHistory.Clear();
        if (effect != null) {
            await effect.Display();
        }

        SceneManager.LoadScene("Empty");
    }

    public Coroutine Run(IEnumerator iterationResult) {
        if (this.gameObject.activeSelf) {
            return StartCoroutine(iterationResult);
        }
        return null;
    }

    public void Stop(Coroutine coroutine) {
        this.StopCoroutineSafe(coroutine);
    }

    public void ShowBlackPanel(BlackPanelObserver ob) {
        if (blackPanelMap.ContainsKey(ob)) {
            return;
        }
        
        var path = "Prefabs/UI/Common/BlackPanel";
        var blackPanel = goPooler.Get<BlackPanel>(path, App.mainUI.defaultUI);
        
        blackPanel.Display(ob);
        blackPanelMap.Add(ob, new GoItemRef<BlackPanel>(blackPanel));
    }

    public void HideBlackPanel(BlackPanelObserver ob) {
        GoItemRef<BlackPanel> blackPanel; 
        blackPanelMap.TryGetValue(ob, out blackPanel);

        if (blackPanel != null && blackPanel.Get() != null) {
            blackPanel.Get().Hide();
        }

        blackPanelMap.Remove(ob);
    }    

    public async Task<AlertBoxResult> ShowAlert(string message, AlertBoxType type) {
        var res = AlertBoxResult.None;
        while (currentCommonAlert != null) {
            await new WaitForSecondsRealtime(0.1f);
        }

        if (App.mainUI == null) {
            return res;
        }

        if (!Application.isPlaying) {
            return res;
        }

        var commonAlert = goPooler.Get<CommonAlertBox>("Prefabs/UI/Common/AlertBox", alertLayer);
        if (commonAlert == null) {
            return res;
        }
        
        onCommonAlertStart?.Invoke();
        res = await commonAlert.Display(message, type);
        onCommonAlertEnd?.Invoke();
        return res;
    }

    public async Task<AlertBoxResult> ShowAlertKey(string key, AlertBoxType type) {
        string message = sb.Get(key);
        return await ShowAlert(message, type);
    }
    
    public virtual void ShowWaitingPanel() {
        waitingPanelRefCount++;
        if (lastWaiting != null) {
            if (!lastWaiting.gameObject.activeSelf) {
                lastWaiting.Show();
            }
            return;
        }
        
        var path = "Common/WaitingPanel";
        lastWaiting = this.InstantiateUI<WaitingPanel>(path, waitingLayer);
        lastWaiting.gameObject.name = "Waiting";
        lastWaiting.Show();
    }

    public virtual void HideWaitingPanel() {
        waitingPanelRefCount--;
        if (waitingPanelRefCount < 0) {
            waitingPanelRefCount = 0;
        }
        
        if (lastWaiting != null && waitingPanelRefCount == 0) {
            lastWaiting.Hide();
            //lastWaiting = null;
        }
    }
    
    public async Task<T> Api<T>(Func<Task<T>> method) {
        while (true) {
            App.mainUI.ShowWaitingPanel();
            var res = await method();
			
            // dummy test
            await new WaitForSeconds(0.5f);
            App.mainUI.HideWaitingPanel();
			
            if (res == null) {
                var key = "common.loading.failed";
                var result = await App.mainUI.ShowAlertKey(key, AlertBoxType.Retry);
                if (result == AlertBoxResult.Ok) {
                    continue;
                } 
            }

            return res;
        }
    }
    
    public async Task<bool> Api(Func<Task<bool>> method) {
        while (true) {
            App.mainUI.ShowWaitingPanel();
            var res = await method();
			
            // dummy test
            await new WaitForSeconds(0.5f);
            App.mainUI.HideWaitingPanel();
			
            if (res == false) {
                var key = "common.loading.failed";
                var result = await App.mainUI.ShowAlertKey(key, AlertBoxType.Retry);
                if (result == AlertBoxResult.Ok) {
                    continue;
                } 
            }

            return res;
        }
    }

    public void RunOnce(IEnumerator method) {
        co.Run(method);
    }
    
    public void Stop(IEnumerator method) {
        co.Stop(method);
    }

    public Vector2 GetSceneSize() {
        return canvasScaler.GetCropSize();
    }

    public Vector2 GetSceneOriginSize() {
        return canvasScaler.GetOriginSize();
    }

    public bool HasPopup() {
        return popups.Count > 0;
    }

    public void AddPopup(IPopupPanel panel) {
        popups.Add(panel);
    }

    public void HideAllPopups() {
        var popupList = popups.ToList();
        foreach (var popup in popupList) {
            popup.Hide();
        }
        popups.Clear();
    }

    public void RemovePopup(IPopupPanel panel) {
        popups.Remove(panel);
    }

    public bool IsTopPopup(IPopupPanel popup) {
        if (popups.Count == 0) {
            return true;
        }

        return popups.Last() == popup;
    }

    public void DefaultRenderFrameInterval(int interval) {
        defaultRenderFrameInterval = interval;
        OnDemandRendering.renderFrameInterval = interval;
    }

    public void AddDemandRendering(string key) {
        if (defaultRenderFrameInterval == 1) {
            return;
        }
        
        onDemandRenderings.Add(key);
        OnDemandRendering.renderFrameInterval = 1;
    }
    
    public void RemoveDemandRendering(string key){
        if (defaultRenderFrameInterval == 1) {
            return;
        }

        onDemandRenderings.Remove(key);
        if (onDemandRenderings.Count == 0) {
            OnDemandRendering.renderFrameInterval = defaultRenderFrameInterval;
        }
    }
}