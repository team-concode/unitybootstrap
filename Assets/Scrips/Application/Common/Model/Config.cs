
using System.Collections.Generic;

public class ConfigMinVersion {
    public string ios { get; set; }
    public string android { get; set; }
}

public class ConfigDownloadUrl {
    public string ios { get; set; }
    public string android { get; set; }
}

public class ConfigUrl {
    public string homepage { get; set; }
}

public class ConfigConnection {
    public string apiServer { get; set; }
    public string shopServer { get; set; }
}

public class ConfigNoticeContent {
    public string lang { get; set; }
    public string title { get; set; }
    public string body { get; set; }
}

public class ConfigMaintenance {
    public bool on { get; set; }
    public List<ConfigNoticeContent> messages { get; set; } 
}

public class Config {
    public ConfigMinVersion minVersion { get; set; }
    public ConfigMaintenance maintenance { get; set; }
    public ConfigDownloadUrl downloadUrl { get; set; }
    public ConfigUrl url { get; set; }
    public List<string> features { get; set; }
    public ConfigConnection connections { get; set; }
    
    public string GetDownloadUrl() {
#if UNITY_IOS
        return downloadUrl.ios;
#elif UNITY_ANDROID
        return downloadUrl.android;
#else
        return "";
#endif
    }

    public bool IsFeatureOn(string feature) {
        if (features == null) {
            return false;
        }

        foreach (var item in features) {
            if (feature == item) {
                return true;
            }
        }

        return false;
    }
}