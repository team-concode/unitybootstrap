using UnityEngine;

public class FxClip {
    public float fired { get; set; }
    public float length => clip?.length ?? 0;
    public AudioClip clip { get; set; }
}