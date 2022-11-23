using System.Collections;
using UnityBean;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour {
    [LazyWired] private SettingService settingService;
    
    private IEnumerator Start() {
        yield return null;
        BeanContainer.LazyDI(this);
        UpdateDPI();
        yield return null;
        var scene = UIPresenterBase.GetNextScene();
        SceneManager.LoadScene(scene.ToString());
    }
    
    private void UpdateDPI() {
#if UNITY_ANDROID
        if (settingService == null) {
            return;
        }

        var factor = 1f;
        if (settingService.value.graphicQuality == SettingGraphicQuality.Low) {
            factor = 150f / 330f;
        } else if (settingService.value.graphicQuality == SettingGraphicQuality.Good) {
            factor = 240f / 330f;
        }

        QualitySettings.resolutionScalingFixedDPIFactor = factor;
#endif
    }
}