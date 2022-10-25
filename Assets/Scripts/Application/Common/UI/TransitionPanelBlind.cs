using System;
using System.Collections;
using UnityEngine;

public class TransitionPanelBlind : TransitionPanel {
    [SerializeField] private RectTransform anchor;

    private Canvas canvas;
    private Coroutine moveRoutine;
    private Vector2 screenSize;

    private void Awake() {
        canvas = GetComponent<Canvas>();
    }

    public override void Display(Action onDone) {
        screenSize = App.mainUI.GetSceneOriginSize();
        canvas.enabled = true;
        anchor.anchoredPosition = new Vector2(0, screenSize.y);
        this.StopCoroutineSafe(moveRoutine);
        moveRoutine = StartCoroutine(MoveTo(-720f, onDone));
    }

    private IEnumerator MoveTo(float toY, Action onDone) {
        var pos = anchor.anchoredPosition;
        var count = Mathf.CeilToInt(Mathf.Abs(toY - pos.y) / 80);
        var offset = pos.y < toY ? 80: -80;
        
        var wait = new WaitForSecondsRealtime(0.01f);
        for (var index = 0; index < count; index++) {
            pos.y += offset;
            anchor.anchoredPosition = pos;
            yield return wait;
        }
        onDone?.Invoke();
    }

    public override void Hide(bool easing, Action onDone) {
        this.StopCoroutineSafe(moveRoutine);
        if (easing == false) {
            canvas.enabled = false;
            return;
        }
        
        moveRoutine = StartCoroutine(MoveTo(screenSize.y, () => {
            canvas.enabled = false;
            onDone?.Invoke();
        }));
    }
}