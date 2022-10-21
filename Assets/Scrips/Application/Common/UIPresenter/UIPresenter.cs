using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public enum SceneType {
    None,
    Intro = 1,
    Title = 2,
    World = 3,
    Test,
    ActRoomEdit,
}

public delegate void AlertEvent();

public interface UIPresenter {
    SceneType sceneType { get; }
    Canvas canvas { get; }
    GameObject defaultUI { get; }
    GameObject alertLayer { get; }
    GameObject waitingLayer { get; }
    GameObject pushLayer { get; }
    BlinkPanel blinkPanel { get; }
    List<IPopupPanel> popups { get; }
    MyCanvasScaler canvasScaler { get; }
    CommonAlertBox currentCommonAlert { get; set; }
    
    event AlertEvent onCommonAlertStart;
    event AlertEvent onCommonAlertEnd;

    Vector2 GetSceneSize();
    Vector2 GetSceneOriginSize();
    
    Task SwitchScene(SceneType scene, SceneTransitionEffect effect = null);
    Task SwitchPrevScene(SceneTransitionEffect effect = null);
    Task SwitchSceneWithHistoryClear(SceneType scene, SceneTransitionEffect effect = null);
    void HideSceneTransitionEffect();

    Coroutine Run(IEnumerator iterationResult);
    void RunOnce(IEnumerator method);
    void Stop(Coroutine coroutine);
    void Stop(IEnumerator method);

    void ShowBlackPanel(BlackPanelObserver ob);
    void HideBlackPanel(BlackPanelObserver ob);
   
    // deprecated
    IEnumerator ShowAlert(string message, AlertBoxType type, AlertBoxOutResult result);
    IEnumerator ShowAlertKey(string key, AlertBoxType type, AlertBoxOutResult result);
    
    Task<AlertBoxResult> ShowAlert(string message, AlertBoxType type);
    Task<AlertBoxResult> ShowAlertKey(string key, AlertBoxType type);
    void ShowToastAlert(string text);
    void ShowToastAlertKey(string key, params object[] values);
    void ShowPushAlert(string title, string body);
    void ShowWaitingPanel();
    void HideWaitingPanel();
    GoItemRef<PointingLabel> ShowPointingLabel(string message, float duration, RectTransform target, Vector2 offset, PointingDirection dir);

    Task<T> Api<T>(Func<Task<T>> method);
    Task<bool> Api(Func<Task<bool>> method);

    void AddPopup(IPopupPanel panel);
    void RemovePopup(IPopupPanel panel);
    void HideAllPopups();
    bool HasPopup();
    bool IsTopPopup(IPopupPanel panel);

    void DefaultRenderFrameInterval(int interval);
    void AddDemandRendering(string key);
    void RemoveDemandRendering(string key);
}
