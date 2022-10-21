using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DualSpriteRenderer : MonoBehaviour {
    [SerializeField] private Material _material;
    [SerializeField] private int renderQueue = 3001;

    private void Start() {
        var sprite = GetComponent<SpriteRenderer>();
        var origin = sprite.material;
        var materials = new Material[2];
        materials[0] = origin;
        materials[1] = _material;
        _material.renderQueue = renderQueue;
        sprite.materials = materials;
    }
}