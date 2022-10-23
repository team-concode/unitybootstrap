using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ScreenshotCamera : MonoBehaviour {
    private int no = 0;
#if UNITY_EDITOR
    private void Update() {
        if (Keyboard.current.tKey.wasPressedThisFrame) {
            var now = DateTime.Now;
            var path = Application.dataPath + $"/../Screenshot/{no++}.png";
            ScreenCapture.CaptureScreenshot(path);
        }
    }
#endif        
}
