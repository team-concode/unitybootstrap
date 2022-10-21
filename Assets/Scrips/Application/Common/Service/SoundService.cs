using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityBean;

[Service]
public class SoundService {
    [AutoWired] private SettingService settingService;
    [AutoWired] private GoPooler goPooler;
    
    private Dictionary<string, FxClip> clips = new();
    private AudioSource currentBgm;
    private AudioSource currentWeather;
    private AudioListener listener;
    private GoContainer container;
    private float effectVolume = 0.5f;
    
    public string currentBgmPath { get; private set; }
    public bool isReady;
    
    private Queue<AudioSource> sourcePool = new();

    public async Task<bool> Initialize() {
        sourcePool.Clear();
        if (!isReady) {
            if (Application.isPlaying) {
                container = GoContainer.New("Sound", true);
                var go = container.gameObject;
                listener = go.AddComponent<AudioListener>();
                container = go.AddComponent<GoContainer>();
                GameObject.DontDestroyOnLoad(go);
            }
            isReady = true;
        }

        currentBgm = null;
        currentWeather = null;
        currentBgmPath = "";
        return true;
    }

    private AudioSource GetSource() {
        if (sourcePool.Count > 0) {
            var source = sourcePool.Dequeue();
            source.gameObject.SetActive(true);
            return source;
        }
        
        var container = GoContainer.New("Source", false);
        var go = container.gameObject;
        go.transform.parent = container.transform;
        return go.AddComponent<AudioSource>();
    }

    public void PlayBGM(string path, float volume = 0.5f) {
        if (settingService.value?.bgm == false) {
            return;
        }

        if (currentBgmPath == path) {
            return;
        }

        if (currentBgm != null) {
            StopBGM();
        }

        var source = GetSource();
        var clip = Resources.Load<AudioClip>("Sound/" + path);
        source.clip = clip;
        source.pitch = 1f;
        source.loop = true;
        source.volume = volume;
        source.Play();

        currentBgm = source;
        currentBgmPath = path;
    }
    
    public void StopBGM() {
        if (currentBgm == null) {
            return;
        }
        
        currentBgmPath = "";
        container.StartCoroutine(StopSource(currentBgm, 0.5f));
        currentBgm = null;
    }

    public void PauseBGM() {
        if (currentBgm != null) {
            currentBgm.Pause();
        }
    }

    public void ResumeBGM() {
        if (currentBgm != null) {
            currentBgm.UnPause();
        }
    }

    public void PlayWeather(string path, float volume = 0.5f) {
        if (settingService.value?.bgm == false) {
            return;
        }

        if (currentWeather != null) {
            StopWeather();
        }

        var source = GetSource();
        var clip = Resources.Load<AudioClip>("Sound/" + path);
        source.clip = clip;
        source.pitch = 1f;
        source.loop = true;
        source.volume = volume;
        source.Play();
        currentWeather = source;
    }

    public void StopWeather() {
        if (currentWeather == null) {
            return;
        }
        
        container.StartCoroutine(StopSource(currentWeather, 0.5f));
        currentWeather = null;
    }

    private IEnumerator StopSource(AudioSource source, float duration) {
        var from = source.volume;
        yield return container.RunEaseUnscaled(EaseType.easeInQuad, duration, v => {
            source.volume = Mathf.Lerp(from, 0, v);
        });

        source.Stop();
        source.gameObject.SetActive(false);
        sourcePool.Enqueue(source);
    }

    public GoSfx PlayFx(string name, bool loop = false, float volume = 0.5f, bool eco = true) {
        if (string.IsNullOrEmpty(name)) {
            return null;
        }
        
        if (settingService.value?.soundFx == false) {
            return null;
        }

        FxClip fx;
        if (clips.ContainsKey(name)) {
            fx = clips[name];
        } else {
            fx = new FxClip();
            fx.clip = Resources.Load<AudioClip>("Sound/" + name);
            if (fx.clip == null) {
                Debug.LogError("can not load effect sound:" + name);
                return null;
            }
            
            clips.Add(name, fx);
        }

        var now = Time.realtimeSinceStartup;
        if (eco && now - fx.fired < 0.05f) {
            return null;
        }

        fx.fired = now;
        var sfx = NewSource();
        sfx.source.clip = fx.clip;
        sfx.source.volume = volume;

        if (loop) {
            sfx.source.loop = true;
            sfx.source.Play();
        } else {
            sfx.source.loop = false;
            sfx.Play();
        }

        return sfx;
    }

    private GoSfx NewSource() {
        return goPooler.Get<GoSfx>("Prefabs/Sound/GoSfx", container.gameObject);
    }
}
