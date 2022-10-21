using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;

public static class TweenExtensions {
    public static void PlayOnce(this MonoBehaviour v, Coroutine coroutine) {
        if (coroutine != null) {
            v.StopCoroutine(coroutine);
        }
    }

    public static IEnumerator MoveTo(this MonoBehaviour v, EaseType easeType, float duration, Vector3 to) {
        Vector3 from = v.transform.localPosition;

        var ease = new EaseRunner(easeType, duration);
        while (ease.IsPlaying()) {
            v.transform.localPosition = Vector3.Lerp(from, to, ease.Run());
            yield return null;
        }
    }

    public static IEnumerator MoveTo(this Transform v, EaseType easeType, float duration, Vector3 to, Action<float> onEase = null, Action onDone = null, bool scaled = true) {
        Vector3 from = v.transform.localPosition;

        var ease = new EaseRunner(easeType, duration);
        while (ease.IsPlaying()) {
            var value = ease.Run(scaled); 
            v.transform.localPosition = Vector3.Lerp(from, to, value);
            onEase?.Invoke(value);
            yield return null;
        }
        
        onDone?.Invoke();
    }

    public static IEnumerator ScaleTo(this MonoBehaviour v, EaseType easeType, float duration, Vector3 to) {
        Vector3 from = v.transform.localScale;

        var ease = new EaseRunner(easeType, duration);
        while (ease.IsPlaying()) {
            v.transform.localScale = Vector3.Lerp(from, to, ease.Run());
            yield return null;
        }
    }

    public static IEnumerator ScaleTo(this Transform v, EaseType easeType, float duration, Vector3 to, Action onDone = null, Action<float> onUpdate = null) {
        Vector3 from = v.localScale;

        var ease = new EaseRunner(easeType, duration);
        while (ease.IsPlaying()) {
            var value = ease.Run();
            v.localScale = Vector3.Lerp(from, to, value);
            onUpdate?.Invoke(value);
            yield return null;
        }
        
        onDone?.Invoke();
    }
    
    public static IEnumerator ScaleTo(this RectTransform v, EaseType easeType, float duration, Vector2 to, Action onDone = null, Action<float> onUpdate = null) {
        Vector2 from = v.sizeDelta;
        Vector3 localScale = v.localScale;
        if (localScale.x < 0) {
            from.x *= -1;
        }

        var ease = new EaseRunner(easeType, duration);
        while (ease.IsPlaying()) {
            var value = ease.Run();
            var size = Vector2.Lerp(from, to, value);
            if (size.x < 0) {
                size.x *= -1;
                if (localScale.x > 0) {
                    localScale.x *= -1;
                    v.localScale = localScale;
                } 
            } else {
                if (localScale.x < 0) {
                    localScale.x *= -1;
                    v.localScale = localScale;
                }
            }

            v.sizeDelta = size;
            onUpdate?.Invoke(value);
            yield return null;
        }
        
        onDone?.Invoke();
    }    

    public static IEnumerator RotationTo(this MonoBehaviour v, EaseType easeType, float duration, Vector3 to) {
        Vector3 from = v.transform.localEulerAngles;

        var ease = new EaseRunner(easeType, duration);
        while (ease.IsPlaying()) {
            v.transform.localEulerAngles = Vector3.Lerp(from, to, ease.Run());
            yield return null;
        }
    }

    public static IEnumerator MoveTo(this GameObject v, EaseType easeType, float duration, Vector3 to) {
        Vector3 from = v.transform.localPosition;

        var ease = new EaseRunner(easeType, duration);
        while (ease.IsPlaying()) {
            v.transform.localPosition = Vector3.Lerp(from, to, ease.Run());
            yield return null;
        }
    }

    public static IEnumerator MoveTo(this RectTransform v, EaseType easeType, float duration, Vector3 to, bool scaled = false, Action<float> onEase = null, Action onDone = null) {
        Vector3 from = v.anchoredPosition;

        var ease = new EaseRunner(easeType, duration);
        while (ease.IsPlaying()) {
            var value = ease.Run(scaled);
            v.anchoredPosition = Vector3.Lerp(from, to, value);
            onEase?.Invoke(value);
            yield return null;
        }
        onDone?.Invoke();
    }

    public static IEnumerator MoveTo(this RectTransform v, EaseType easeType, Vector2 toPos, float duration, Action<float> onEase, Action onDone) {
        var fromPos = v.anchoredPosition;
        yield return v.RunEase(easeType, duration, (value) => {
            v.anchoredPosition = Vector2.Lerp(fromPos, toPos, value);
            onEase?.Invoke(value);
        });

        onDone?.Invoke();
    }

    public static IEnumerator RunEase(this MonoBehaviour v, EaseType easeType, float duration, Action<float> onEase, Action onDone = null) {
        var ease = new EaseRunner(easeType, duration);
        while (ease.IsPlaying()) {
            onEase(ease.Run());
            yield return null;
        }
        
        onDone?.Invoke();
    }
    
    public static IEnumerator RunEaseUnscaled(this MonoBehaviour v, EaseType easeType, float duration, Action<float> onEase, Action onDone = null) {
        var ease = new EaseRunner(easeType, duration);
        while (ease.IsPlaying()) {
            onEase(ease.Run(false));
            yield return null;
        }
        
        onDone?.Invoke();
    }


    public static IEnumerator WaitDone(this MonoBehaviour v, IEnumerator coroutine, Action onDone = null) {
        yield return coroutine;        
        onDone?.Invoke();
    }

    public static IEnumerator RunBubble(this MonoBehaviour mono, Vector2 magnitude, Action<Vector3> onEase, Action onDone = null, float duration = 0.24f) {
        return mono.RunEase(EaseType.linear, 0.24f, v => {
            if (v < 0.5f) {
                var v2 = v * 2f;
                var x = Mathf.Lerp(1, magnitude.x, Ease.easeInQuad(0, 1, v2));
                var y = Mathf.Lerp(1, magnitude.y, Ease.easeOutQuad(0, 1, v2));
                onEase?.Invoke(new Vector3(x, y, 1));
            } else {
                var v2 = (v - 0.5f) * 2f;
                var x = Mathf.Lerp(magnitude.x, 1, Ease.easeInQuad(0, 1, v2));
                var y = Mathf.Lerp(magnitude.y, 1, Ease.easeOutQuad(0, 1, v2));
                onEase?.Invoke(new Vector3(x, y, 1));
            }
        }, () => {
            onEase?.Invoke(Vector3.one);
            onDone?.Invoke();
        });
    } 

    public static IEnumerator RunEase(this RectTransform v, EaseType easeType, float duration, Action<float> onEase, Action onDone = null, bool scaled = true) {
        var ease = new EaseRunner(easeType, duration);
        while (ease.IsPlaying()) {
            onEase(ease.Run(scaled));
            yield return null;
        }
        
        onDone?.Invoke();
    }

    public static IEnumerator ScaleTo(this GameObject v, EaseType easeType, float duration, Vector3 to, bool scaled = true) {
        Vector3 from = v.transform.localScale;

        var ease = new EaseRunner(easeType, duration);
        while (ease.IsPlaying()) {
            v.transform.localScale = Vector3.Lerp(from, to, ease.Run(scaled));
            yield return null;
        }
    }

    public static IEnumerator RotationTo(this GameObject v, EaseType easeType, float duration, Vector3 to) {
        Vector3 from = v.transform.localEulerAngles;

        var ease = new EaseRunner(easeType, duration);
        while (ease.IsPlaying()) {
            v.transform.localEulerAngles = Vector3.Lerp(from, to, ease.Run());
            yield return null;
        }
    }

    public static IEnumerator ValueTo(this Slider v, EaseType easeType, float duration, float to) {
        float from = v.value;

        var ease = new EaseRunner(easeType, duration);
        while (ease.IsPlaying()) {
            v.value = Mathf.Lerp(from, to, ease.Run());
            yield return null;
        }
    }

    public static IEnumerator CameraRectTo(this Camera v, EaseType easeType, float duration, Rect to) {
        Rect from = v.rect;

        var ease = new EaseRunner(easeType, duration);
        while (ease.IsPlaying()) {
            Rect rect = new Rect();
            float process = ease.Run();
            rect.xMin = Mathf.Lerp(from.xMin, to.xMin, process);
            rect.yMin = Mathf.Lerp(from.yMin, to.yMin, process);
            rect.xMax = Mathf.Lerp(from.xMax, to.xMax, process);
            rect.yMax = Mathf.Lerp(from.yMax, to.yMax, process);
            v.rect = rect;
            yield return null;
        }
    }

    public static IEnumerator TintColorTo(this Material v, EaseType easeType, float duration, Color to) {
        Color from = v.GetColor("_TintColor");

        var ease = new EaseRunner(easeType, duration);
        while (ease.IsPlaying()) {
            Color color = Color.Lerp(from, to, ease.Run());
            v.SetColor("_TintColor", color);
            yield return null;
        }
    }

    public static IEnumerator AlphaTo(this CanvasGroup v, EaseType easeType, float duration, float to, Action onDone = null, bool scaled = false) {
        var from = v.alpha;
        var ease = new EaseRunner(easeType, duration);
        while (ease.IsPlaying()) {
            v.alpha = Mathf.Lerp(from, to, ease.Run(scaled));
            yield return null;
        }
        
        onDone?.Invoke();
    }
    
    public static IEnumerator AlphaTo(this SpriteRenderer v, EaseType easeType, float duration, float to) {
        var fromColor = v.color;
        var toColor = v.color;
        toColor.a = to;

        var ease = new EaseRunner(easeType, duration);
        while (ease.IsPlaying()) {
            v.color = Color.Lerp(fromColor, toColor, ease.Run());
            yield return null;
        }
    }
    
    public static IEnumerator ColorTo(this Graphic v, EaseType easeType, float duration, Color to) {
        Color from = v.color;

        var ease = new EaseRunner(easeType, duration);
        while (ease.IsPlaying()) {
            v.color = Color.Lerp(from, to, ease.Run());
            yield return null;
        }
    }
    
    public static IEnumerator AlphaTo(this Graphic v, EaseType easeType, float duration, float to) {
        Color fromColor = v.color;
        Color toColor = v.color;
        toColor.a = to;

        var ease = new EaseRunner(easeType, duration);
        while (ease.IsPlaying()) {
            v.color = Color.Lerp(fromColor, toColor, ease.Run());
            yield return null;
        }
    }

    public static IEnumerator WaveIt(this RectTransform v, int count, float power, float duration) {
        float waveTime = 0;
        Vector3 originPosition = v.anchoredPosition;
        
        while (waveTime < duration) {
            float process = waveTime / duration;
            float value = Mathf.Sin(2 * Mathf.PI * process * count);
            float radius = value * power;
            
            v.anchoredPosition = Vector3.up * radius + originPosition;
            waveTime += Time.deltaTime;
            yield return null;
        }

        v.anchoredPosition = originPosition;
    }

    
#if BOOT_NGUI_SUPPORT
    public static IEnumerator AlphaTo(this UIPanel v, EaseType easeType, float duration, float to) {
        float ingredient = v.alpha;

        var ease = new EaseRunner(easeType, duration);
        while (ease.IsPlaying()) {
            v.alpha = Mathf.Lerp(ingredient, to, ease.Run());
#if UNITY_EDITOR
            NGUITools.SetDirty(v);
#endif
            yield return null;
        }
    }

    public static IEnumerator ColorTo(this UIWidget v, EaseType easeType, float duration, Color to) {
        Color ingredient = v.color;

        var ease = new EaseRunner(easeType, duration);
        while (ease.IsPlaying()) {
            v.color = Color.Lerp(ingredient, to, ease.Run());
#if UNITY_EDITOR
            NGUITools.SetDirty(v);
#endif
            yield return null;
        }
    }

    public static IEnumerator AlphaTo(this UIWidget v, EaseType easeType, float duration, float to) {
        Color ingredient = v.color;
        Color toClolor = v.color;
        toClolor.a = to;

        var ease = new EaseRunner(easeType, duration);
        while (ease.IsPlaying()) {
            v.color = Color.Lerp(ingredient, toClolor, ease.Run());
#if UNITY_EDITOR
            NGUITools.SetDirty(v);
#endif
            yield return null;
        }
    }
#endif

    public static IEnumerator TweenBounce(this MonoBehaviour v) {
        yield return v.ScaleTo(EaseType.easeOutQuad, 0.1f, Vector3.one * 0.9f);
        yield return v.ScaleTo(EaseType.easeInQuad, 0.1f, Vector3.one * 1.08f);
        yield return v.ScaleTo(EaseType.easeOutQuad, 0.1f, Vector3.one * 0.94f);
        yield return v.ScaleTo(EaseType.easeInQuad, 0.1f, Vector3.one * 1.02f);
        yield return v.ScaleTo(EaseType.easeOutQuad, 0.1f, Vector3.one * 1.0f);
    }
}