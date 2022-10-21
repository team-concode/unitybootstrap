using System;
using UnityEngine;

public class ScreenshotCamera : MonoBehaviour {
    private int no = 0;
#if UNITY_EDITOR
    private void Update() {
        if (Input.GetKeyDown(KeyCode.T)) {
            var now = DateTime.Now;
            var path = Application.dataPath + $"/../screenshot/{no++}.png";
            ScreenCapture.CaptureScreenshot(path);
        }
    }
#endif        
}
