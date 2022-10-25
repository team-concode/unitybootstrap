using UnityEngine;
using UnityEngine.UI;
using System;

public class Radio : MonoBehaviour {
    [SerializeField] private Color offColor;
    [SerializeField] private Color onColor;
    [SerializeField] private Image background;
    [SerializeField] private Image core;

    private bool isChecked;
    private Action<bool> onChanged;
    private Coroutine changeRoutine;

    public void Display(bool isChecked, Action<bool> onChanged) {
        this.onChanged = onChanged;
        SetChecked(isChecked, false);
    }

    private void SetChecked(bool on, bool easing) {
        isChecked = on;
        var fromC = background.color;
        var toC = on ? onColor : offColor;
        var fromPos = core.rectTransform.anchoredPosition;
        var toPos = on ? new Vector2(20, 0) : new Vector2(-20, 0);

        if (easing == false) {
            background.color = toC;
            core.rectTransform.anchoredPosition = toPos;
            return;
        }
        
        this.StopCoroutineSafe(changeRoutine);
        changeRoutine = StartCoroutine(this.RunEaseUnscaled(EaseType.easeInOutQuad, 0.2f, (v) => {
            background.color = Color.Lerp(fromC, toC, v);
            core.rectTransform.anchoredPosition = Vector2.Lerp(fromPos, toPos, v);
        }));
    }

    public void OnClickItem() {
        SetChecked(!isChecked, true);
        onChanged?.Invoke(isChecked);
    }
}