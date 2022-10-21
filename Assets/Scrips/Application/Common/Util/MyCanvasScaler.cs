using System;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteInEditMode]
public class MyCanvasScaler : MonoBehaviour {
    [SerializeField] private Canvas canvas; 
    [SerializeField] private RectTransform defaultUI; 
    [SerializeField] private float _minWidth = 720; 
    [SerializeField] private float _maxWidth = 720; 
    [SerializeField] private float _minHeight = 1280; 
    [SerializeField] private float _maxHeight = 1400;
    [SerializeField] private float standAloneScaler = 1f;
    
    [SerializeField] private bool _overMax = false;
    [SerializeField] private float _safeOverMax = 40;

    private float minWidth => _minWidth * GetScaler();
    private float maxWidth => _maxWidth * GetScaler();
    private float minHeight => _minHeight * GetScaler();
    private float maxHeight => _maxHeight * GetScaler();

    private Vector2 cropSize;
    private Vector2 originSize;
    private DeviceOrientation orientation;

    public EventBusTopic onChangeOrientation = new();

    private void Awake() {
        orientation = Input.deviceOrientation;
        Refresh();
    } 

    private void Start() {
        if (Application.isPlaying) {
            this.RunNextFrame(Refresh);
        }
    }

    public void SetMinSize(Vector2 size) {
        _minWidth = size.x;
        _minHeight = size.y;
    }

    public void SetMaxSize(Vector2 size) {
        _maxWidth = size.x;
        _maxHeight = size.y;
    }

    public void SetStandAloneScaler(float scaler) {
        standAloneScaler = scaler;
    }

    private void Refresh() {
        if (canvas == null || defaultUI == null) {
            return;
        }

        if (float.IsNaN(Screen.width) || float.IsNaN(Screen.height)) {
            return;
        }

        if (Screen.width == 0 || Screen.height == 0) {
            return;
        }

        var scalerW = minWidth / Screen.width;
        var scalerH = minHeight / Screen.height;
        var scaler = Mathf.Max(scalerW, scalerH);
        var size = new Vector2(Screen.width * scaler, Screen.height * scaler);
        cropSize = originSize = size;

        var mh = _maxHeight;
        if (_overMax) {
            mh = size.y - _safeOverMax * 2;
            mh = Mathf.Max(_maxHeight, mh);
        }

        cropSize.x = Mathf.Min(maxWidth, size.x);
        cropSize.y = Mathf.Min(mh, size.y);
        canvas.scaleFactor = 1f / scaler;
        
        defaultUI.anchoredPosition = Vector3.zero;
        defaultUI.sizeDelta = cropSize;

        AdjustOffst(cropSize, scaler);
    }

    private void AdjustOffst(Vector2 size, float scaler) {
#if UNITY_IOS
        return;
#else
        float height = minWidth * (Screen.height / (float)Screen.width);
        if (height < minHeight) {
            height = minHeight;
        }

        var cutouts = Screen.cutouts;
        var maxHole = 0f;
        foreach (var cutout in cutouts) {
            var hole = cutout.height / scaler;
            maxHole = Mathf.Max(hole, maxHole);
        }

        var available = height - maxHole;
        var diff = Mathf.Max(0, size.y - available);
        var offset = Vector2.zero;

        if (diff > 0) {
            size.y -= diff;
            offset.y -= diff * 0.5f;

            defaultUI.sizeDelta = size;
            defaultUI.anchoredPosition = offset;
        }
#endif
    }

    public Vector2 GetCropSize() {
        if (cropSize.x <= 0) {
            Refresh();
        }

        return cropSize;
    } 

    public Vector2 GetOriginSize() {
        if (originSize.x <= 0) {
            Refresh();
        }

        return originSize;
    } 

    public float GetScale() {
        var size = GetOriginSize();
        return size.x / Screen.width;
    }

    private float GetScaler() {
        #if UNITY_STANDALONE || UNITY_WEBGL
        return standAloneScaler;
        #else 
        return 1;
        #endif
    }

    private void Update() {
        #if UNITY_EDITOR
        Refresh();
        #endif

        if (Input.deviceOrientation != orientation) {
            this.orientation = Input.deviceOrientation;
            Refresh();
            onChangeOrientation.Fire();
        }
    }
}
