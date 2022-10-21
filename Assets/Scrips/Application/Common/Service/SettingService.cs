using System.Threading.Tasks;
using UnityEngine;

public delegate void SettingEvent(Setting value);

[UnityBean.Service]
public class SettingService {
    public event SettingEvent onSettingUpdate;

    public Setting value { get; internal set; }
    public bool isDev { get; set; }
    
    private bool ready = false; 

    public async Task<bool> Initialize() {
        if (!ready) {
            Load();
            isDev = false;
            ready = true;
        }
        
#if UNITY_STANDALONE
        value.shared.nextScene = SceneType.None;
#endif

        onSettingUpdate = null;
        return true;
    }

    public void Sync(bool fireEvent) {
        Save();
        if (fireEvent) {
            onSettingUpdate?.Invoke(value);
        }
    }

    private void Load() {
        value = LocalSerializer.instance.Load<Setting>(URL.instance.localSetting);
        if (value == null) {
            value = new Setting();
        }

        if (value.pref == null) {
            value.pref = new SettingPref();
        }
    }

    private void Save() {
        LocalSerializer.instance.Save(URL.instance.localSetting, value);
    }

    public void Clear() {
        value = new Setting();
        PersistenceUtil.DeleteFile(URL.instance.localSetting);
    }
}