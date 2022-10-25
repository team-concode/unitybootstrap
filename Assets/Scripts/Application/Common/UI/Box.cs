using UnityEngine;

public class Box : MonoBehaviour {
    public Vector2 areaPadding;
    
    public float width { get; protected set; }
    public float height { get; protected set; }

    public bool Contains(float x, float y) {
        var position = transform.localPosition;
        float xMin = -width * 0.5f + position.x;
        float xMax = width * 0.5f + position.x;
        float yMin = -height * 0.5f + position.y;
        float yMax = height * 0.5f + position.y;

        if (x < xMin || xMax < x) {
            return false;
        }
        if (y < yMin || yMax < y) {
            return false;
        }

        return true;
    }

    private void OnEnable() {
        BoxAgency.instance.Add(this);
    }

    private void OnDisable() {
        BoxAgency.instance.Remove(this);
    }

    public virtual void Arrange() {
        
    }
}