using UnityEngine;
using UnityEngine.UI;

public class Fps : MonoBehaviour {
    private Text fps;
    private int count;
    private int lastSec;

    private void Awake() {
        fps = GetComponent<Text>();
    }

    private void Update() {
        count++;
        var sec = Mathf.FloorToInt(Time.realtimeSinceStartup);
        if (lastSec == sec) {
            return;
        }
        
        lastSec = sec;
        fps.text = count.ToString();
        count = 0;
    }
}
