using UnityEngine;

public class GoSfx : GoItem {
    [SerializeField] private AudioSource _source;

    public AudioSource source => _source;

    public void Play() {
        source.time = 0;
        source.Play();
        this.RunAfter(source.clip.length, () => {
            this.pool.Return(this);
        });
    }

    public void Stop() {
        source.Stop();
        this.pool.Return(this);
    }
}