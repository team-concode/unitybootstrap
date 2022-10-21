using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityBean;
using UnityEngine;
using UnityEngine.UI;

public enum PointingDirection {
    UpSide,
    DownSide,
    LeftSide
}

public class PointingLabel : GoItem {
    [SerializeField] private Text message;
    [SerializeField] private TextOutFitterLimit textFitter;
    [SerializeField] private Transform pointer;
    
    private CanvasGroup panel;
    private bool display; 
    private RectTransform rect;
    private RectTransform target;
    private Vector2 offset;
    private PointingDirection dir;

    [LazyWired] private GoPooler goPooler;

    private void Awake() {
        BeanContainer.LazyDI(this);
        rect = GetComponent<RectTransform>();
        panel = GetComponent<CanvasGroup>();
    }

    public async void Display(string text, float duration, RectTransform target, Vector2 offset, PointingDirection dir) {
        display = true; 
        this.gameObject.SetActive(true);
        this.target = target;
        this.offset = offset;
        this.dir = dir;
        SetParent(target);

        panel.alpha = 0;
        message.text = text;
        
        await new WaitForEndOfFrame();
        textFitter.RefreshSize();
        await new WaitForEndOfFrame();
        
        panel.alpha = 1;

        ArrangePosition();
        if (duration > 0) {
            App.mainUI.Run(AutoHide(duration));
        }        
    }

    public void ArrangePosition() {
        var textSize = textFitter.GetSize();
        var targetSize = target.sizeDelta;
        var screenSize = App.mainUI.GetSceneOriginSize();
        Vector2 position = target.rect.center;
        position += offset;
        var rectTrans = textFitter.gameObject.GetComponent<RectTransform>();

        switch(dir) {
            case PointingDirection.UpSide:
                rectTrans.pivot = new Vector2(rectTrans.pivot.x, 1);
                position.y -= targetSize.y * 0.5f + 16;
                break;
            case PointingDirection.DownSide:
                rectTrans.pivot = new Vector2(rectTrans.pivot.x, 0);
                position.y += targetSize.y * 0.5f + 16;
                break;
            case PointingDirection.LeftSide:
                rectTrans.pivot = new Vector2(rectTrans.pivot.x, 0.5f);
                position.x -= targetSize.x * 0.5f + 16;
                break;
        }

        var defaultUI = App.mainUI.defaultUI.GetComponent<RectTransform>();
        var worldPos = target.localToWorldMatrix.MultiplyPoint(position);
        var pt = defaultUI.ToLocalPosition(worldPos);
        var textPos = Vector2.zero;

        switch(dir) {
            case PointingDirection.UpSide:
                textPos = new Vector2(0, -14);
                break;
            case PointingDirection.DownSide:
                textPos = new Vector2(0, 14);
                break;
            case PointingDirection.LeftSide:
                textPos = new Vector2(50, 0);
                break;
        }
        
        float lhs = pt.x - textSize.x / 2;
        float rhs = pt.x + textSize.x / 2;
        float leftMin = -screenSize.x * 0.5f + 32;
        float rightMax = screenSize.x * 0.5f - 32;

        float screenOffset = 0;
        if (lhs < leftMin) {
            screenOffset = Mathf.Abs(lhs - leftMin);
        }
        if (rhs > rightMax) {
            screenOffset = -Mathf.Abs(rhs - rightMax);
        }

        float minOffset = Mathf.Max(0, textSize.x * 0.5f - 40);
        if (screenOffset > minOffset) {
            screenOffset = minOffset;
        } else if (screenOffset < -minOffset) {
            screenOffset = -minOffset;
        }

        textPos.x += screenOffset; 
        rect.anchorMax = target.anchorMax;
        rect.anchorMin = target.anchorMin;
        rect.pivot = target.pivot;
        rect.anchoredPosition = position + target.anchoredPosition;

        textFitter.SetPosition(textPos);

        switch(dir) {
            case PointingDirection.UpSide:
                pointer.localPosition = new Vector3(pointer.localPosition.x, 0, pointer.localPosition.z);
                break;
            case PointingDirection.DownSide:
                pointer.localPosition = new Vector3(pointer.localPosition.x, -0, pointer.localPosition.z);
                break;
        }
    }

    public void Hide() {
        goPooler.Return(this);
    }
    
    private void SetParent(RectTransform target) {
        var parent = target.transform.parent;
        var index = target.transform.GetSiblingIndex();

        if (transform.parent != parent) {
            transform.SetParent(parent);
        }

        transform.localScale = Vector3.one;
        transform.SetSiblingIndex(index + 1);
    }

    private IEnumerator AutoHide(float duration) {
        float past = 0;
        while (display) {
            past += Time.deltaTime;
            if (past > duration) {
                break;
            }
            yield return null;
        }
        Hide();
    }
}

