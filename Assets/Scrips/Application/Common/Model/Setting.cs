using System.Collections.Generic;

public class SettingPref {
    public bool termsAgreed = false;
    public bool dlcAgreed = false;
}

public enum SettingGraphicQuality {
    None,
    Low,
    Good,
    Best
}

public enum SettingResolution {
    None,
    Normal,
    Wide,
    Wider,
}

public class WdSharedData {
    public SceneType nextScene { get; set; } 
}

public class Setting {
    public string language;
    public SettingGraphicQuality graphicQuality = SettingGraphicQuality.Good;
    public SettingResolution resolution = SettingResolution.Normal;
    public bool bgm = true;
    public bool soundFx = true;
    public float bgmVol = 1f;
    public float soundFxVol = 1f;

    public bool localNotification = true;
    public bool heroAura = true;
    public bool landscape = false;
    public bool shadowEffect = true;
    public bool shortcut = true;
    public int padOpacity = 50;
    public int vibrationIntensity = 75;
    public SettingPref pref;
    public WdSharedData shared = new();

    public Setting() {
        language = StringBundleService.GetLanguageCode();
        pref = new SettingPref();
    }
}